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
        channel.QueueDeclare(queue: "config_updates_queue", durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue: "config_updates_queue", exchange: "config_exchange", routingKey: "config.#");

        Console.WriteLine("Waiting for messages...");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Received message: {message}");
            
        };

        channel.BasicConsume(queue: "config_updates_queue", autoAck: true, consumer: consumer);

        Console.ReadLine();
    }
}
