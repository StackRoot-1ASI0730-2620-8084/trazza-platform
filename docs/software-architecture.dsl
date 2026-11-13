workspace "Trazza Platform" "C4 model of the Trazza logistics SaaS platform." {

    model {
        carrier = person "Carrier" "Transportista with empty return trips who publishes routes and executes trips."
        merchant = person "Merchant" "Comerciante that publishes shipment requests and confirms deliveries."

        trazza = softwareSystem "Trazza" "Connects carriers with empty return trips and merchants who need to ship goods in Lima." {
            landingPage = container "Landing Page" "Public marketing site." "HTML, CSS, JavaScript"
            webApp = container "Web Application" "Single page application for carriers and merchants." "Vue 3, PrimeVue, Vite"
            api = container "Trazza Platform API" "RESTful Web Services organized in DDD bounded contexts." "ASP.NET Core, .NET 10, C# 14" {
                iam = component "IAM & Fleet" "Users, sign-up, sign-in, JWT, AES-GCM identity, carrier/merchant profiles, vehicles, drivers, and saved addresses." "Bounded Context"
                matchmaking = component "Matchmaking & Routing" "Return routes, shipment requests, match proposals, negotiation messages, and RouteMatchingService." "Bounded Context"
                execution = component "Service Execution & Monitoring" "Trips, live GPS tracking points, route deviation alerts, IoT devices, and incidents." "Bounded Context"
                billing = component "Payment & Billing" "Subscription plans, carrier subscriptions, freight settlements, and cancellation compensations." "Bounded Context"
                reputation = component "Loyalty & Reputation" "Trip ratings, user reputation summaries, and loyalty points." "Bounded Context"
                shared = component "Shared Kernel" "Money, Address, GeoLocation, Result, Error, interceptors, and pipeline behaviors." "Bounded Context"
            }
            database = container "Database" "Stores users, profiles, fleet, routes, shipment requests, proposals, trips, billing, and ratings." "MySQL 8" "Database"
        }

        reniecProvider = softwareSystem "Identity Verification Provider" "Identity verification service (simulated RENIEC provider)." "External"

        carrier -> landingPage "Visits"
        merchant -> landingPage "Visits"
        carrier -> webApp "Publishes routes, negotiates proposals, and executes trips"
        merchant -> webApp "Publishes shipment requests, negotiates proposals, and tracks trips"
        landingPage -> webApp "Redirects to sign-in and sign-up"
        webApp -> api "Calls" "JSON/HTTPS, Bearer JWT"
        api -> database "Reads and writes" "EF Core, MySQL 8"
        iam -> reniecProvider "Verifies personal identity" "IIdentityVerificationProvider"

        webApp -> iam "Authenticates and manages fleet/profiles"
        matchmaking -> iam "Queries carrier/merchant profiles and vehicles through IIamContextFacade"
        matchmaking -> execution "Creates trip on proposal acceptance through IExecutionContextFacade"
        execution -> matchmaking "Queries proposal and route context through IMatchmakingContextFacade"
        billing -> execution "Queries trip context through IExecutionContextFacade"
        reputation -> execution "Queries completed trip parties through IExecutionContextFacade"
        iam -> database "Persists users, profiles, vehicles, drivers, and addresses"
        matchmaking -> database "Persists return routes, shipment requests, proposals, and messages"
        execution -> database "Persists trips, tracking points, deviation alerts, and incidents"
        billing -> database "Persists plans, subscriptions, settlements, and compensations"
        reputation -> database "Persists ratings and loyalty point entries"
    }

    views {
        systemContext trazza "SystemContext" {
            include *
            autolayout lr
        }

        container trazza "Containers" {
            include *
            autolayout lr
        }

        component api "Components" {
            include *
            autolayout lr
        }

        styles {
            element "Person" {
                shape person
                background #0b3d91
                color #ffffff
            }
            element "Software System" {
                background #1168bd
                color #ffffff
            }
            element "Container" {
                background #438dd5
                color #ffffff
            }
            element "Component" {
                background #85bbf0
                color #000000
            }
            element "Database" {
                shape cylinder
            }
            element "External" {
                background #999999
                color #ffffff
            }
        }
    }
}
