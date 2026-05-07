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
  - `airwatch/sensors` — persiste medições e atualiza `LastSeen` do dispositivo
  - `airwatch/devices/+/status` — processa birth messages (`online`) e LWT (`offline`) publicados pelo ESP; atualiza `IsOnline` no banco e emite evento SignalR

## Banco de dados — tabelas principais
- `users` — autenticação
- `devices` — `IsOnline`, `LastSeen`, `ExternalId`, `IsActive`
- `measurements` — `SensorType`, `AdcRaw`, `VoltageV`, `RsOhm`, `RsR0Ratio`, `Ppm`, `Calibrated`, `Timestamp`

## MQTT — broker HiveMQ Cloud
```
Host:  0160eb21063349f3a226443abf16e94e.s1.eu.hivemq.cloud
Porta: 8883 (TLS)
User:  airwatch
```
Estrutura de tópicos atual:
- `airwatch/sensors` — telemetria (payload JSON com leituras dos sensores)
- `airwatch/devices/{externalId}/status` — presença do dispositivo (`online` / `offline`, retain=true)

Tópicos planejados (ainda não implementados):
- `airwatch/devices/{externalId}/commands` — comandos do backend para o ESP (calibração, etc.)
- `airwatch/devices/{externalId}/config` — configurações remotas

## Sensores e cálculos
O Arduino envia apenas o **ADC raw** de cada sensor. Tensão, Rs, Rs/R0 e PPM devem ser calculados no backend — **essa lógica ainda não foi implementada**.
- MQ3 — álcool etílico/etanol (25–5.000 ppm)
- MQ5 — GLP/gás natural (300–10.000 ppm)
- MQ135 — qualidade geral do ar: CO₂, NH₃, NOₓ, benzeno, fumaça (10–1.000 ppm para CO₂)

O R0 de cada sensor (resistência de referência em ar limpo) deve ser calibrado fisicamente, persistido no banco e recalibrável remotamente via MQTT — **também não implementado ainda**.

## Convenções
- Seguir Clean Architecture: regras de negócio ficam em Domain/Application, nunca em Infrastructure ou Api
- DTOs para entrada e saída de dados nas controllers — nunca expor entidades diretamente
- Sempre usar repositórios via interface (nunca instanciar DbContext direto fora de Infrastructure)
- SignalR emite eventos nomeados — manter consistência com os nomes que o frontend consome

## Dev sem hardware
- Para simular leituras de sensores: publicar JSON no tópico `airwatch/sensors` via Web Client do HiveMQ
- Para simular status: publicar `online` ou `offline` no tópico `airwatch/devices/{externalId}/status` com retain=true via Web Client do HiveMQ
- O MqttSimulatorService foi removido intencionalmente — não recriar

## Funcionalidades planejadas (ainda não implementadas)
- Alertas ao usuário quando concentração perigosa detectada
- Comandos remotos ao Arduino via MQTT
- Calibração remota de R0
- Ativação/desativação de dispositivos pelo frontend