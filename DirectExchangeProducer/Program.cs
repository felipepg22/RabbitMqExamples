using System;
using System.Text;
using DirectExchangeProducer;
using RabbitMQ.Client;

class Program
{
    static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();


        var exchangeName = "direct_logs";

        // Declare Direct Exchange
        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct);

        while (true)
        {
            Console.WriteLine("Write a log message: ");
            var logMessage = Console.ReadLine();

            if (string.IsNullOrEmpty(logMessage) || logMessage.Equals("quit"))
                break;

            Console.WriteLine("Chose the severity of the log message: ");
            Console.WriteLine("1 - Information");
            Console.WriteLine("2 - Warning");
            Console.WriteLine("3 - Error");

            var severity = Console.ReadLine();

            var routingKey = GetRoutingKey(Convert.ToInt32(severity));

            if (string.IsNullOrEmpty(routingKey))
            {
                Console.WriteLine("Invalid log severity!");
                return;
            }
            
            var body = Encoding.UTF8.GetBytes(logMessage);

            // Publish message with routing key
            await channel.BasicPublishAsync(exchange: exchangeName,
                                 routingKey: routingKey,
                                 body: body);

            Console.WriteLine($"[x] Sent '{routingKey}': {logMessage}");
        }

        
    }

    private static string GetRoutingKey(int severity)
    {
        switch (severity)
        {
            case (int)Severity.Information:
                return "info";
            case (int)Severity.Warning:
                return "warning";
            case (int)Severity.Error: 
                return "error";
            default:
                return string.Empty;
        }
    }
}
