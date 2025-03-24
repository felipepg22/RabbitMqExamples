using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Program
{
    static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        string exchangeName = "direct_logs";
        var queueResult = await channel.QueueDeclareAsync();

        Console.WriteLine("Type routing key: ");
        var routingKey = Console.ReadLine();       

        // Declare Direct Exchange
        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct);

        // Bind Queue to Exchange with Routing Key
        await channel.QueueBindAsync(queue: queueResult.QueueName,
                          exchange: exchangeName,
                          routingKey: routingKey ?? "info");

       
        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            Console.WriteLine($" [x] Received '{routingKey}':'{message}'");
            return Task.CompletedTask;
        };       

        await channel.BasicConsumeAsync(queueResult.QueueName, autoAck: true, consumer: consumer);
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();


    }
}
