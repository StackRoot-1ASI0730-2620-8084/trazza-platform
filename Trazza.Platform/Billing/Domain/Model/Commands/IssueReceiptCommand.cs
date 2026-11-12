using Trazza.Platform.Billing.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Commands;

public record IssueReceiptCommand(
    SubscriptionPaymentId TransactionId,
    string Type,
    string CustomerName,
    string CustomerDocument);
