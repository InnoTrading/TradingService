using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace TradingService.Infrastructure.Messaging
{
    internal class RpcClientHelper
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper;

        public RpcClientHelper(ConnectionManager connectionManager, ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper)
        {
            _connectionManager = connectionManager;
            _callbackMapper = callbackMapper;
        }

        public async Task<string> CallRpcAsync(object payload, string routingKey, CancellationToken ct = default)
        {
            _connectionManager.EnsureChannel();

            var corrId = Guid.NewGuid().ToString();
            var props = new BasicProperties
            {
                CorrelationId = corrId,
                ReplyTo = _connectionManager.ReplyQueueName
            };

            var tcs = new TaskCompletionSource<string>();
            _callbackMapper[corrId] = tcs;

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

            await _connectionManager.Channel.BasicPublishAsync(
                exchange: "",
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            return await tcs.Task;
        }
        public async Task<string> CallRpcRawAsync(string rawString, string routingKey, CancellationToken ct = default)
        {
            _connectionManager.EnsureChannel();

            var corrId = Guid.NewGuid().ToString();
            var props = new BasicProperties
            {
                CorrelationId = corrId,
                ReplyTo = _connectionManager.ReplyQueueName
            };

            var tcs = new TaskCompletionSource<string>();
            _callbackMapper[corrId] = tcs;

            var body = Encoding.UTF8.GetBytes(rawString);

            await _connectionManager.Channel.BasicPublishAsync(
                exchange: "",
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            return await tcs.Task;
        }

    }
}
