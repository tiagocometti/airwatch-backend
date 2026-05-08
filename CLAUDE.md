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
- `MqttSubscriberService` — único background service MQTT. Assina dois tópicos:
  - `airwatch/+/telemetry` — recebe CSV `deviceId,adc_mq3,adc_mq5,adc_mq135`, calcula PPMs e persiste
  - `airwatch/devices/+/status` — processa birth messages (`online`) e LWT (`offline`) publicados pelo ESP

## Banco de dados — tabelas principais
- `users` — autenticação
- `devices` — `IsOnline`, `LastSeen`, `ExternalId`, `IsActive` + `RlMq3/5/135`, `R0Mq3/5/135`
- `measurements` — uma linha por ciclo: `Mq3Adc`, `Mq5Adc`, `Mq135Adc`, `PpmAlcohol`, `PpmLpg`, `PpmCo2`, `PpmNh3`, `Timestamp`
- `sensor_coefficients` — coeficientes de curva por sensor+gás: `SensorType`, `GasTarget`, `CoefA`, `CoefB`, `RatioMin`, `RatioMax`

## MQTT — broker HiveMQ Cloud
```
Host:  0160eb21063349f3a226443abf16e94e.s1.eu.hivemq.cloud
Porta: 8883 (TLS)
User:  airwatch
```
Estrutura de tópicos:
- `airwatch/{externalId}/telemetry` — telemetria (CSV publicado pelo ESP; backend assina `airwatch/+/telemetry`)
- `airwatch/devices/{externalId}/status` — presença do dispositivo (`online` / `offline`, retain=true)

Tópicos planejados (ainda não implementados):
- `airwatch/devices/{externalId}/commands` — comandos do backend para o ESP
- `airwatch/devices/{externalId}/config` — configurações remotas

## Cadeia de cálculo (ADC → PPM)
O backend é o único responsável por todos os cálculos:
1. `ADC → VRL`:  `vrl = adc * (5.0 / 1023)`
2. `VRL → Rs`:   `rs = ((5.0 / vrl) - 1.0) * rl`  (RL por sensor em `devices`)
3. `Rs → ratio`: `ratio = rs / r0`                  (R0 temporário em `devices`)
4. `ratio → PPM`: `ppm = coefA * ratio^coefB`        (coeficientes em `sensor_coefficients`)

- MQ3 → Álcool (coef: a=0.3934, b=-1.5040)
- MQ5 → GLP (coef: a=217.4972, b=-2.4221)
- MQ135 → CO₂ (coef: a=110.47, b=-2.862) e NH₃ (coef: a=102.2, b=-2.473)

R0 default: MQ3=25000Ω, MQ5=105000Ω, MQ135=76630Ω (temporário — será revisado na calibração)

## SignalR — eventos
- `DeviceStatusChanged` — status online/offline de dispositivo
- `NewMeasurement` — nova medição com os quatro PPMs (emitido após cada telemetria processada)

## Convenções
- Seguir Clean Architecture: regras de negócio ficam em Domain/Application, nunca em Infrastructure ou Api
- DTOs para entrada e saída de dados nas controllers — nunca expor entidades diretamente
- Sempre usar repositórios via interface (nunca instanciar DbContext direto fora de Infrastructure)
- SignalR emite eventos nomeados — manter consistência com os nomes que o frontend consome

## Dev sem hardware
- Para simular telemetria: publicar CSV `arduino-01,293,89,118` no tópico `airwatch/arduino-01/telemetry` via Web Client do HiveMQ
- Para simular status: publicar `online` ou `offline` no tópico `airwatch/devices/arduino-01/status` com retain=true via Web Client do HiveMQ
- O MqttSimulatorService foi removido intencionalmente — não recriar

## Funcionalidades planejadas (ainda não implementadas)
- Alertas ao usuário quando concentração perigosa detectada
- Comandos remotos ao Arduino via MQTT
- Calibração e gerenciamento de R0 (valores atuais são temporários)
- Ativação/desativação de dispositivos pelo frontend
