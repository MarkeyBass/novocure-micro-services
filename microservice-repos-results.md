# Microservice Repo Search Results

Executed via GitHub MCP using the search brief in `agent-plans/github-repo-search-agent-prompt.md`.

## Summary

- Exact matches to all hard filters were not found.
- The strict combination of `200+ stars` + `active within 18 months` + `root docker-compose.yml` + `4+ service folders` + `Angular/Keycloak/observability` is very rare on GitHub for this stack.
- The best study set is a mix of:
  - one strong backend architecture reference,
  - one full-stack feature-complete but low-star repo,
  - one older but still structurally useful gateway/eventing sample.

## Best Partial Matches

### Repo: `meysamhadeli/booking-microservices`
URL: https://github.com/meysamhadeli/booking-microservices
Stars: 1339
Last commit: 2026-02-26
Score: 10/16
Matched criteria: [ASP.NET Core backend, 4+ microservices, RabbitMQ, MongoDB, PostgreSQL, OIDC/OAuth2 auth, OpenTelemetry, Prometheus + Grafana, Jaeger, Docker Compose]
Missing criteria: [Angular frontend, NGXS, Keycloak specifically, Redis, Kafka, real-time push, confirmed gateway endpoint aggregation]
Partial match notes: Best backend architecture match. Strongest repo here for vertical-slice boundaries, event-driven messaging, observability, and service separation.

NGXS / RxJS state flow:
  No Angular frontend in this repo. It is backend-focused, so it will not help with NGXS or Angular state flow.

Gateway-to-Service discovery:
  Uses YARP and Aspire service discovery per README. This looks closer to runtime service discovery than static-only routing, but it does not appear to expose the exact "gateway dynamically discovers sibling endpoints and aggregates them" pattern from the prompt.

RabbitMQ / Kafka implementation complexity:
  Strong. The README explicitly calls out RabbitMQ on MassTransit plus Inbox/Outbox, event sourcing, gRPC, and vertical slices. Consumers and event contracts are separated across services/building blocks.

Top-level structure:
  `assets`, `deployments`, `scripts`, `src`
  Service folders visible from paths/README: `src/Services/Booking`, `src/Services/Flight`, `src/Services/Identity`, `src/Services/Passenger`

Examples:
  - Endpoint/example request handler area: repo uses Minimal APIs rather than MVC controllers
  - Consumer/subscriber: `src/Services/Passenger/src/Passenger/Identity/Consumers/RegisteringNewUser/V1/RegisterNewUser.cs`
  - Message/event class: `src/Services/Passenger/src/Passenger/Identity/Consumers/RegisteringNewUser/V1/PassengerCreatedDomainEvent.cs`
  - Angular component: none in repo

Setup friction:
  Uses Duende IdentityServer rather than Keycloak, and the README notes production licensing considerations for Duende-style auth stacks.

Notes: This is the best repo to study first if the interview leans toward service boundaries, messaging, event sourcing, observability, and debugging distributed flows. It is weaker for the Novocure-style Angular dashboard side.
Clone command: `git clone https://github.com/meysamhadeli/booking-microservices`

### Repo: `aekoky/AiChatPlatform`
URL: https://github.com/aekoky/AiChatPlatform
Stars: 3
Last commit: 2026-04-10
Score: 8/16
Matched criteria: [ASP.NET Core backend, 4+ microservices, RabbitMQ, PostgreSQL, Angular frontend, Keycloak, Docker Compose, real-time push]
Missing criteria: [200+ stars, NGXS, Kafka, MongoDB, Redis confirmed in compose, OpenTelemetry, Prometheus + Grafana, Jaeger, dynamic service discovery]
Partial match notes: Best full-stack shape match to the Novocure-style brief. It combines Angular, Keycloak, RabbitMQ, gateway routing, multiple services, and live push in one locally runnable stack.

NGXS / RxJS state flow:
  Uses Angular 21, but the repo description indicates `NgRx SignalStore`, not NGXS. It is still a useful reactive frontend reference, just not the exact state-management stack from the prompt.

Gateway-to-Service discovery:
  Uses Kong. The root `docker-compose.yml` shows Kong routing to `chatserviceapi`, `notificationserviceapi`, and `webclient`, but this looks declarative/static rather than runtime endpoint discovery.

RabbitMQ / Kafka implementation complexity:
  Moderate to strong. The repo description references Wolverine sagas and separate worker services; `docker-compose.yml` includes multiple API and worker processes depending on RabbitMQ.

Top-level structure:
  `AiService`, `ChatService`, `DocumentIngestion`, `DocumentService`, `NotificationService`, `WebClient`, `Kong`, `RabbitMQ`, `keycloak`, `docker-compose.yml`

Examples:
  - Controller: `ChatService/ChatService.Api/Controllers/ChatController.cs`
  - Consumer/subscriber: worker-based consumers are spread across services; RabbitMQ-backed workers include `AiService` and `DocumentIngestion`
  - Message/event class: `ChatService/ChatService.Domain/Message/MessageAggregate.cs`
  - Angular component: `WebClient/src/app/features/chat/chat.component.ts`

Setup friction:
  Local setup is heavier than average. The compose stack pulls Ollama models, boots Keycloak, Kong, RabbitMQ, Postgres, MinIO, and multiple workers.

Notes: This is the closest UX/stack simulation for the interview, despite the very low star count. Use it as a supplement to study Angular + gateway + auth + live updates together.
Clone command: `git clone https://github.com/aekoky/AiChatPlatform`

### Repo: `ivaaak/.NET-RealEstate`
URL: https://github.com/ivaaak/.NET-RealEstate
Stars: 76
Last commit: 2024-07-23
Score: 7/16
Matched criteria: [ASP.NET Core backend, 4+ microservices, RabbitMQ, PostgreSQL, Redis, Keycloak, Docker Compose]
Missing criteria: [200+ stars, commit within 18 months, Angular frontend, NGXS, Kafka, MongoDB confirmed in root compose, OpenTelemetry, Prometheus + Grafana, Jaeger, real-time push, dynamic service discovery]
Partial match notes: Useful for studying a gateway-plus-many-services .NET layout with event consumers, Keycloak, RabbitMQ, and Redis in one repo. It is older and backend-only.

NGXS / RxJS state flow:
  No Angular frontend was found.

Gateway-to-Service discovery:
  Includes `RealEstate.ApiGateway` and a root `docker-compose.yml`, but nothing found suggests dynamic discovery. This appears closer to static routing.

RabbitMQ / Kafka implementation complexity:
  Basic to moderate. There are explicit event consumers and event classes in `RealEstate.Shared/EventBus`, but it does not look as advanced as the `booking-microservices` event stack.

Top-level structure:
  `Microservices`, `RealEstate.ApiGateway`, `RealEstate.Shared`, `build`, `docker-compose.yml`
  Compose services include `api-gateway`, `api.estates`, `api.listings`, `api.clients`, `api.contracts`, `api.external`, `api.messaging`, `api.utilities`

Examples:
  - Controller: `Microservices/EstatesMicroservice/Controllers/EstateController.cs`
  - Consumer/subscriber: `RealEstate.Shared/EventBus/Consumers/ClientConsumer.cs`
  - Message/event class: `RealEstate.Shared/EventBus/Events/EstateEvent.cs`
  - Angular component: none found

Setup friction:
  Older stack, older Keycloak image (`19.0.1` in root compose), and Windows-style secret mounts for at least one service suggest extra environment-specific setup work.

Notes: Not a top clone candidate, but still useful if you specifically want a readable .NET gateway + service fleet + shared event-bus example with Keycloak and Redis.
Clone command: `git clone https://github.com/ivaaak/.NET-RealEstate`

## Ruled Out Or Lower Priority

- `dotnet-architecture/eShopOnContainers`
  - Famous reference, but archived and last pushed in 2023.
- `phongnguyend/Practical.CleanArchitecture`
  - Very active and highly starred, with Angular, SignalR, RabbitMQ/Kafka options, OpenTelemetry, and YARP.
  - Lower priority for this specific prompt because the repo mixes monolith, modular monolith, and microservices modes, and the exact root-level microservice compose + isolated-service fit is less direct.

## Recommendation

If you only study one repo first, pick `meysamhadeli/booking-microservices` for architecture and debugging depth.

If you want the closest end-to-end simulation of the Novocure interview shape, pair it with `aekoky/AiChatPlatform`.

## Secondary Repos

### Repo: `phongnguyend/Practical.CleanArchitecture`
URL: https://github.com/phongnguyend/Practical.CleanArchitecture
Why it is worth keeping on the list: Very active and highly starred, with Angular, SignalR, OpenTelemetry, RabbitMQ/Kafka options, YARP, and a real `src/Microservices` tree. It is a strong breadth repo for modern .NET distributed app patterns.
Why it is secondary: It mixes monolith, modular monolith, and microservices modes, so it is less focused as a pure microservice interview-study repo. It also does not use Keycloak.
Clone command: `git clone https://github.com/phongnguyend/Practical.CleanArchitecture`

### Repo: `desenvolvedor-io/dev-store`
URL: https://github.com/desenvolvedor-io/dev-store
Why it is worth keeping on the list: Active and well-starred, with 7 APIs plus one web app, RabbitMQ, gRPC, BFF/API Gateway, and Docker Compose. It is one of the cleaner service-fleet study repos.
Why it is secondary: It is weaker than the top picks for Angular dashboard matching, Keycloak alignment, and observability depth.
Clone command: `git clone https://github.com/desenvolvedor-io/dev-store`

### Repo: `mansoorafzal/AspnetMicroservices`
URL: https://github.com/mansoorafzal/AspnetMicroservices
Why it is worth keeping on the list: Useful as an older infra-stack reference for RabbitMQ, Ocelot, OpenTelemetry, Jaeger, Redis, MongoDB, PostgreSQL, and SQL Server in one sample.
Why it is secondary: It is too old to recommend as a primary study repo, but still useful if you want one more observability-heavy reference.
Clone command: `git clone https://github.com/mansoorafzal/AspnetMicroservices`
