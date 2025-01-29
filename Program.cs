using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: "config_exchange", type: ExchangeType.Topic, durable: true);

        
        string projectId = "1";  
        string queueName = $"config_updates_queue_{projectId}";

        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);


        string routingKey = $"config.{projectId}.#";
        channel.QueueBind(queue: queueName, exchange: "config_exchange", routingKey: routingKey);

        Console.WriteLine($"Waiting for messages for Project ID {projectId}...");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Received message for Project {projectId}: {message}");
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        Console.ReadLine();
    }
}
