using System;
using Logging.Domain.Entities;
using Logging.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers;

public abstract class ActivityLogConsumerBase<TMessage>(IActivityLogRepository repository, ILogger logger) : IConsumer<TMessage> where TMessage : class
{
    private readonly IActivityLogRepository repository = repository;
    private readonly ILogger logger = logger;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var message = context.Message;

        var log = BuildLog(message);

        try
        {
            await repository.AddAsync(log, context.CancellationToken);
            logger.LogInformation("Activity log saved for {MessageType}", typeof(TMessage).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save activity log for {MessageType}", typeof(TMessage).Name);
            throw;
        }
    }

    protected abstract ActivityLog BuildLog(TMessage message);
}
