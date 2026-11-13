# Trazza Platform

[![Release](https://img.shields.io/badge/release-v1.0.0-blue.svg)](CHANGELOG.md)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-blue.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)

## Project Overview

The **Trazza Platform** is the RESTful backend of **Trazza**, a logistics SaaS that connects **carriers** who drive back empty after a delivery with **merchants** (small businesses and entrepreneurs) who need to ship goods inside Metropolitan Lima. Carriers publish *return routes* with their free capacity, merchants publish *shipment requests*, the platform suggests compatible matches, lets both parties negotiate proposals and messages, tracks the resulting trip until delivery, manages subscriptions and freight settlements, and builds the reputation and loyalty points of both sides.

Built with **.NET 10** and **C# 14**, it adheres to **Domain-Driven Design (DDD)** tactical patterns and implements **Command Query Responsibility Segregation (CQRS)** with pipeline behaviors. It serves the Vue 3 web application `trazza-webapp`.

The platform is structured into decoupled **Bounded Contexts** (`Iam`, `Matchmaking`, `Execution`, `Billing`, `Reputation`, and `Shared`), ensuring clear separation of concerns, high maintainability, and transactional consistency.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Domain-Driven Design (DDD) Concepts](#domain-driven-design-ddd-concepts)
3. [Key Features & Best Practices](#key-features--best-practices)
4. [Bounded Contexts](#bounded-contexts)
   * [IAM & Fleet (Identity, Profiles & Fleet Management)](#iam--fleet-identity-profiles--fleet-management)
   * [Matchmaking & Routing](#matchmaking--routing)
   * [Service Execution & Monitoring](#service-execution--monitoring)
   * [Payment & Billing](#payment--billing)
   * [Loyalty & Reputation](#loyalty--reputation)
   * [Shared Kernel](#shared-kernel)
5. [Technologies Used](#technologies-used)
6. [Getting Started](#getting-started)
   * [Prerequisites](#prerequisites)
   * [Configuration](#configuration)
   * [Setup Instructions](#setup-instructions)
7. [Project Structure](#project-structure)
8. [Documentation](#documentation)
9. [Team](#team)
10. [Contributing & Changelog](#contributing--changelog)
11. [License](#license)

---

## Architecture Overview

The platform architecture follows **Domain-Driven Design (DDD)** and **Command Query Responsibility Segregation (CQRS)**:

* **Domain Layer**: Encapsulates aggregates (`AggregateRoot`), entities, value objects, domain events (`IEvent`), domain services (`RouteMatchingService`), error catalogs, and repository contracts.
* **Application Layer**: Coordinates domain operations through **Command Services** (write side) and **Query Services** (read side), outbound service contracts (`ITokenService`, `IHashingService`, `IPersonalDataProtector`, `IIdentityVerificationProvider`), and ACL facades.
* **Infrastructure Layer**: Implements persistence via Entity Framework Core (MySQL), strongly-typed ID value converters, JWT token services, BCrypt hashing, AES-GCM personal data protection, and interceptor-based domain event dispatching.
* **Interfaces Layer (Presentation)**: Exposes RESTful controllers, resources and assemblers, RFC 9457 `ProblemDetails` error responses, and ACL facade contracts.

---

## Domain-Driven Design (DDD) Concepts

* **Bounded Contexts**: Independent business domains (`Iam`, `Matchmaking`, `Execution`, `Billing`, `Reputation`, and `Shared`).
* **Aggregates & Invariant Protection**: Aggregates (`User`, `CarrierProfile`, `MerchantProfile`, `Vehicle`, `Driver`, `SavedAddress`, `ReturnRoute`, `ShipmentRequest`, `MatchProposal`, `Message`, `Trip`, `Incident`, `Plan`, `Subscription`, `FreightSettlement`, `CancellationCompensation`, `Rating`, `LoyaltyPointEntry`) inherit from `AggregateRoot` and guard internal state consistency.
* **Public & Internal Identifiers**: Public resources are exposed via `Guid` identifiers (`PublicId`), keeping internal database keys isolated inside each bounded context.
* **Value Objects**: Immutable records (`Money`, `Address`, `GeoLocation`, `TimeWindow`, `Cargo`, `CargoType`, `Detour`, `RouteGeometry`, `SpatialPoint`).
* **Domain Service**: `RouteMatchingService` evaluates capacity, time windows, cargo types, and spatial corridors between return routes and shipment requests.
* **Anti-Corruption Layer (ACL)**: Facades (`IIamContextFacade`, `IMatchmakingContextFacade`, `IExecutionContextFacade`, `IBillingContextFacade`, `IReputationContextFacade`) isolate cross-context communication.

---

## Key Features & Best Practices

* **CQRS with Mediator Pipeline Behaviors**:
  * `TransactionCommandBehavior`: Commits changes via `IUnitOfWork.CompleteAsync()` on successful command execution.
  * `ExceptionHandlingCommandBehavior`: Centralizes database and concurrency exception mapping.
  * `LoggingCommandBehavior`: Structured execution tracing.
* **Functional Result Pattern**:
  * Operations return `Result<T>` and strongly-typed `Error` records with `EErrorType` classification (`Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`, `Unexpected`).
* **Interactive Scalar OpenAPI Documentation**:
  * Built on `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore` at `/scalar/v1`, with JWT Bearer authorization through `BearerSecuritySchemeTransformer`.
* **Standardized Error Handling**:
  * ASP.NET Core `IExceptionHandler` (`GlobalExceptionHandler`) emitting RFC 9457 `ProblemDetails`.
* **Internationalization (i18n)**:
  * Localized error messages in English and Spanish via `IStringLocalizer` and `.resx` resource files, selected with `Accept-Language`.

---

## Bounded Contexts

### IAM & Fleet (Identity, Profiles & Fleet Management)
* **Scope**: User registration, authentication, JWT & refresh tokens, AES-GCM encrypted personal identity (DNI/CE), RENIEC identity verification, carrier & merchant profiles, vehicles, drivers, and saved addresses.
* **Aggregates**: `User`, `PersonIdentity`, `IdentityVerification`, `RefreshToken`, `CarrierProfile`, `MerchantProfile`, `Vehicle`, `Driver`, `SavedAddress`
* **RESTful Endpoints**:
  * `POST /api/v1/authentication/sign-up`, `POST /api/v1/authentication/sign-in`, `POST /api/v1/authentication/refresh`, `POST /api/v1/authentication/sign-out`
  * `GET /api/v1/users/me`, `PUT /api/v1/users/me/identity`, `POST /api/v1/users/me/roles`, `PUT /api/v1/users/me/merchant-profile`, `GET|POST /api/v1/users/me/identity-verifications`
  * `GET|POST /api/v1/vehicles`, `PUT|DELETE /api/v1/vehicles/{vehicleId}`
  * `GET|POST /api/v1/drivers`, `PUT|DELETE /api/v1/drivers/{driverId}`
  * `GET|POST /api/v1/saved-addresses`, `DELETE /api/v1/saved-addresses/{savedAddressId}`

### Matchmaking & Routing
* **Scope**: Return routes, shipment requests, compatibility suggestions, match proposals, and proposal negotiation messages.
* **Aggregates**: `ReturnRoute`, `ShipmentRequest`, `MatchProposal`, `Message`
* **Domain Service**: `RouteMatchingService`
* **RESTful Endpoints**:
  * `/api/v1/return-routes` (`GET`, `POST`, `GET /mine`, `GET /search`, `GET /{id}`, `GET /{id}/proposals`, `GET /{id}/suggested-shipments`, `POST /{id}/cancel`)
  * `/api/v1/shipment-requests` (`GET`, `POST`, `GET /open`, `GET /mine`, `GET /search`, `GET /{id}`, `GET /{id}/proposals`, `GET /{id}/suggested-routes`, `POST /{id}/cancel`)
  * `/api/v1/match-proposals` (`GET`, `POST`, `GET /{id}`, `POST /{id}/accept`, `POST /{id}/reject`, `POST /{id}/cancel`)
  * `/api/v1/match-proposals/{matchProposalId}/messages` (`GET`, `POST`, `POST /read`)

### Service Execution & Monitoring
* **Scope**: Trip lifecycle from accepted proposal to confirmed delivery, driver assignment, live GPS tracking points, route deviation alerts, IoT devices, and incidents with evidence.
* **Aggregates**: `Trip`, `TrackingPoint`, `DeviationAlert`, `Incident`, `IotDevice`
* **RESTful Endpoints**:
  * `/api/v1/trips` (`GET`, `GET /{tripId}`, `GET /{tripId}/status-history`, `PUT /{tripId}/driver`, `POST /{tripId}/pick-up`, `POST /{tripId}/deliver`, `POST /{tripId}/confirm`, `POST /{tripId}/cancel`)
  * `/api/v1/trips/{tripId}/tracking-points` (`GET`, `POST`)
  * `/api/v1/trips/{tripId}/deviation-alerts` (`GET`, `POST /{alertId}/acknowledge`)
  * `/api/v1/trips/{tripId}/incidents` (`GET`, `POST`, `POST /{incidentId}/evidences`, `POST /{incidentId}/resolve`)
  * `/api/v1/iot-devices` (`GET`)

### Payment & Billing
* **Scope**: Carrier subscription plans, active subscriptions with simulated payments, freight settlements per trip, and cancellation compensations.
* **Aggregates**: `Plan`, `Subscription`, `SubscriptionPayment`, `FreightSettlement`, `CancellationCompensation`
* **RESTful Endpoints**:
  * `/api/v1/plans` (`GET`)
  * `/api/v1/subscriptions` (`GET`, `POST`, `POST /{subscriptionId}/renew`, `POST /{subscriptionId}/cancel`)
  * `/api/v1/freight-settlements` (`GET`, `POST /{tripId}/mark-paid`, `POST /{tripId}/confirm`)
  * `/api/v1/cancellation-compensations` (`GET`, `POST /{tripId}/confirm-paid`, `POST /{tripId}/waive`, `POST /{tripId}/dispute`)

### Loyalty & Reputation
* **Scope**: Mutual trip ratings between carriers and merchants, user reputation summaries, and loyalty point accrual and redemptions.
* **Aggregates**: `Rating`, `LoyaltyPointEntry`
* **RESTful Endpoints**:
  * `/api/v1/ratings` (`POST`, `GET /received`, `GET /given`, `GET /summary/{userId}`)
  * `/api/v1/loyalty` (`GET /points`, `POST /redemptions`)

### Shared Kernel
* **Scope**: Common building blocks used across all contexts.
* **Contents**: `AggregateRoot`, `IEntityId<T>`, `Result<T>`, `Error`, `Money`, `Address`, `GeoLocation`, `LimaDistricts`, `LimaCalendar`, interceptors, pipeline behaviors, `ProblemDetailsFactory`, `GlobalExceptionHandler`, and `/api/v1/health`.

---

## Technologies Used

* **.NET 10** & **C# 14**: Core runtime and language.
* **ASP.NET Core**: Web API framework.
* **Entity Framework Core**: Relational database ORM (MySQL).
* **MySQL 8**: Primary database (`trazza_database`).
* **Cortex.Mediator**: In-process mediator, pipeline behaviors, and domain event publishing.
* **Scalar.AspNetCore** & **Microsoft.AspNetCore.OpenApi**: Interactive OpenAPI documentation.
* **BCrypt.Net-Next**: Password hashing.
* **Microsoft.IdentityModel.JsonWebTokens**: JWT token generation and validation.
* **Humanizer**: Table and route naming conventions.
* **Microsoft.Extensions.Localization**: English and Spanish localization.

---

## Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* Access to the shared MySQL 8 database `trazza_database` (Amazon RDS) or a local MySQL 8 instance with the same schema
* JetBrains Rider or Visual Studio 2026 / VS Code

### Configuration

No credentials are stored in the repository. Every secret is read from environment variables (or a local `.env` file in Development, see `.env.example`):

| Variable | Description | Example |
|:--|:--|:--|
| `DATABASE_URL` | MySQL host | `localhost` or the RDS endpoint |
| `DATABASE_USER` | MySQL user | your own database user |
| `DATABASE_PASSWORD` | MySQL password | (never commit it) |
| `DATABASE_SCHEMA` | Database name | `trazza_database` |
| `TokenSettings__Secret` | JWT signing key used by IAM, at least 32 characters | (never commit it) |
| `PersonalData__EncryptionKey` | 32-byte Base64 AES-GCM key for personal data | (never commit it) |
| `PersonalData__HashKey` | 32-byte Base64 HMAC key for document hashing | (never commit it) |
| `Cors__AllowedOrigins__0` | Web application origin allowed by CORS | `http://localhost:5173` |

The database schema is managed outside the API (database-first). The API does not create or migrate tables on startup.

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/StackRoot-1ASI0730-2620-8084/trazza-platform.git
   cd trazza-platform
   ```

2. **Copy `.env.example` to `.env`** and fill in your local or RDS variables.

3. **Restore and build:**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run --project Trazza.Platform
   ```

5. **Explore Interactive API Reference:**
   * Scalar Documentation: [http://localhost:5222/scalar/v1](http://localhost:5222/scalar/v1)
   * OpenAPI Specification: [http://localhost:5222/openapi/v1.json](http://localhost:5222/openapi/v1.json)

---

## Project Structure

```
Trazza.Platform/
├── Iam/                     # IAM, Profiles & Fleet Bounded Context
│   ├── Application/         # User & Fleet Command/Query Services, ACL Facade
│   ├── Domain/              # User, CarrierProfile, MerchantProfile, Vehicle, Driver, SavedAddress
│   ├── Infrastructure/      # Repositories, JWT TokenService, BCrypt, AES-GCM, Authorization Middleware
│   └── Interfaces/          # Authentication, Users, Vehicles, Drivers, SavedAddresses Controllers
├── Matchmaking/             # Matchmaking & Routing Bounded Context
│   ├── Application/         # ReturnRoute, ShipmentRequest, MatchProposal & Message Services, ACL Facade
│   ├── Domain/              # ReturnRoute, ShipmentRequest, MatchProposal, Message, RouteMatchingService
│   ├── Infrastructure/      # Repositories, EF Core configuration, IAM Profile adapter
│   └── Interfaces/          # ReturnRoutes, ShipmentRequests, MatchProposals, Messages Controllers
├── Execution/               # Service Execution & Monitoring Bounded Context
│   ├── Application/         # Trip, Tracking, Incident & Query Services, ACL Facade
│   ├── Domain/              # Trip, TrackingPoint, DeviationAlert, Incident, IotDevice
│   ├── Infrastructure/      # Execution Repositories, EF Core configuration
│   └── Interfaces/          # TripsController, IotDevicesController
├── Billing/                 # Payment & Billing Bounded Context
│   ├── Application/         # Subscription, Settlement & Billing Query Services, ACL Facade
│   ├── Domain/              # Plan, Subscription, SubscriptionPayment, FreightSettlement, CancellationCompensation
│   ├── Infrastructure/      # Billing Repositories, EF Core configuration
│   └── Interfaces/          # Plans, Subscriptions, FreightSettlements, CancellationCompensations Controllers
├── Reputation/              # Loyalty & Reputation Bounded Context
│   ├── Application/         # Reputation Command/Query Services, ACL Facade
│   ├── Domain/              # Rating, LoyaltyPointEntry
│   ├── Infrastructure/      # Reputation Repositories, EF Core configuration
│   └── Interfaces/          # RatingsController, LoyaltyController
├── Shared/                  # Shared Kernel & Infrastructure
│   ├── Application/         # Result<T>, IEventHandler
│   ├── Domain/              # AggregateRoot, IEntityId, Error, Money, Address, GeoLocation
│   ├── Infrastructure/      # AppDbContext, Pipeline Behaviors, Interceptors, OpenAPI, GlobalExceptionHandler
│   ├── Interfaces/          # HealthController, ProblemDetailsFactory, shared resources
│   └── Resources/           # Common & Error localization files (en, es)
├── Program.cs               # Startup configuration and DI registrations
└── appsettings.json         # Configuration settings
```

---

## Documentation

* **REST API Endpoints**: [`docs/endpoints.md`](docs/endpoints.md)
* **Technical User Stories**: [`docs/user-stories.md`](docs/user-stories.md)
* **Architecture Decision Records (ADRs)**: [`docs/adrs.md`](docs/adrs.md)
* **PlantUML Class Diagram**: [`docs/class-diagram.puml`](docs/class-diagram.puml)
* **Software Architecture DSL (C4)**: [`docs/software-architecture.dsl`](docs/software-architecture.dsl)
* **Study Guide**: [`docs/study-guide.md`](docs/study-guide.md)

---

## Team

**StackRoot** — Web Applications Development (1ASI0730), Universidad Peruana de Ciencias Aplicadas.

| Member | Bounded Context |
|:--|:--|
| Emanuel Renato Checalla Apaza | IAM & Profiles |
| Fabricio Jofred Lozano Quispe | Payment & Billing |
| Ingrid Melani Medina Merma | Service Execution & Monitoring |
| Gabriel Augusto Peñaranda Caldas | Matchmaking & Routing, Shared Kernel |
| Rodrigo Matias Vite Celis | Loyalty & Reputation |

---

## Contributing & Changelog

* See [`CONTRIBUTING.md`](CONTRIBUTING.md) for contribution guidelines, commit standards, and workflow.
* See [`CHANGELOG.md`](CHANGELOG.md) for version release notes.

---

## License

This project is licensed under the **MIT License**. See [`LICENSE.md`](LICENSE.md) for details.
