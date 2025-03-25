using RabbitMQ.Client;
using System.Text;

class Program
{
    static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();

        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

        while (true)
        {
            Console.WriteLine("Write a log message: ");

            var logMessage = Console.ReadLine();

            if (string.IsNullOrEmpty(logMessage) || logMessage.Equals("quit"))
            {
                break;
            }

            var body = Encoding.UTF8.GetBytes(logMessage);

            await channel.BasicPublishAsync(exchange: "logs",
                                 routingKey: string.Empty,
                                 body: body);

            Console.WriteLine($" [x] Sent {logMessage}");
        }
    }
}

