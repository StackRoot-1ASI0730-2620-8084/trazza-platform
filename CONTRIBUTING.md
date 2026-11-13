# Contributing to Trazza Platform

Thank you for contributing to the **Trazza Platform**! Please review this guide to understand our engineering practices and workflow.

---

## 1. Development Principles

We adhere to modern .NET development practices:
- **C# 14 & .NET 10 Standards:** Primary constructors, collection expressions (`[.. items]`), file-scoped namespaces, pattern matching, records, and nullable reference types enabled.
- **Readable & Documented Code:** Express intent clearly through descriptive names for types, methods, and variables, using XML documentation (`/// <summary>`) on public contracts, domain aggregates, and REST controllers.
- **Domain-Driven Design (DDD):**
  - Bounded contexts (`Iam`, `Matchmaking`, `Execution`, `Billing`, `Reputation`, `Shared`).
  - Aggregate roots inherit from `AggregateRoot` and protect their domain invariants.
  - Public resources use `Guid` public identifiers (`PublicId`) to avoid exposing sequential database keys.
  - Cross-context communication goes through ACL facades (`IIamContextFacade`, `IMatchmakingContextFacade`, `IExecutionContextFacade`, `IBillingContextFacade`, `IReputationContextFacade`) or domain events.
- **CQRS (Command Query Responsibility Segregation):**
  - Separation between Command Services (write side) and Query Services (read side).
  - Cross-cutting concerns managed through Cortex Mediator behaviors (`LoggingCommandBehavior`, `TransactionCommandBehavior`, `ExceptionHandlingCommandBehavior`).
- **Functional Result Pattern:**
  - Return `Result<T>` and strongly-typed `Error` records instead of throwing exceptions for expected business errors.

---

## 2. Conventional Commits

All commits follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification:

```
<type>(<scope>): <short description>

[optional body]
```

### Commit Types
- `feat`: A new feature or endpoint.
- `fix`: A bug fix.
- `refactor`: Code restructuring without changing external behavior or fixing bugs.
- `perf`: A code change that improves execution performance.
- `test`: Adding or updating tests.
- `docs`: Documentation-only changes.
- `chore`: Maintenance tasks, dependency updates, or configuration changes.

### Common Scopes
- `iam`: Identity, Access Management, Profiles, and Fleet bounded context.
- `matchmaking`: Return routes, shipment requests, match proposals, negotiation messages, and route matching.
- `execution`: Trips, live GPS tracking points, deviation alerts, IoT devices, and incidents.
- `billing`: Subscription plans, subscriptions, freight settlements, and cancellation compensations.
- `reputation`: Ratings, reputation summaries, and loyalty points.
- `shared`: Shared kernel, pipeline behaviors, or domain abstractions.

---

## 3. Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Access to MySQL 8 (`trazza_database`)
- JetBrains Rider, Visual Studio, or VS Code

### Local Environment Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/StackRoot-1ASI0730-2620-8084/trazza-platform.git
   cd trazza-platform
   ```

2. Copy `.env.example` to `.env` and configure `DATABASE_URL`, `DATABASE_USER`, `DATABASE_PASSWORD`, `DATABASE_SCHEMA`, `TokenSettings__Secret`, `PersonalData__EncryptionKey`, and `PersonalData__HashKey`.

3. Restore, build, and run the solution:
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project Trazza.Platform
   ```

4. Access the interactive API reference:
   - Scalar UI: `http://localhost:5222/scalar/v1`
   - OpenAPI document: `http://localhost:5222/openapi/v1.json`

---

## 4. Pull Request Process

1. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git checkout -b feature/your-feature-name
   ```
2. Verify that `dotnet build` succeeds with zero errors.
3. Commit your changes following conventional commit syntax.
4. Push to your branch and open a Pull Request into `develop`.
