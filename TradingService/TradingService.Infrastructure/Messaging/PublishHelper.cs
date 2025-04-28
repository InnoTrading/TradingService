using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace TradingService.Infrastructure.Messaging
{
    internal class PublishHelper
    {
        private readonly ConnectionManager _connectionManager;

        public PublishHelper(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task PublishMessageAsync(object payload, string exchange, string routingKey, CancellationToken ct = default)
        {
            _connectionManager.EnsureChannel();

            var json = JsonSerializer.Serialize(payload);
            var data = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            await _connectionManager.Channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: data,
                cancellationToken: ct);
        }
    }
}
