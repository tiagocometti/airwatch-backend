# AirWatch — Backend

## Stack
- .NET (C#) — Clean Architecture: Domain, Application, Infrastructure, Api
- Entity Framework Core + PostgreSQL (Npgsql)
- Autenticação JWT + BCrypt para hash de senhas
- MQTTnet para comunicação com broker HiveMQ Cloud (TLS, porta 8883)
- SignalR para eventos em tempo real ao frontend

## Estrutura de projetos
```
AirWatch.Domain/          # Entidades, interfaces, enums
AirWatch.Application/     # Use cases, DTOs, serviços de aplicação
AirWatch.Infrastructure/  # EF Core, repositórios, serviços externos, background services
AirWatch.Api/             # Controllers, middleware, configuração de DI
```

## Background Services (rodam em paralelo)
- `MqttSubscriberService` — conecta ao broker, assina três tópicos e roteia mensagens:
  - `airwatch/+/telemetry` — CSV `deviceId,adc_mq3,adc_mq5,adc_mq135`; calcula PPMs e persiste (descarta se não houver calibração ativa)
  - `airwatch/+/calibration` — mesmo CSV durante modo de calibração; encaminha para `ICalibrationSampleHandler`
  - `airwatch/devices/+/status` — birth messages (`online`) e LWT (`offline`) do ESP
- `CalibrationBackgroundService` — singleton que implementa `IHostedService`, `ICalibrationManager` e `ICalibrationSampleHandler`:
  - Mantém sessões ativas em `ConcurrentDictionary<string, CalibrationSession>` keyed por `ExternalId`
  - `DuracaoSegundos` define a duração de cada sessão (constante no serviço, gravada na tabela por calibração)
  - Ao encerrar o timer: calcula média dos Rs amostrados → grava R0 → status `Completed`. Falha apenas com 0 amostras.
  - `UserCts` por sessão: cancelado exclusivamente via `/cancel`. Safety timer de 120s guarda pelo `CalibrationId` para não cancelar nova sessão do mesmo device.
  - No startup: calibrações `InProgress` no banco são marcadas como `Failed`.

## Banco de dados — tabelas principais
- `users` — autenticação
- `devices` — `IsOnline`, `LastSeen`, `ExternalId`, `IsActive`, `RlMq3/5/135` (RL de carga do hardware; R0 **não** fica aqui)
- `measurements` — uma linha por ciclo: `Mq3Adc`, `Mq5Adc`, `Mq135Adc`, `PpmAlcohol`, `PpmLpg`, `PpmCo2`, `PpmNh3`, `Timestamp`
- `sensor_coefficients` — coeficientes de curva por sensor+gás: `SensorType`, `GasTarget`, `CoefA`, `CoefB`, `SafeMax`, `GoodMax`, `AlertMax`
- `calibrations` — histórico de calibrações:
  - `Id`, `DeviceId`, `StartedAt`, `CompletedAt`, `Status` (`InProgress`/`Completed`/`Cancelled`/`Failed`)
  - `Location`, `SampleCount`, `DuracaoSegundos`
  - `R0Mq3`, `R0Mq5`, `R0Mq135` — médias das resistências Rs em ar limpo
  - `IsActive` — apenas uma ativa por device (partial unique index `WHERE "IsActive" = TRUE`)

## MQTT — broker HiveMQ Cloud
```
Host:  0160eb21063349f3a226443abf16e94e.s1.eu.hivemq.cloud
Porta: 8883 (TLS)
User:  airwatch
```
Estrutura de tópicos (todos implementados):
- `airwatch/{externalId}/telemetry` — CSV publicado pelo ESP no modo normal
- `airwatch/{externalId}/calibration` — CSV publicado pelo ESP durante modo de calibração
- `airwatch/{externalId}/commands` — backend → ESP: `start_calibration` ou `stop_calibration`
- `airwatch/devices/{externalId}/status` — presença do dispositivo (`online` / `offline`, retain=true)

## Cadeia de cálculo (ADC → PPM)
O backend é o único responsável por todos os cálculos:
1. `ADC → VRL`:  `vrl = adc * (5.0 / 1023)`
2. `VRL → Rs`:   `rs = ((5.0 / vrl) - 1.0) * rl`  (RL por sensor em `devices`)
3. `Rs → ratio`: `ratio = rs / r0`  (R0 vem da calibração ativa em `calibrations`; **sem calibração ativa → medição descartada**)
4. `ratio → PPM`: `ppm = coefA * ratio^coefB`  (coeficientes em `sensor_coefficients`)

- MQ3 → Álcool (coef: a=0.3934, b=-1.5040)
- MQ5 → GLP (coef: a=217.4972, b=-2.4221)
- MQ135 → CO₂ (coef: a=110.47, b=-2.862) e NH₃ (coef: a=102.2, b=-2.473)

## SignalR — eventos emitidos pelo hub `/hubs/device-status`
- `DeviceStatusChanged` — status online/offline de dispositivo
- `NewMeasurement` — nova medição com os quatro PPMs
- `CalibrationStarted` — `{ deviceId, calibrationId, startedAt, duracaoSegundos }`
- `CalibrationProgress` — `{ deviceId, calibrationId, progressPercent, sampleCount, currentR0Mq3, currentR0Mq5, currentR0Mq135 }`
- `CalibrationCompleted` — `{ deviceId, calibrationId, r0Mq3, r0Mq5, r0Mq135 }`
- `CalibrationFailed` — `{ deviceId, calibrationId, reason }`
- `CalibrationCancelled` — `{ deviceId, calibrationId }`

## API — endpoints de calibração
- `POST /api/calibrations/start` — body: `{ deviceId, location }` → inicia sessão
- `POST /api/calibrations/{id}/cancel` — cancela calibração em andamento
- `POST /api/calibrations/{id}/activate` — ativa calibração `Completed` como referência de R0
- `GET  /api/calibrations/device/{deviceId}` — histórico de calibrações do dispositivo
- `GET  /api/calibrations/device/{deviceId}/active` — calibração ativa atual (204 se nenhuma)

## Registro de DI (singletons compartilhados)
```csharp
services.AddSingleton<CalibrationBackgroundService>();
services.AddHostedService(sp => sp.GetRequiredService<CalibrationBackgroundService>());
services.AddSingleton<ICalibrationManager>(sp => sp.GetRequiredService<CalibrationBackgroundService>());
services.AddSingleton<ICalibrationSampleHandler>(sp => sp.GetRequiredService<CalibrationBackgroundService>());
```

## Convenções
- Clean Architecture: regras de negócio em Domain/Application, nunca em Infrastructure ou Api
- DTOs para entrada e saída nas controllers — nunca expor entidades diretamente
- Repositórios sempre via interface — nunca instanciar `DbContext` fora de Infrastructure
- SignalR: nomes de eventos devem ser idênticos aos consumidos pelo frontend
- Background services singleton resolvem serviços scoped (DbContext) via `IServiceScopeFactory`

## Dev sem hardware
- Telemetria normal: publicar `arduino-01,293,89,118` em `airwatch/arduino-01/telemetry` (descartada sem calibração ativa)
- Calibração: publicar mesmo CSV em `airwatch/arduino-01/calibration` com sessão ativa em `_sessions`
- Status: publicar `online`/`offline` em `airwatch/devices/arduino-01/status` com retain=true
- O MqttSimulatorService foi removido intencionalmente — não recriar

## API — endpoint de thresholds (público, sem autenticação)
- `GET /api/sensor-coefficients/thresholds` — retorna `[{ gasTarget, safeMax, goodMax, alertMax }]` para os 4 gases

## Funcionalidades planejadas (ainda não implementadas)
- Alertas ao usuário quando concentração perigosa detectada
- Ativação/desativação de dispositivos pelo frontend
