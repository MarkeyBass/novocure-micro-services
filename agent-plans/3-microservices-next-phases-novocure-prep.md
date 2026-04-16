# Next Phases: Microservices Build-Out for Novocure Prep

This document starts the next development track fresh from the current separated-services baseline.

It assumes the current baseline branch preserves:
- Angular UI talking directly to `HousingApi` and `TodoApi`
- `BookStoreApi` still existing as a separate REST service
- local databases running separately
- no message broker or gateway yet

---

## Recommended baseline reference branch

Preferred branch name:

- `reference/separated-services-baseline`

Why this name:
- `reference` makes it clear this branch is a checkpoint, not the future architecture
- `separated-services-baseline` describes the current state honestly
- it stays useful later when you compare pre-RabbitMQ and post-gateway changes

Good alternatives:
- `checkpoint/rest-baseline-before-rabbitmq`
- `reference/monorepo-rest-baseline`

---

## What the baseline intentionally does not include yet

The baseline branch should explicitly **not** include:
- RabbitMQ eventing
- API gateway / reverse proxy
- unified Docker Compose for the whole app
- Prometheus metrics
- Grafana dashboards
- centralized logging stack

That is a feature, not a weakness: it keeps the reference branch simple, readable, and easy to explain in an interview as the "before microservices messaging" checkpoint.

---

## Repo noise to document before the next phase

Keep learning-oriented comments, but document anything that may confuse the next phase.

Current item worth calling out:
- `TodoApi/Controllers/TodoItemsController-no-dto-ref.cs`

Recommendation:
- if it is truly unused, either remove it before starting the microservices branch or add one short note in the baseline docs that it is a learning/reference file and not part of the active application path
- keep explanatory comments that help you discuss DTO evolution, refactoring, or controller design in the interview

Do not spend time on broad cleanup now. Only remove or document files that create ambiguity.

---

## Architecture direction for the next branch

Target the next branch toward the stack you want to discuss confidently in a Novocure interview:

- Angular 18
- NGXS
- RxJS
- custom CSS
- ASP.NET Core 7 REST APIs
- Docker
- microservices
- RabbitMQ
- Kafka later
- Keycloak later
- HTTPS
- sockets later if there is a real use case
- Mongo
- SQL Server

Important framing:
- do not try to introduce every listed technology in one step
- build a clean RabbitMQ microservices slice first
- add Kafka and Keycloak only after the core event-driven flow is stable

---

## Gateway recommendation: nginx vs YARP

For a company stack like this, the more likely production-facing answer is:

- `nginx` or another infrastructure reverse proxy / ingress is more likely at the edge

Why:
- it is common in Docker and microservices environments
- it is a natural fit for container routing, TLS termination, and simple path-based forwarding
- it is easier to explain as a standard infrastructure layer in interview discussions

Where `YARP` fits:
- YARP is a strong option if the team wants a .NET-native gateway with custom routing logic, auth integration, header transforms, or BFF-style behavior
- it is especially attractive in an ASP.NET-heavy ecosystem

Recommendation for this project:
- Phase 3 should use **nginx first**
- mention in your notes that **YARP would be the .NET-centric alternative** if business rules or gateway logic become application-level concerns

That gives you the strongest interview answer:
- "I would start with nginx for infrastructure-level reverse proxy concerns, and consider YARP if the gateway needs application-aware logic."

---

## Phase plan

### Phase 0: Freeze the current baseline

Goal:
- preserve the current separated-services state as a reference branch

Tasks:
- verify the current Angular, `HousingApi`, `TodoApi`, and `BookStoreApi` flows still start cleanly
- make the baseline boundary explicit in docs
- document the one or two intentionally retained reference files that are not part of the main runtime path
- branch from this state before introducing RabbitMQ or containers for the apps

Deliverable:
- a clean reference branch you can always return to

---

### Phase 1: Unify local infrastructure for development

Goal:
- create one repeatable local environment for the next microservices work

Tasks:
- consolidate duplicated database compose files into one source of truth
- add RabbitMQ with management UI
- define a shared Docker network
- move service connection details toward environment-based configuration instead of hardcoded localhost assumptions
- decide what runs in containers first:
  - databases and RabbitMQ immediately
  - APIs next
  - Angular either in Docker later or locally during early backend development

Deliverable:
- one `docker compose up` that brings up the infrastructure required for microservices development

Why this comes first:
- messaging, health checks, and gateway work are much easier once the environment is reproducible

---

### Phase 2: Containerize the backend services

Goal:
- make the APIs runnable in the same network as Mongo, SQL Server, and RabbitMQ

Tasks:
- add Dockerfiles for `HousingApi`, `TodoApi`, and `BookStoreApi`
- make configuration environment-driven for local container networking
- keep the services reachable for local manual testing over HTTPS or controlled HTTP inside Compose
- preserve direct API access during development so debugging stays simple

Deliverable:
- the three APIs can run locally in containers with their dependencies

Notes for interview alignment:
- this phase is a strong place to talk about separation of concerns, service ownership, and environment parity

---

### Phase 3: Implement the first RabbitMQ event flow

Goal:
- prove real asynchronous communication with the smallest valuable business slice

First event flow only:
- `HousingApplicationCreated`

Flow:
- `HousingApi` persists the application
- `HousingApi` publishes `HousingApplicationCreated`
- `TodoApi` consumes the event
- `TodoApi` creates a SQL todo like "Review housing application ..."

Tasks:
- choose a messaging approach:
  - MassTransit if you want cleaner infrastructure and more interview-friendly abstractions
  - raw `RabbitMQ.Client` if you want lower-level learning value
- create explicit message contracts
- add producer logic in `HousingApi`
- add consumer logic in `TodoApi`
- add basic logging for published and consumed messages
- define failure-handling expectations for the first iteration

Recommendation:
- prefer **MassTransit + RabbitMQ** for this project

Why:
- it is more production-aligned
- it reduces plumbing code
- it gives you better language for discussing retries, consumers, and transport abstraction in interviews

Deliverable:
- one end-to-end event-driven workflow that you can demo and explain

What not to add yet:
- `BookCreated`
- reverse `TodoCompleted`
- Kafka
- outbox implementation beyond design notes

---

### Phase 4: Add the reverse proxy / gateway

Goal:
- present one frontend-facing entry point and reduce direct browser-to-service coupling

Tasks:
- add nginx as a reverse proxy container
- route requests by path, for example:
  - `/api/housing/*`
  - `/api/todos/*`
  - `/api/books/*`
- decide whether Angular will call relative paths through the gateway
- reduce CORS complexity by moving toward one browser-facing origin

Deliverable:
- one consistent access point for the frontend and local testing tools

Interview note:
- frame nginx as the infrastructure-first solution
- mention YARP as a valid .NET gateway option if routing rules become application-aware

---

### Phase 5: Add health checks, logs, and diagnostics

Goal:
- make the system observable enough to debug before adding dashboards

Tasks:
- add ASP.NET Core health endpoints
- add container health checks in Compose
- improve console logging structure across services
- capture broker connection failures, consumer failures, and retry behavior clearly
- decide whether to adopt Serilog now or stay with built-in logging first

Deliverable:
- startup and failure states are visible without guessing

Why this phase matters:
- Prometheus and Grafana are much more valuable once the app already emits trustworthy health and log signals

---

### Phase 6: Add Prometheus and Grafana

Goal:
- expose and visualize service health and activity

Tasks:
- add Prometheus
- expose scrape endpoints
- add Grafana dashboards for:
  - API health
  - request counts
  - event publish/consume visibility
  - broker/service uptime

Deliverable:
- basic operational dashboards for your demo and interview discussion

Keep it simple:
- only instrument what helps explain the architecture
- do not overbuild dashboards before you have stable runtime behavior

---

### Phase 7: Add the second and third event flows

Goal:
- expand from one proven messaging workflow to a real multi-service story

Add next:
- `BookCreated` from `BookStoreApi` to `TodoApi`

Then later:
- `TodoCompleted` from `TodoApi` back to the originating service

Tasks:
- reuse the same contract and consumer design patterns from Phase 3
- make origin tracking explicit so `TodoCompleted` can route to the correct service
- decide how completion counters are stored and updated

Deliverable:
- the application starts to behave like a real event-driven microservice system instead of isolated REST APIs

---

### Phase 8: Add stack-aligned extensions only after the core flow is stable

Possible later additions:
- Kafka
- Keycloak
- WebSocket / SignalR or other socket-based notifications
- stronger resilience patterns such as outbox and idempotency handling
- migration planning toward Angular 18 and ASP.NET Core 7 if you want the repo to match the target stack more literally

Recommendation:
- do not start with Kafka if RabbitMQ is already your required event backbone
- use Kafka later only if you want to discuss event streaming separately from command/event messaging

---

## Recommended order of execution

1. Freeze and document the baseline branch.
2. Unify Compose and add RabbitMQ.
3. Containerize the three backend services.
4. Implement `HousingApplicationCreated` end-to-end.
5. Add nginx gateway routing.
6. Add health checks and structured logging.
7. Add Prometheus and Grafana.
8. Expand to `BookCreated` and `TodoCompleted`.
9. Add Kafka and Keycloak only if there is still time and a clear interview payoff.

---

## Interview-friendly story to tell

If this progression works, you will be able to explain the system in a very clean way:

- first, I built separated REST services with Angular integration
- then I preserved that state as a reference checkpoint
- next, I introduced shared containerized infrastructure
- then I added RabbitMQ to make the first business workflow asynchronous
- after that, I added gateway routing and observability
- only once the core system was stable did I expand to more advanced platform concerns

That is a strong engineering story because it shows sequencing, tradeoff awareness, and practical delivery discipline.
