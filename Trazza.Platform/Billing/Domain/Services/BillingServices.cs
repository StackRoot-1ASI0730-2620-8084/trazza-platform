using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Execution.Interfaces.Acl;
using Trazza.Platform.Shared.Domain.Model;
 
namespace Trazza.Platform.Billing.Domain.Services;
 
public record SubscribedVehicle(Guid Id, string Plate);
 
public record SubscriptionView(
    Subscription Subscription,
    Plan Plan,
    List<SubscribedVehicle> Vehicles,
    List<SubscriptionPayment> Payments);
 
public record FreightSettlementView(FreightSettlement Settlement, TripSnapshot Trip, bool IsCarrier);
 
public record CompensationView(CancellationCompensation Compensation, TripSnapshot Trip, bool IsCarrier);
 
public interface ISubscriptionCommandService
{
    Task<Result<SubscriptionView, CommandError>> Handle(SubscribeCommand command, CancellationToken ct = default);
 
    Task<Result<SubscriptionView, CommandError>> Handle(RenewSubscriptionCommand command, CancellationToken ct = default);
 
    Task<Result<SubscriptionView, CommandError>> Handle(CancelSubscriptionCommand command, CancellationToken ct = default);
}
 
public interface ISettlementCommandService
{
    Task<Result<FreightSettlementView, CommandError>> Handle(MarkFreightPaidCommand command, CancellationToken ct = default);
 
    Task<Result<FreightSettlementView, CommandError>> Handle(ConfirmFreightReceivedCommand command,
        CancellationToken ct = default);
 
    Task<Result<CompensationView, CommandError>> Handle(UpdateCompensationCommand command, CancellationToken ct = default);
}
 
public interface IBillingQueryService
{
    Task<List<Plan>> ListPlansAsync(CancellationToken ct = default);
 
    Task<List<SubscriptionView>> ListMySubscriptionsAsync(ulong userId, CancellationToken ct = default);
 
    Task<List<FreightSettlementView>> ListMySettlementsAsync(ulong userId, CancellationToken ct = default);
 
    Task<List<CompensationView>> ListMyCompensationsAsync(ulong userId, CancellationToken ct = default);
}