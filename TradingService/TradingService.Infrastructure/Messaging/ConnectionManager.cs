using RabbitMQ.Client;

namespace TradingService.Infrastructure.Messaging
{
    internal class ConnectionManager
    {
        private readonly IConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;
        private string? _replyQueueName;

        public IChannel Channel => _channel!;
        public string ReplyQueueName => _replyQueueName!;

        public ConnectionManager(IConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task StartAsync()
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            var declareOk = await _channel.QueueDeclareAsync();
            _replyQueueName = declareOk.QueueName;
        }

        public void EnsureChannel()
        {
            if (_channel == null || string.IsNullOrEmpty(_replyQueueName))
                throw new InvalidOperationException("Call StartAsync() first.");
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
        }
    }
}
