using System.Threading;
using System.Threading.Tasks;
using Cortex.Mediator.Commands;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;

/// <summary>
///     Pipeline behavior that commits the changes of a command through the unit of work
///     only after the command finished without errors. If the command throws, nothing is saved.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
public class TransactionCommandBehavior<TCommand>(IUnitOfWork unitOfWork)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(TCommand command, CommandHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
