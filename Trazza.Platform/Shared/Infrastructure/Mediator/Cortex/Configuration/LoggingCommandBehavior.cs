using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cortex.Mediator.Commands;
using Microsoft.Extensions.Logging;

namespace Trazza.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;

/// <summary>
///     Pipeline behavior that logs the start, the end and the duration of every command.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
public class LoggingCommandBehavior<TCommand>(ILogger<LoggingCommandBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(TCommand command, CommandHandlerDelegate next, CancellationToken cancellationToken)
    {
        var commandName = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling command {CommandName}", commandName);
        try
        {
            await next();
            logger.LogInformation("Handled command {CommandName} in {ElapsedMilliseconds} ms", commandName,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Command {CommandName} failed after {ElapsedMilliseconds} ms", commandName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
