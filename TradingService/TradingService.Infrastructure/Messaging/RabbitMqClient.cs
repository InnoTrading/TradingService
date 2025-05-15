using System.Collections.Concurrent;
using TradingService.Application.Contracts.Messaging;
using TradingService.Application.Interfaces;
using TradingService.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TradingService.Domain.Interfaces;

namespace TradingService.Infrastructure.Messaging
{
    public class RabbitMqClient : ITradingServiceClient, IAsyncDisposable, IDisposable
    {
        private const string AvailableBalanceQueue = "user_available_balance_rpc_queue";
        private const string StockCountQueue = "user_stock_count_rpc_queue";
        private const string OrdersExchange = "";
        private const string OrdersRoutingKey = "order_executed_queue";

        private readonly ConnectionManager _connectionManager;
        private readonly RpcClientHelper _rpcClientHelper;
        private readonly PublishHelper _publishHelper;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();

        public RabbitMqClient()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connectionManager = new ConnectionManager(factory);
            _rpcClientHelper = new RpcClientHelper(_connectionManager, _callbackMapper);
            _publishHelper = new PublishHelper(_connectionManager);
        }

        public async Task StartAsync()
        {
            await _connectionManager.StartAsync();

            var consumer = new AsyncEventingBasicConsumer(_connectionManager.Channel);
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

            await _connectionManager.Channel.BasicConsumeAsync(
                queue: _connectionManager.ReplyQueueName,
                autoAck: true,
                consumer: consumer
            );
        }

        public async Task<decimal> RequestUserAvaibleBalanceToOrders(string userId, CancellationToken cancellationToken = default)
        {
            var response = await _rpcClientHelper.CallRpcRawAsync(userId, AvailableBalanceQueue, cancellationToken);

            if (string.IsNullOrWhiteSpace(response))
                throw new InvalidOperationException("Received empty response from PortfolioService when requesting available balance.");

            return decimal.Parse(response, System.Globalization.CultureInfo.InvariantCulture);
        }


        public async Task<int> RequestUserSpecificStocksAmountForSale(string userId, string stockTicker, CancellationToken ct = default)
        {
            var reqDto = new { UserId = userId, StockTicker = stockTicker };
            var response = await _rpcClientHelper.CallRpcAsync(reqDto, StockCountQueue, ct);
            return int.Parse(response);
        }

        public async Task<bool> ReserveBalance(string userId, decimal amount, CancellationToken ct = default)
        {
            var payload = new { UserId = userId, Amount = amount };
            var response = await _rpcClientHelper.CallRpcAsync(payload, "user_reserve_balance_rpc_queue", ct);
            return bool.Parse(response);
        }

        public async Task<bool> ReleaseReservedBalance(string userId, decimal amount, CancellationToken ct = default)
        {
            var payload = new { UserId = userId, Amount = amount };
            var response = await _rpcClientHelper.CallRpcAsync(payload, "user_release_reserved_balance_rpc_queue", ct);
            return bool.Parse(response);
        }

        public async Task PublishOrderExecutedAsync(OrderRequest req, CancellationToken ct = default)
        {
            await _publishHelper.PublishMessageAsync(req, OrdersExchange, OrdersRoutingKey, ct);
        }

        public async ValueTask DisposeAsync()
        {
            await _connectionManager.DisposeAsync();
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}