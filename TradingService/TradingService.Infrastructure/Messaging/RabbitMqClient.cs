using System.Collections.Concurrent;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TradingService.Domain.Interfaces;

namespace TradingService.Infrastructure.Messaging;

public class RabbitMqClient : ITradingServiceClient, IAsyncDisposable, IDisposable
{
    private const string QueueName = "user_free_balance_rpc_queue";

    private readonly IConnectionFactory _connectionFactory = new ConnectionFactory { HostName = "localhost" };
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _replyQueueName;

    public async Task StartAsync()
    {
        _connection = await _connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        QueueDeclareOk queueDeclareOk = await _channel.QueueDeclareAsync();
        _replyQueueName = queueDeclareOk.QueueName;
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += (model, ea) =>
        {
            string? correlationId = ea.BasicProperties.CorrelationId;

            if (false == string.IsNullOrEmpty(correlationId))
            {
                if (_callbackMapper.TryRemove(correlationId, out var tcs))
                {
                    var body = ea.Body.ToArray();
                    var response = Encoding.UTF8.GetString(body);
                    tcs.TrySetResult(response);
                }
            }

            return Task.CompletedTask;
        };
        await _channel.BasicConsumeAsync(_replyQueueName, true, consumer);
    }
    public async Task<decimal> RequestUserFreeBalanceToOrders(string userId, CancellationToken cancellationToken = default)
    {

        if (_channel == null || string.IsNullOrEmpty(_replyQueueName))
            throw new InvalidOperationException("RabbitMQ channel has not been initialized. Call StartAsync() first.");
        
        var correlationId = Guid.NewGuid().ToString();

        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = _replyQueueName
        };

        var tcs = new TaskCompletionSource<string>();
        _callbackMapper[correlationId] = tcs;
        
        var messageBytes = Encoding.UTF8.GetBytes(userId);

        await _channel.BasicPublishAsync<BasicProperties>(
            exchange: "",
            routingKey: QueueName,
            mandatory: false,
            basicProperties: props,
            body: messageBytes,
            cancellationToken: cancellationToken);

        var response = await tcs.Task;
        if (decimal.TryParse(response, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal balance))
        {
            Console.WriteLine($"{response}");
            return balance;
        }
        else
        {
            throw new Exception($"Failed to parse response '{response}' on decimal type.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
        }
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}