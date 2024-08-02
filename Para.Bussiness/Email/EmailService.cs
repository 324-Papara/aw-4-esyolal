using System;
using System.Text;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Para.Bussiness.Email;
public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public EmailService(IConfiguration configuration, IBackgroundJobClient backgroundJobClient)
    {
        _configuration = configuration;
        _backgroundJobClient = backgroundJobClient;
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Hostname"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"],
            Port = int.Parse(_configuration["RabbitMQ:Port"])
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        var queueName = _configuration["RabbitMQ:QueueName"];
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);

            _backgroundJobClient.Enqueue(() => SendEmail(emailMessage));
        };

        channel.BasicConsume(queue: _configuration["RabbitMQ:QueueName"], autoAck: true, consumer: consumer);
    }

    public void SendEmail(EmailMessage emailMessage)
    {
        Console.WriteLine($"Sending email to: {emailMessage.To}");
    }
}
