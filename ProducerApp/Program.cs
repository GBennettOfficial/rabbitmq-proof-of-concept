using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

Console.WriteLine("Producer App");

/*
 * You’re defining how your app connects to RabbitMQ.
 * This is equivalent to a DB connection string but for AMQP.
 */
var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

/*
 * RabbitMQ requires queues to exist before publishing.
 * Declaring them on both producer and consumer is normal and safe.
*/
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "demo-queue",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

var sw = Stopwatch.StartNew();
while (sw.Elapsed < TimeSpan.FromHours(1))
{
    Console.WriteLine("Enter a message to send to the queue (or 'exit' to quit):");
    string message = Console.ReadLine() ?? "";

    if (message.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var body = Encoding.UTF8.GetBytes(message);

    try
    {
        /*
         * This sends a message to the default exchange ("") using the queue name as the routing key.
         */
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "demo-queue",
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body);

        Console.WriteLine("Sent: {0}", message);
    } 
    catch (Exception ex)
    {
        Console.WriteLine($"Error publishing message: {ex.Message}");
    }

    
}
Console.WriteLine("Exiting producer app.");



