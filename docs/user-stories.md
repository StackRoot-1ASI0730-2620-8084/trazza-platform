# Trazza Platform — REST API Technical Stories

## Overview

This document contains API-focused technical stories for integrating `trazza-webapp` with the Trazza Platform REST API (ASP.NET Core / .NET 10).

Common conventions:
- Base path: `/api/v1`
- Content-Type: `application/json`
- Error responses follow RFC 9457 `ProblemDetails` (`type`, `title`, `status`, `detail`, `instance`, `code`).
- Authentication: `Authorization: Bearer <token>`.

## Traceability

| Technical Story | Bounded Context | Endpoints |
|:--|:--|:--|
| TS-IAM-001 Sign Up & Sign In | IAM | `POST /authentication/sign-up`, `POST /authentication/sign-in`, `POST /authentication/refresh`, `POST /authentication/sign-out` |
| TS-IAM-002 Manage User Identity & Roles | IAM | `GET /users/me`, `PUT /users/me/identity`, `POST /users/me/roles`, `PUT /users/me/merchant-profile`, `GET/POST /users/me/identity-verifications` |
| TS-IAM-003 Manage Fleet & Saved Addresses | IAM | `GET/POST/PUT/DELETE /vehicles`, `GET/POST/PUT/DELETE /drivers`, `GET/POST/DELETE /saved-addresses` |
| TS-MAT-001 Publish & Search Return Routes | Matchmaking | `POST /return-routes`, `GET /return-routes`, `GET /return-routes/mine`, `GET /return-routes/search`, `POST /return-routes/{id}/cancel` |
| TS-MAT-002 Suggested Shipments for Route | Matchmaking | `GET /return-routes/{id}/suggested-shipments`, `GET /return-routes/{id}/proposals` |
| TS-MAT-003 Publish & Search Shipment Requests | Matchmaking | `POST /shipment-requests`, `GET /shipment-requests/open`, `GET /shipment-requests/mine`, `GET /shipment-requests/search`, `POST /shipment-requests/{id}/cancel` |
| TS-MAT-004 Suggested Routes for Request | Matchmaking | `GET /shipment-requests/{id}/suggested-routes`, `GET /shipment-requests/{id}/proposals` |
| TS-MAT-005 Create & Decide Match Proposals | Matchmaking | `POST /match-proposals`, `GET /match-proposals`, `POST /match-proposals/{id}/accept`, `POST /match-proposals/{id}/reject`, `POST /match-proposals/{id}/cancel` |
| TS-MAT-006 Proposal Negotiation Messages | Matchmaking | `GET /match-proposals/{id}/messages`, `POST /match-proposals/{id}/messages`, `POST /match-proposals/{id}/messages/read` |
| TS-EXE-001 Trip Lifecycle & Driver Assignment | Execution | `GET /trips`, `GET /trips/{id}`, `GET /trips/{id}/status-history`, `PUT /trips/{id}/driver`, `POST /trips/{id}/pick-up`, `POST /trips/{id}/deliver`, `POST /trips/{id}/confirm`, `POST /trips/{id}/cancel` |
| TS-EXE-002 Live GPS Tracking & Deviation Alerts | Execution | `GET/POST /trips/{id}/tracking-points`, `GET /trips/{id}/deviation-alerts`, `POST /trips/{id}/deviation-alerts/{alertId}/acknowledge`, `GET /iot-devices` |
| TS-EXE-003 Trip Incidents & Evidence | Execution | `GET/POST /trips/{id}/incidents`, `POST /trips/{id}/incidents/{incidentId}/evidences`, `POST /trips/{id}/incidents/{incidentId}/resolve` |
| TS-BIL-001 Plans & Carrier Subscriptions | Billing | `GET /plans`, `GET /subscriptions`, `POST /subscriptions`, `POST /subscriptions/{id}/renew`, `POST /subscriptions/{id}/cancel` |
| TS-BIL-002 Freight Settlements & Compensations | Billing | `GET /freight-settlements`, `POST /freight-settlements/{tripId}/mark-paid`, `POST /freight-settlements/{tripId}/confirm`, `GET /cancellation-compensations`, `POST /cancellation-compensations/{tripId}/confirm-paid` |
| TS-REP-001 Trip Ratings & Reputation Summary | Reputation | `POST /ratings`, `GET /ratings/received`, `GET /ratings/given`, `GET /ratings/summary/{userId}` |
| TS-REP-002 Loyalty Points & Redemptions | Reputation | `GET /loyalty/points`, `POST /loyalty/redemptions` |

---

## Bounded Context: IAM & Fleet

### TS-IAM-001 — Sign Up & Sign In
- **Scenario: Successful sign-up**
  - Given `POST /api/v1/authentication/sign-up` with `email`, `phone`, `password`, and `role`
  - Then the API creates the user account, activates the requested role profile, and returns JWT access and refresh tokens.
- **Scenario: Sign-in and token refresh**
  - Given `POST /api/v1/authentication/sign-in` with valid credentials
  - Then the API returns the authenticated user and token pair; `POST /api/v1/authentication/refresh` rotates the refresh token.

### TS-IAM-002 — Manage User Identity & Roles
- **Scenario: Register personal identity & verify with RENIEC**
  - Given `PUT /api/v1/users/me/identity` with document type, number, and personal names
  - Then the document number is encrypted with AES-GCM and can be verified via `POST /api/v1/users/me/identity-verifications`.

### TS-IAM-003 — Manage Fleet & Saved Addresses
- **Scenario: Register vehicles and drivers**
  - Carriers manage their fleet through `/api/v1/vehicles` and `/api/v1/drivers`.
  - Users manage frequent origin/destination addresses through `/api/v1/saved-addresses`.

---

## Bounded Context: Matchmaking & Routing

### TS-MAT-001 & TS-MAT-002 — Return Routes & Suggested Shipments
- Carriers publish return routes (`POST /api/v1/return-routes`) specifying vehicle, origin/destination coordinates, corridor radius, departure window, and available capacity.
- `GET /api/v1/return-routes/{returnRouteId}/suggested-shipments` evaluates open shipment requests with `RouteMatchingService`.

### TS-MAT-003 & TS-MAT-004 — Shipment Requests & Suggested Routes
- Merchants publish shipment requests (`POST /api/v1/shipment-requests`) specifying cargo weight, volume, declared value, and pickup/delivery locations.
- `GET /api/v1/shipment-requests/{shipmentRequestId}/suggested-routes` returns compatible active return routes.

### TS-MAT-005 & TS-MAT-006 — Match Proposals & Negotiation Messages
- Carriers or merchants submit match proposals (`POST /api/v1/match-proposals`) and exchange messages or counteroffers (`POST /api/v1/match-proposals/{matchProposalId}/messages`).
- Accepting a proposal (`POST /api/v1/match-proposals/{matchProposalId}/accept`) reserves route capacity and automatically creates the `Trip` in the `Execution` bounded context.

---

## Bounded Context: Service Execution & Monitoring

### TS-EXE-001 — Trip Lifecycle
- Accepted proposals open a `Trip` (`/api/v1/trips`), which progresses through driver assignment (`PUT /{tripId}/driver`), cargo pick-up (`POST /{tripId}/pick-up`), delivery (`POST /{tripId}/deliver`), and merchant confirmation (`POST /{tripId}/confirm`).

### TS-EXE-002 — Live GPS Tracking & Deviation Alerts
- `POST /api/v1/trips/{tripId}/tracking-points` records GPS telemetry and checks the distance to the planned route corridor, creating a `DeviationAlert` when the threshold is exceeded.

### TS-EXE-003 — Trip Incidents
- Participants report incidents (`POST /api/v1/trips/{tripId}/incidents`), attach evidence (`POST .../evidences`), and resolve them (`POST .../resolve`).

---

## Bounded Context: Payment & Billing

### TS-BIL-001 — Plans & Carrier Subscriptions
- Carriers inspect plans (`GET /api/v1/plans`) and manage subscriptions (`POST /api/v1/subscriptions`, `renew`, `cancel`).

### TS-BIL-002 — Freight Settlements & Cancellation Compensations
- Completed trips generate freight settlements (`/api/v1/freight-settlements`) that merchants mark as paid and carriers confirm.
- Cancelled trips after commitment generate compensations (`/api/v1/cancellation-compensations`).

---

## Bounded Context: Loyalty & Reputation

### TS-REP-001 — Trip Ratings & Reputation Summary
- After trip completion, carriers and merchants rate each other (`POST /api/v1/ratings`) and query reputation summaries (`GET /api/v1/ratings/summary/{userId}`).

### TS-REP-002 — Loyalty Points
- Completed trips and ratings award loyalty points (`GET /api/v1/loyalty/points`) that users can redeem (`POST /api/v1/loyalty/redemptions`).
