using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(exchange: "topic_logs", type: ExchangeType.Topic);       
        

        var randomNumber = new Random().Next(1, 100);
        var queueName = "queue" + randomNumber;

        await channel.QueueDeclareAsync(queue: queueName, false, false, false, null);

        foreach (var bindingKey in args)
        {
            await channel.QueueBindAsync(queue: queueName, exchange: "topic_logs", routingKey: bindingKey);
        }        

        Console.WriteLine($"Waiting for messages on {queueName}...");
       
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received from {ea.RoutingKey}: {message}");
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);        
        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }
}