using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TradingService.Application.Contracts.Messaging;
using TradingService.Domain.Entities;
using TradingService.Domain.Interfaces;

namespace TradingService.Infrastructure.Messaging
{
    public class RabbitMqClient : ITradingServiceClient, IAsyncDisposable, IDisposable
    {
        private const string FreeBalanceQueue = "user_free_balance_rpc_queue";
        private const string StockCountQueue = "user_stock_count_rpc_queue";
        private const string OrdersExchange = "";  // default exchange
        private const string OrdersRoutingKey = "order_executed_queue";

        private readonly IConnectionFactory _factory
            = new ConnectionFactory { HostName = "localhost" };
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>>
            _callbackMapper = new();

        private IConnection? _connection;
        private IChannel? _channel;
        private string? _replyQueueName;

        public async Task StartAsync()
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            var declareOk = await _channel.QueueDeclareAsync();
            _replyQueueName = declareOk.QueueName;

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                var corrId = ea.BasicProperties.CorrelationId;
                if (!string.IsNullOrEmpty(corrId)
                    && _callbackMapper.TryRemove(corrId, out var tcs))
                {
                    var resp = Encoding.UTF8.GetString(ea.Body.ToArray());
                    tcs.TrySetResult(resp);
                }
                return Task.CompletedTask;
            };

            await _channel.BasicConsumeAsync(
                queue: _replyQueueName,
                autoAck: true,
                consumer: consumer
            );
        }

        public async Task<decimal> RequestUserFreeBalanceToOrders(
            string userId,
            CancellationToken cancellationToken = default)
        {
            EnsureChannel();

            var corrId = Guid.NewGuid().ToString();
            var props = new BasicProperties
            {
                CorrelationId = corrId,
                ReplyTo = _replyQueueName
            };

            var tcs = new TaskCompletionSource<string>();
            _callbackMapper[corrId] = tcs;

            var body = Encoding.UTF8.GetBytes(userId);
            await _channel!.BasicPublishAsync(
                exchange: "",
                routingKey: FreeBalanceQueue,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);

            var resp = await tcs.Task;
            return decimal.Parse(resp, System.Globalization.CultureInfo.InvariantCulture);
        }

        public async Task PublishOrderExecutedAsync(
            OrderRequest req,
            CancellationToken ct = default)
        {
            EnsureChannel();

            var json = JsonSerializer.Serialize(req);
            var data = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel!.BasicPublishAsync(
                exchange: OrdersExchange,
                routingKey: OrdersRoutingKey,
                mandatory: false,
                basicProperties: props,
                body: data,
                cancellationToken: ct);
        }


        public async Task<int> RequestUserSpecificStocksAmountForSale(
            string userId,
            string stockTicker,
            CancellationToken ct)
        {
            EnsureChannel();

            var corrId = Guid.NewGuid().ToString();
            var props = new BasicProperties
            {
                CorrelationId = corrId,
                ReplyTo = _replyQueueName
            };

            var tcs = new TaskCompletionSource<string>();
            _callbackMapper[corrId] = tcs;

            var reqDto = new { UserId = userId, StockTicker = stockTicker };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(reqDto));

            await _channel!.BasicPublishAsync(
                exchange: "",
                routingKey: StockCountQueue,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            var resp = await tcs.Task;
            return int.Parse(resp);
        }

        private void EnsureChannel()
        {
            if (_channel == null || string.IsNullOrEmpty(_replyQueueName))
                throw new InvalidOperationException("Call StartAsync() first.");
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
        }
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

    }
}
