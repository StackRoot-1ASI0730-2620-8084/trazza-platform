using System;
using System.Threading;
using System.Threading.Tasks;
using Cortex.Mediator.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Trazza.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;

/// <summary>
///     Pipeline behavior that classifies the database exceptions thrown while a command runs.
///     It logs them with the command name and rethrows them, so the GlobalExceptionHandler
///     turns them into the right ProblemDetails response (409 for conflicts, 500 for the rest).
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
public class ExceptionHandlingCommandBehavior<TCommand>(ILogger<ExceptionHandlingCommandBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(TCommand command, CommandHandlerDelegate next, CancellationToken cancellationToken)
    {
        var commandName = typeof(TCommand).Name;
        try
        {
            await next();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Command {CommandName} was cancelled", commandName);
            throw;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogWarning(exception,
                "Command {CommandName} found a concurrency conflict: the data was changed by another request",
                commandName);
            throw;
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception,
                "Command {CommandName} could not save its changes: {DatabaseMessage}",
                commandName, exception.InnerException?.Message ?? exception.Message);
            throw;
        }
    }
}
