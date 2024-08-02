using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;

namespace Para.Bussiness.RabbitMQ
{
    public class RabbitMQService : IDisposable
    {
        private readonly IConnection _connection;
        public readonly IModel Channel;
        private readonly string _queueName;

        public RabbitMQService(IConfiguration configuration)
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:HostName"],
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"],
                Port = int.Parse(configuration["RabbitMQ:Port"])
            };

            _connection = factory.CreateConnection();
            Channel = _connection.CreateModel();

            _queueName = configuration["RabbitMQ:QueueName"];
            Channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Publish(string message)
        {
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            Channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }

        public void Dispose()
        {
            Channel?.Close();
            _connection?.Close();
        }
    }
}
