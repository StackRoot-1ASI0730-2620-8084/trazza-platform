# Architecture Decision Records (ADRs)

This document records the key architectural decisions made for the **Trazza Platform** RESTful Web Services, detailing the context, options considered, decisions, and consequences.

---

## ADR Index

1. [ADR-001: Domain-Driven Design with Trazza Bounded Contexts](#adr-001-domain-driven-design-with-trazza-bounded-contexts)
2. [ADR-002: CQRS with Command and Query Services and Cortex Mediator](#adr-002-cqrs-with-command-and-query-services-and-cortex-mediator)
3. [ADR-003: Result Pattern and Typed Error Model Instead of Business Exceptions](#adr-003-result-pattern-and-typed-error-model-instead-of-business-exceptions)
4. [ADR-004: Public Guid Identifiers and Strongly-Typed Internal IDs](#adr-004-public-guid-identifiers-and-strongly-typed-internal-ids)
5. [ADR-005: Non-Tracking Reads in Query Services](#adr-005-non-tracking-reads-in-query-services)
6. [ADR-006: Domain Events Dispatched via EF Core Interceptor](#adr-006-domain-events-dispatched-via-ef-core-interceptor)
7. [ADR-007: Shared Kernel with Money, Address and GeoLocation](#adr-007-shared-kernel-with-money-address-and-geolocation)
8. [ADR-008: Route Matching as a Domain Service](#adr-008-route-matching-as-a-domain-service)
9. [ADR-009: Anti-Corruption Layer Facades Between Bounded Contexts](#adr-009-anti-corruption-layer-facades-between-bounded-contexts)
10. [ADR-010: IAM with BCrypt, JWT, Refresh Tokens and AES-GCM Personal Data Protection](#adr-010-iam-with-bcrypt-jwt-refresh-tokens-and-aes-gcm-personal-data-protection)
11. [ADR-011: IExceptionHandler and RFC 9457 Problem Details with Localized Messages](#adr-011-iexceptionhandler-and-rfc-9457-problem-details-with-localized-messages)
12. [ADR-012: EF Core with Shared MySQL Database Schema and Audit Interceptor](#adr-012-ef-core-with-shared-mysql-database-schema-and-audit-interceptor)
13. [ADR-013: OpenAPI Documentation with Scalar](#adr-013-openapi-documentation-with-scalar)

---

## ADR-001: Domain-Driven Design with Trazza Bounded Contexts

- **Status:** Accepted
- **Date:** 2026-11-12

### Context
Trazza connects carriers that travel with empty return trips and merchants that need to ship goods inside Metropolitan Lima. The domain covers identity, profiles and fleets, route matching and proposal negotiation, trip execution and GPS monitoring, subscription and freight settlement billing, and reputation with loyalty points.

### Decision
Organize `Trazza.Platform` into five domain bounded contexts plus `Shared`, each split into `Domain`, `Application`, `Infrastructure`, and `Interfaces`:

| Bounded Context | Aggregates | Main Responsibilities |
|:--|:--|:--|
| `Iam` | `User`, `PersonIdentity`, `IdentityVerification`, `RefreshToken`, `CarrierProfile`, `MerchantProfile`, `Vehicle`, `Driver`, `SavedAddress` | Authentication, encrypted personal data, RENIEC verification, profiles, fleet, and saved addresses |
| `Matchmaking` | `ReturnRoute`, `ShipmentRequest`, `MatchProposal`, `Message` | Return routes, shipment requests, compatibility suggestions, proposals, and negotiation messages |
| `Execution` | `Trip`, `TrackingPoint`, `DeviationAlert`, `Incident`, `IotDevice` | Trip execution lifecycle, live GPS tracking, route corridor deviation alerts, and incidents |
| `Billing` | `Plan`, `Subscription`, `SubscriptionPayment`, `FreightSettlement`, `CancellationCompensation` | Carrier subscription plans, active subscriptions, trip freight settlements, and cancellation compensations |
| `Reputation` | `Rating`, `LoyaltyPointEntry` | Post-trip ratings, reputation summaries, and loyalty point accrual and redemptions |
| `Shared` | `AggregateRoot` | `Money`, `Address`, `GeoLocation`, `LimaDistricts`, `LimaCalendar`, interceptors, and pipeline behaviors |

### Consequences
- **Positive:** Clear ubiquitous language and independent evolution of each module.
- **Negative:** Requires explicit mapping between REST resources, commands, and domain models.

---

## ADR-002: CQRS with Command and Query Services and Cortex Mediator

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
- Every context exposes `I*CommandService` (write operations returning `Result<T>`) and `I*QueryService` (read operations).
- Commands and queries are immutable records in `Domain/Model/Commands` and `Domain/Model/Queries`.
- `Cortex.Mediator` is registered with `LoggingCommandBehavior`, `ExceptionHandlingCommandBehavior`, and `TransactionCommandBehavior`.

---

## ADR-003: Result Pattern and Typed Error Model Instead of Business Exceptions

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
- `Error(Code, Message, EErrorType)` classifies domain errors into `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`, and `Unexpected`.
- Each context declares its error catalog (`IamErrors`, `MatchmakingErrors`, `ExecutionErrors`, `BillingErrors`, `ReputationErrors`).
- Command services return `Result<T>`, and controllers map failures to RFC 9457 `ProblemDetails` via `ProblemDetailsFactory`.

---

## ADR-004: Public Guid Identifiers and Strongly-Typed Internal IDs

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
- Aggregates maintain internal database keys (`long`/`int` or strongly-typed record structs implementing `IEntityId<T>`) while exposing a `Guid` `PublicId` (`public_id`) over REST endpoints.
- Cross-context and public API routes resolve resources by `Guid` (`{returnRouteId:guid}`, `{shipmentRequestId:guid}`, `{matchProposalId:guid}`, `{tripId:guid}`, `{vehicleId:guid}`, `{userId:guid}`).

---

## ADR-005: Non-Tracking Reads in Query Services

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
Query services and read repositories use `.AsNoTracking()` for read projections, while command services load tracked aggregates through EF Core repositories and commit via `IUnitOfWork.CompleteAsync()`.

---

## ADR-006: Domain Events Dispatched via EF Core Interceptor

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
Aggregates inherit from `AggregateRoot` and record domain events via `AddDomainEvent`. `DispatchDomainEventsInterceptor` dispatches queued events through `Cortex.Mediator` after changes are saved.

---

## ADR-007: Shared Kernel with Money, Address and GeoLocation

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
The `Shared` context provides common domain value objects:
- `Money(Amount, Currency)` in PEN (`S/`).
- `Address(Street, District)` validated against `LimaDistricts`.
- `GeoLocation(Latitude, Longitude)` with Haversine distance and segment calculations.
- `LimaCalendar` with the Lima timezone offset (UTC-5).

---

## ADR-008: Route Matching as a Domain Service

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
`IRouteMatchingService` / `RouteMatchingService` in `Matchmaking/Domain/Services` evaluates spatial corridor compatibility, available weight and volume capacity, cargo type compatibility, and time windows between `ReturnRoute` and `ShipmentRequest` aggregates.

---

## ADR-009: Anti-Corruption Layer Facades Between Bounded Contexts

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
Bounded contexts communicate synchronously only through ACL facades in `Interfaces/Acl`:
- `IIamContextFacade`: User identity, carrier/merchant profiles, and fleet lookups.
- `IMatchmakingContextFacade`: Proposal and trip context snapshots for Execution.
- `IExecutionContextFacade`: Trip status, participants, and completion snapshots for Billing and Reputation.
- `IBillingContextFacade`: Carrier subscription status and freight settlement creation.
- `IReputationContextFacade`: Reputation summary and loyalty points integration.

---

## ADR-010: IAM with BCrypt, JWT, Refresh Tokens and AES-GCM Personal Data Protection

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
- Passwords are hashed with BCrypt (`BCryptHashingService`).
- Access tokens are signed with HMAC SHA-256 (`JwtTokenService`), paired with rotating `RefreshToken` records.
- Sensitive identity numbers (DNI/CE) are encrypted at rest with AES-GCM (`AesGcmPersonalDataProtector`) and indexed via HMAC-SHA256 blind hashes.
- `RequestAuthorizationMiddleware` populates `AuthenticatedUser` in `HttpContext.Items`.

---

## ADR-011: IExceptionHandler and RFC 9457 Problem Details with Localized Messages

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
- `GlobalExceptionHandler` implements `IExceptionHandler` and writes RFC 9457 `ProblemDetails` for unhandled exceptions.
- `ProblemDetailsFactory` localizes error titles and details using `CommonMessages` and `ErrorMessages` (`.resx` in English and Spanish) based on `Accept-Language`.

---

## ADR-012: EF Core with Shared MySQL Database Schema and Audit Interceptor

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
- `AppDbContext` applies each bounded context's entity mappings (`ApplyIamConfiguration`, `ApplyMatchmakingConfiguration`, `ApplyExecutionConfiguration`, `ApplyBillingConfiguration`, `ApplyReputationConfiguration`) against the MySQL 8 `trazza_database` schema.
- Database credentials and secrets are read strictly from environment variables (`DATABASE_URL`, `DATABASE_USER`, `DATABASE_PASSWORD`, `DATABASE_SCHEMA`).
- `AuditableEntityInterceptor` populates `CreatedAt` and `UpdatedAt` timestamps in UTC.

---

## ADR-013: OpenAPI Documentation with Scalar

- **Status:** Accepted
- **Date:** 2026-11-12

### Decision
`Microsoft.AspNetCore.OpenApi` generates `/openapi/v1.json` with `BearerSecuritySchemeTransformer`, and `Scalar.AspNetCore` serves the interactive API reference at `/scalar/v1`.
