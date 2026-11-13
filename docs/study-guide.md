# Study Guide: Trazza Platform Backend Architecture (.NET 10 / C# 14)

## 1. Purpose and Scope

`Trazza.Platform` is the RESTful backend for **Trazza**, a logistics platform connecting carriers with empty return trips and merchants shipping goods across Metropolitan Lima. The solution is built on **.NET 10** and **C# 14** using **Domain-Driven Design (DDD)**, **CQRS**, and **Entity Framework Core** against MySQL 8 (`trazza_database`).

---

## 2. Bounded Contexts Overview

| Bounded Context | Namespace | Core Aggregates | Primary Responsibility |
|:--|:--|:--|:--|
| **Shared** | `Trazza.Platform.Shared` | `AggregateRoot` | Base domain abstractions, `Result<T>`, `Error`, `Money`, `Address`, `GeoLocation`, EF Core interceptors, pipeline behaviors, RFC 9457 `ProblemDetails` |
| **IAM & Fleet** | `Trazza.Platform.Iam` | `User`, `PersonIdentity`, `IdentityVerification`, `RefreshToken`, `CarrierProfile`, `MerchantProfile`, `Vehicle`, `Driver`, `SavedAddress` | Authentication, JWT & refresh tokens, AES-GCM encrypted identity, RENIEC verification, carrier/merchant profiles, and fleet management |
| **Matchmaking** | `Trazza.Platform.Matchmaking` | `ReturnRoute`, `ShipmentRequest`, `MatchProposal`, `Message` | Publishing return routes and shipment requests, corridor & capacity matching (`RouteMatchingService`), proposals, and negotiation messages |
| **Execution** | `Trazza.Platform.Execution` | `Trip`, `TrackingPoint`, `DeviationAlert`, `Incident`, `IotDevice` | Trip lifecycle (`scheduled`, `picked_up`, `in_transit`, `delivered`, `completed`, `cancelled`), GPS tracking points, corridor deviation alerts, and incidents |
| **Billing** | `Trazza.Platform.Billing` | `Plan`, `Subscription`, `SubscriptionPayment`, `FreightSettlement`, `CancellationCompensation` | Carrier subscription plans, active subscriptions, trip freight settlements, and cancellation compensations |
| **Reputation** | `Trazza.Platform.Reputation` | `Rating`, `LoyaltyPointEntry` | Post-trip mutual ratings, user reputation summaries, and loyalty points ledger |

---

## 3. Layered Architecture per Bounded Context

Each bounded context is organized into four layers:

1. **Domain Layer (`Domain/`)**
   - `Model/Aggregates/` and `Model/Entities/`: Business state and invariant checks.
   - `Model/ValueObjects/`: Immutable domain values (`Money`, `TimeWindow`, `Cargo`, `RouteStop`, `SpatialPoint`).
   - `Model/Commands/` and `Model/Queries/`: Immutable records representing write and read intentions.
   - `Model/Errors/`: Strongly-typed `Error` catalog (`IamErrors`, `MatchmakingErrors`, `ExecutionErrors`, `BillingErrors`, `ReputationErrors`).
   - `Repositories/` and `Services/`: Domain repository and service interfaces.
2. **Application Layer (`Application/`)**
   - `Internal/CommandServices/`: Orchestrates write operations and returns `Result<T>`.
   - `Internal/QueryServices/`: Executes read queries and projections.
   - `Acl/`: Implements the context's Anti-Corruption Layer facade.
3. **Infrastructure Layer (`Infrastructure/`)**
   - `Persistence/EntityFrameworkCore/`: EF Core entity configurations (`ModelBuilderExtensions`) and repository implementations.
   - Security, token, hashing, and external provider adapters.
4. **Interfaces Layer (`Interfaces/`)**
   - `Rest/`: ASP.NET Core controllers, request/response resource records, and resource assemblers.
   - `Acl/`: Public ACL facade interfaces consumed by other bounded contexts.

---

## 4. End-to-End Request Flow

1. **HTTP Request & Authentication**:
   - Requests enter the ASP.NET Core pipeline (`Program.cs`).
   - `RequestAuthorizationMiddleware` validates the `Authorization: Bearer <token>` header and attaches `AuthenticatedUser` to `HttpContext.Items`.
2. **Controller & Assembler**:
   - The controller translates the request body or route parameters into a command or query record.
3. **Command / Query Service**:
   - Command services validate business rules across aggregates and repositories, persist changes via `IUnitOfWork.CompleteAsync()`, and return `Result<T>`.
4. **EF Core Interceptors**:
   - `AuditableEntityInterceptor` updates `CreatedAt` and `UpdatedAt` timestamps.
   - `DispatchDomainEventsInterceptor` publishes any domain events accumulated on aggregates after `SaveChangesAsync` succeeds.
5. **HTTP Response or RFC 9457 ProblemDetails**:
   - Successful results are mapped to REST resource DTOs; domain errors are mapped to localized RFC 9457 `ProblemDetails` responses by `ProblemDetailsFactory`.

---

## 5. Cross-Context Integration Flows

1. **Matchmaking → IAM (`IIamContextFacade` / `IProfilesService`)**:
   - Validates carrier profiles, vehicle ownership and capacity (`MaxWeightKg`, `MaxVolumeM3`), and merchant profiles when publishing routes or shipment requests.
2. **Matchmaking → Execution (`IExecutionContextFacade`)**:
   - When a `MatchProposal` is accepted (`POST /api/v1/match-proposals/{id}/accept`), `MatchProposalCommandService` reserves capacity on the `ReturnRoute`, marks the `ShipmentRequest` as matched, and creates the `Trip` in `Execution`.
3. **Execution → Billing & Reputation (`IExecutionContextFacade`)**:
   - `Billing` and `Reputation` query trip state, participants, and agreed freight amounts through `IExecutionContextFacade` to settle freight payments, handle cancellation compensations, and verify rating eligibility.
