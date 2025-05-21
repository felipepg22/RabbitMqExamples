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

        await channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

        var randomNumber = new Random().Next(1, 100);
        var queueName = "logs_queue";

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", "dlx_logs" }
        };

        await channel.QueueDeclareAsync(queue: queueName, 
                                        durable: true, 
                                        exclusive: false, 
                                        autoDelete: false,
                                        arguments: mainQueueArgs);

        await channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: string.Empty);

        Console.WriteLine(" [*] Waiting for logs.");

        var consumer = new AsyncEventingBasicConsumer(channel);

        

        consumer.ReceivedAsync += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (message.Equals("error", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Simulated exception for error message");
                }

                Console.WriteLine($" [x] Received {message}");
                
            }
            catch (Exception ex)
            {

                channel.BasicRejectAsync(ea.DeliveryTag, true);

            }
            
            return Task.CompletedTask;
            
        };

        await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}