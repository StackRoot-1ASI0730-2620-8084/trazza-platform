# Trazza Platform — REST API Endpoints

This catalog lists every endpoint exposed by `Trazza.Platform`. All routes are documented interactively with OpenAPI and Scalar at `/scalar/v1`.

## Conventions

- Base path: `/api/v1`, kebab-case resource names, lowercase URLs.
- Content type: `application/json`, camelCase properties, timestamps in UTC ISO 8601.
- Authentication: `Authorization: Bearer <token>` obtained from `POST /api/v1/authentication/sign-in` or `sign-up`.
- Errors: RFC 9457 `ProblemDetails` with `status`, `title`, `detail`, `instance`, and the domain error `code` extension. Messages are localized with `Accept-Language: es` or `en`.

## IAM & Fleet

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `POST` | `/api/v1/authentication/sign-up` | Sign up | No | Registers a new user and returns the authenticated user with access and refresh tokens. |
| `POST` | `/api/v1/authentication/sign-in` | Sign in | No | Authenticates a user with email and password and returns JWT access and refresh tokens. |
| `POST` | `/api/v1/authentication/refresh` | Refresh token | No | Issues a new access and refresh token pair from a valid refresh token. |
| `POST` | `/api/v1/authentication/sign-out` | Sign out | Yes | Revokes the active refresh token for the user session. |
| `GET` | `/api/v1/users/me` | Get current user | Yes | Gets the profile, roles, and identity status of the authenticated user. |
| `PUT` | `/api/v1/users/me/identity` | Update personal identity | Yes | Registers or updates encrypted personal identity (DNI/CE) and contact details. |
| `POST` | `/api/v1/users/me/roles` | Activate role | Yes | Activates the carrier or merchant role for the authenticated user. |
| `PUT` | `/api/v1/users/me/merchant-profile` | Update merchant profile | Yes | Updates the merchant business profile and RUC information. |
| `GET` | `/api/v1/users/me/identity-verifications` | List identity verifications | Yes | Lists identity verification attempts for the authenticated user. |
| `POST` | `/api/v1/users/me/identity-verifications` | Verify identity | Yes | Runs identity verification against the simulated RENIEC provider. |
| `GET` | `/api/v1/vehicles` | List vehicles | Yes | Lists vehicles belonging to the authenticated carrier. |
| `POST` | `/api/v1/vehicles` | Register vehicle | Yes | Registers a vehicle in the carrier's fleet. |
| `PUT` | `/api/v1/vehicles/{vehicleId}` | Update vehicle | Yes | Updates a vehicle's attributes or status by its public `Guid`. |
| `DELETE` | `/api/v1/vehicles/{vehicleId}` | Delete vehicle | Yes | Removes a vehicle from the carrier's fleet. |
| `GET` | `/api/v1/drivers` | List drivers | Yes | Lists drivers registered by the authenticated carrier. |
| `POST` | `/api/v1/drivers` | Register driver | Yes | Adds a driver to the carrier's fleet. |
| `PUT` | `/api/v1/drivers/{driverId}` | Update driver | Yes | Updates driver license and contact details. |
| `DELETE` | `/api/v1/drivers/{driverId}` | Delete driver | Yes | Removes a driver from the carrier's fleet. |
| `GET` | `/api/v1/saved-addresses` | List saved addresses | Yes | Lists saved addresses of the authenticated user. |
| `POST` | `/api/v1/saved-addresses` | Create saved address | Yes | Saves a frequent origin or destination address. |
| `DELETE` | `/api/v1/saved-addresses/{savedAddressId}` | Delete saved address | Yes | Deletes a saved address. |

## Matchmaking & Routing

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `POST` | `/api/v1/return-routes` | Publish return route | Yes | Publishes a carrier return route with vehicle capacity, corridor, and time window. |
| `GET` | `/api/v1/return-routes` | List return routes | Yes | Lists active return routes. |
| `GET` | `/api/v1/return-routes/mine` | List my return routes | Yes | Lists return routes published by the authenticated carrier. |
| `GET` | `/api/v1/return-routes/search` | Search return routes | Yes | Searches return routes by origin, destination, date, and capacity. |
| `GET` | `/api/v1/return-routes/{returnRouteId}` | Get return route | Yes | Gets a return route by its public `Guid`. |
| `GET` | `/api/v1/return-routes/{returnRouteId}/proposals` | List route proposals | Yes | Lists match proposals associated with a return route. |
| `GET` | `/api/v1/return-routes/{returnRouteId}/suggested-shipments` | Suggest shipments | Yes | Returns compatible open shipment requests ranked by `RouteMatchingService`. |
| `POST` | `/api/v1/return-routes/{returnRouteId}/cancel` | Cancel return route | Yes | Cancels an active return route. |
| `POST` | `/api/v1/shipment-requests` | Publish shipment request | Yes | Publishes a merchant shipment request with cargo weight, volume, and route endpoints. |
| `GET` | `/api/v1/shipment-requests` | List shipment requests | Yes | Lists shipment requests. |
| `GET` | `/api/v1/shipment-requests/open` | List open shipment requests | Yes | Lists open shipment requests available for matching. |
| `GET` | `/api/v1/shipment-requests/mine` | List my shipment requests | Yes | Lists shipment requests created by the authenticated merchant. |
| `GET` | `/api/v1/shipment-requests/search` | Search shipment requests | Yes | Searches shipment requests by districts, cargo type, and pickup window. |
| `GET` | `/api/v1/shipment-requests/{shipmentRequestId}` | Get shipment request | Yes | Gets a shipment request by its public `Guid`. |
| `GET` | `/api/v1/shipment-requests/{shipmentRequestId}/proposals` | List request proposals | Yes | Lists match proposals submitted for a shipment request. |
| `GET` | `/api/v1/shipment-requests/{shipmentRequestId}/suggested-routes` | Suggest return routes | Yes | Returns compatible return routes ranked by `RouteMatchingService`. |
| `POST` | `/api/v1/shipment-requests/{shipmentRequestId}/cancel` | Cancel shipment request | Yes | Cancels an open shipment request. |
| `POST` | `/api/v1/match-proposals` | Create match proposal | Yes | Submits a match proposal between a return route and a shipment request. |
| `GET` | `/api/v1/match-proposals` | List match proposals | Yes | Lists match proposals involving the authenticated user. |
| `GET` | `/api/v1/match-proposals/{matchProposalId}` | Get match proposal | Yes | Gets a match proposal by its public `Guid`. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/accept` | Accept proposal | Yes | Accepts a pending proposal, reserves capacity, and creates the execution trip. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/reject` | Reject proposal | Yes | Rejects a pending match proposal. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/cancel` | Cancel proposal | Yes | Cancels a pending match proposal. |
| `GET` | `/api/v1/match-proposals/{matchProposalId}/messages` | List messages | Yes | Lists negotiation messages exchanged within a match proposal. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/messages` | Send message | Yes | Sends a text or counteroffer message in a match proposal thread. |
| `POST` | `/api/v1/match-proposals/{matchProposalId}/messages/read` | Mark messages read | Yes | Marks proposal messages as read by the participant. |

## Service Execution & Monitoring

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `GET` | `/api/v1/trips` | List trips | Yes | Lists trips where the authenticated user participates as carrier or merchant. |
| `GET` | `/api/v1/trips/{tripId}` | Get trip | Yes | Gets trip details by public `Guid`. |
| `GET` | `/api/v1/trips/{tripId}/status-history` | Get status history | Yes | Gets the chronological status transitions of a trip. |
| `PUT` | `/api/v1/trips/{tripId}/driver` | Assign driver | Yes | Assigns a carrier driver and optional IoT device to the trip. |
| `POST` | `/api/v1/trips/{tripId}/pick-up` | Confirm pick-up | Yes | Marks cargo as picked up and starts in-transit tracking. |
| `POST` | `/api/v1/trips/{tripId}/deliver` | Mark delivered | Yes | Marks the trip cargo as delivered at the destination. |
| `POST` | `/api/v1/trips/{tripId}/confirm` | Confirm delivery | Yes | Merchant confirms reception of the cargo and completes the trip. |
| `POST` | `/api/v1/trips/{tripId}/cancel` | Cancel trip | Yes | Cancels a trip before completion and triggers applicable compensation rules. |
| `GET` | `/api/v1/trips/{tripId}/tracking-points` | List tracking points | Yes | Lists GPS tracking coordinates recorded for the trip. |
| `POST` | `/api/v1/trips/{tripId}/tracking-points` | Record tracking point | Yes | Records a live GPS coordinate and evaluates route corridor deviation. |
| `GET` | `/api/v1/trips/{tripId}/deviation-alerts` | List deviation alerts | Yes | Lists route deviation alerts triggered during the trip. |
| `POST` | `/api/v1/trips/{tripId}/deviation-alerts/{alertId}/acknowledge` | Acknowledge alert | Yes | Acknowledges a route deviation alert. |
| `GET` | `/api/v1/trips/{tripId}/incidents` | List incidents | Yes | Lists incidents reported on the trip. |
| `POST` | `/api/v1/trips/{tripId}/incidents` | Report incident | Yes | Reports an operational incident during or after the trip. |
| `POST` | `/api/v1/trips/{tripId}/incidents/{incidentId}/evidences` | Attach evidence | Yes | Attaches evidence metadata to a reported incident. |
| `POST` | `/api/v1/trips/{tripId}/incidents/{incidentId}/resolve` | Resolve incident | Yes | Resolves an open trip incident. |
| `GET` | `/api/v1/iot-devices` | List IoT devices | Yes | Lists GPS/IoT tracking devices available to the carrier. |

## Payment & Billing

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `GET` | `/api/v1/plans` | List subscription plans | Yes | Lists available carrier subscription plans and vehicle limits. |
| `GET` | `/api/v1/subscriptions` | Get subscription | Yes | Gets the current subscription and payment history of the carrier. |
| `POST` | `/api/v1/subscriptions` | Subscribe to plan | Yes | Subscribes the carrier to a plan with a simulated payment. |
| `POST` | `/api/v1/subscriptions/{subscriptionId}/renew` | Renew subscription | Yes | Renews an existing carrier subscription. |
| `POST` | `/api/v1/subscriptions/{subscriptionId}/cancel` | Cancel subscription | Yes | Cancels auto-renewal on an active subscription. |
| `GET` | `/api/v1/freight-settlements` | List settlements | Yes | Lists freight settlements for completed trips. |
| `POST` | `/api/v1/freight-settlements/{tripId}/mark-paid` | Mark settlement paid | Yes | Merchant marks a trip's freight settlement as paid. |
| `POST` | `/api/v1/freight-settlements/{tripId}/confirm` | Confirm settlement | Yes | Carrier confirms receipt of the freight settlement payment. |
| `GET` | `/api/v1/cancellation-compensations` | List compensations | Yes | Lists cancellation compensations for cancelled trips. |
| `POST` | `/api/v1/cancellation-compensations/{tripId}/confirm-paid` | Confirm compensation | Yes | Confirms payment of a cancellation compensation. |
| `POST` | `/api/v1/cancellation-compensations/{tripId}/waive` | Waive compensation | Yes | Waives a cancellation compensation. |
| `POST` | `/api/v1/cancellation-compensations/{tripId}/dispute` | Dispute compensation | Yes | Opens a dispute on a cancellation compensation. |

## Loyalty & Reputation

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `POST` | `/api/v1/ratings` | Submit rating | Yes | Submits a rating and comment for the counterpart of a completed trip. |
| `GET` | `/api/v1/ratings/received` | List received ratings | Yes | Lists ratings received by the authenticated user. |
| `GET` | `/api/v1/ratings/given` | List given ratings | Yes | Lists ratings submitted by the authenticated user. |
| `GET` | `/api/v1/ratings/summary/{userId}` | Get reputation summary | Yes | Returns the aggregate rating score and breakdown for a user `Guid`. |
| `GET` | `/api/v1/loyalty/points` | Get loyalty points | Yes | Gets the loyalty points balance and ledger entries of the authenticated user. |
| `POST` | `/api/v1/loyalty/redemptions` | Redeem loyalty points | Yes | Redeems loyalty points for platform benefits. |

## Shared

| Method | Route | Summary | Auth | Description |
|:--|:--|:--|:--|:--|
| `GET` | `/api/v1/health` | Database health check | No | Verifies connectivity with the MySQL database. |
