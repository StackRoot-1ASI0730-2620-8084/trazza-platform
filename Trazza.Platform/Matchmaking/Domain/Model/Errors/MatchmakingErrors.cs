using Trazza.Platform.Shared.Domain.Model.Errors;

namespace Trazza.Platform.Matchmaking.Domain.Model.Errors;

public static class MatchmakingErrors
{
    public static readonly Error ReturnRouteNotFound =
        Error.NotFound("Matchmaking.ReturnRouteNotFound", "The specified return route was not found.");

    public static readonly Error ShipmentRequestNotFound =
        Error.NotFound("Matchmaking.ShipmentRequestNotFound", "The specified shipment request was not found.");

    public static readonly Error MatchProposalNotFound =
        Error.NotFound("Matchmaking.MatchProposalNotFound", "The specified match proposal was not found.");

    public static readonly Error CarrierProfileNotFound =
        Error.NotFound("Matchmaking.CarrierProfileNotFound", "The user does not have a carrier profile.");

    public static readonly Error MerchantProfileNotFound =
        Error.NotFound("Matchmaking.MerchantProfileNotFound", "The user does not have a merchant profile.");

    public static readonly Error VehicleNotOwned =
        Error.Forbidden("Matchmaking.VehicleNotOwned", "The vehicle does not belong to the carrier.");

    public static readonly Error DriverNotOwned =
        Error.Forbidden("Matchmaking.DriverNotOwned", "The driver does not belong to the carrier.");

    public static readonly Error InvalidAddress =
        Error.Validation("Matchmaking.InvalidAddress", "The address or its coordinates are not valid.");

    public static readonly Error InvalidCargo =
        Error.Validation("Matchmaking.InvalidCargo", "The weight and the volume must be greater than zero.");

    public static readonly Error InvalidDescription =
        Error.Validation("Matchmaking.InvalidDescription", "The description is required and has at most 500 characters.");

    public static readonly Error InvalidTimeWindow =
        Error.Validation("Matchmaking.InvalidTimeWindow", "The end of the time window must be after its start.");

    public static readonly Error DepartureInPast =
        Error.Validation("Matchmaking.DepartureInPast", "The departure must be in the future.");

    public static readonly Error InvalidMaxShipments =
        Error.Validation("Matchmaking.InvalidMaxShipments", "A route accepts between 1 and 2 shipments.");

    public static readonly Error InvalidRate =
        Error.Validation("Matchmaking.InvalidRate", "The offered rate must be greater than zero.");

    public static readonly Error ReturnRouteNotPublished =
        Error.Conflict("Matchmaking.ReturnRouteNotPublished", "The return route is not published.");

    public static readonly Error ReturnRouteFull =
        Error.Conflict("Matchmaking.ReturnRouteFull", "The return route already has all its shipments.");

    public static readonly Error ReturnRouteNotCancellable =
        Error.Conflict("Matchmaking.ReturnRouteNotCancellable", "The return route can no longer be cancelled.");

    public static readonly Error CapacityExceeded =
        Error.Conflict("Matchmaking.CapacityExceeded", "The shipment exceeds the remaining capacity of the vehicle.");

    public static readonly Error ShipmentRequestNotOpen =
        Error.Conflict("Matchmaking.ShipmentRequestNotOpen", "The shipment request is not open.");

    public static readonly Error ShipmentRequestNotCancellable =
        Error.Conflict("Matchmaking.ShipmentRequestNotCancellable", "The shipment request can no longer be cancelled.");

    public static readonly Error ProposalAlreadyExists =
        Error.Conflict("Matchmaking.ProposalAlreadyExists", "There is already a proposal for this shipment and route.");

    public static readonly Error ProposalNotPending =
        Error.Conflict("Matchmaking.ProposalNotPending", "The proposal is not pending.");

    public static readonly Error NotProposalParticipant =
        Error.Forbidden("Matchmaking.NotProposalParticipant", "The user is not part of this proposal.");

    public static readonly Error NotShipmentOwner =
        Error.Forbidden("Matchmaking.NotShipmentOwner", "Only the merchant that owns the shipment can do this.");

    public static readonly Error NotRouteOwner =
        Error.Forbidden("Matchmaking.NotRouteOwner", "Only the carrier that owns the route can do this.");

    public static readonly Error EmptyMessage =
        Error.Validation("Matchmaking.EmptyMessage", "The message cannot be empty.");

    public static readonly Error ChatClosed =
        Error.Conflict("Matchmaking.ChatClosed", "The chat of this proposal is closed.");

    public static readonly Error ProposalHasTrip =
        Error.Conflict("Matchmaking.ProposalHasTrip", "The proposal already has a trip. Cancel the trip instead.");

    public static readonly Error MessageTooLong =
        Error.Validation("Matchmaking.MessageTooLong", "The message cannot have more than 2000 characters.");
}
