using RabbitMQ.Client;
using System.Text;

class Program
{
    static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: "topic_logs", type: ExchangeType.Topic);        

        while(true)
        {
            Console.WriteLine("Type routing key: ");
            var routingKey = Console.ReadLine();

            if (string.IsNullOrEmpty(routingKey))
            {
                Console.WriteLine("Invalid routing key!");
                return;
            }

            Console.WriteLine("Type message: ");
            var message = Console.ReadLine();

            if (string.IsNullOrEmpty(message))
            {
                Console.WriteLine("Invalid message!");
                continue;
            }

            if (message.Equals("quit"))
                break;

            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange: "topic_logs", routingKey: routingKey, body: body);
            Console.WriteLine($"Sent to {routingKey}: {message}");
        }
    }
}