using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

class Program
{
    static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var exchangeName = "dlx_logs";

        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Fanout);

        var queueName = "dlx_logs_queue";
        var mainQueueName = "logs_queue";

        await channel.QueueDeclareAsync(queue: queueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        null);

        await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty);        

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", exchangeName }
        };

        // Declare the main queue with dead-letter settings
        await channel.QueueDeclareAsync(mainQueueName, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"[DLQ] Received: {message}");

        
            channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);

            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}