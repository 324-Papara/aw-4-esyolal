using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Para.Bussiness.Notification;

namespace Para.Bussiness.Hangfire
{
    public class EmailJob
    {
        private readonly INotificationService _notificationService;
        private readonly string _queueName = "emailQueue";

        public EmailJob(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void ProcessEmails()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost", 
                UserName = "guest",    
                Password = "guest"     
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var parts = message.Split('|');
                    var subject = parts[0];
                    var email = parts[1];
                    var content = parts[2];

                    _notificationService.SendEmail(subject, email, content);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

               
            }
        }
    }
}
