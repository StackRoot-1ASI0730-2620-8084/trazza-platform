# Changelog

All notable changes to the **Trazza Platform** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-11-13

### Added
- **IAM & Fleet Bounded Context (`Iam`)**:
  - User registration, authentication, JWT access tokens, and refresh token rotation (`/api/v1/authentication`).
  - AES-GCM encrypted personal identity storage (`PersonIdentity`), simulated RENIEC verification (`IdentityVerification`), and merchant/carrier role activation (`/api/v1/users/me`).
  - Carrier fleet management (`Vehicle`, `Driver`) and merchant saved addresses (`SavedAddress`).
- **Matchmaking & Routing Bounded Context (`Matchmaking`)**:
  - Return route publication, search, and shipment suggestions (`/api/v1/return-routes`).
  - Shipment request publication, search, and route suggestions (`/api/v1/shipment-requests`).
  - Match proposal workflow (`pending`, `accepted`, `rejected`, `cancelled`, `expired`), in-proposal negotiation messages (`/api/v1/match-proposals/{id}/messages`), and automatic trip creation upon proposal acceptance.
- **Service Execution & Monitoring Bounded Context (`Execution`)**:
  - Trip lifecycle management from driver assignment and cargo pick-up to delivery and merchant confirmation (`/api/v1/trips`).
  - Live GPS tracking points (`TrackingPoint`), route corridor deviation alerts (`DeviationAlert`), incident reporting with evidence (`Incident`), and IoT device listings (`/api/v1/iot-devices`).
- **Payment & Billing Bounded Context (`Billing`)**:
  - Subscription plans catalog (`/api/v1/plans`) and carrier subscriptions with simulated payments, renewal, and cancellation (`/api/v1/subscriptions`).
  - Trip freight settlements (`/api/v1/freight-settlements`) and cancellation compensations (`/api/v1/cancellation-compensations`).
- **Loyalty & Reputation Bounded Context (`Reputation`)**:
  - Mutual post-trip ratings and user reputation summaries (`/api/v1/ratings`).
  - Loyalty points ledger and redemptions (`/api/v1/loyalty`).
- **Shared Kernel (`Shared`)**:
  - `AggregateRoot`, `Result<T>`, `Error`, `Money`, `Address`, `GeoLocation`, `LimaDistricts`, `LimaCalendar`, EF Core interceptors, Cortex Mediator pipeline behaviors, RFC 9457 `ProblemDetails`, English/Spanish localization, and `/api/v1/health`.
