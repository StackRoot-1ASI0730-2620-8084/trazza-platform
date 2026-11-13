using System.Threading;
using System.Threading.Tasks;
using Cortex.Mediator.Commands;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;

public class TransactionCommandBehavior<TCommand>(IUnitOfWork unitOfWork)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(TCommand command, CommandHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
