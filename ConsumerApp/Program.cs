using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Schema;

Console.WriteLine("Consumer App");

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "demo-queue",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

/*
 * This is the heart of the consumer.
 * It registers a callback that fires only when a message arrives.
 * No loops.
 * No polling.
 * No wasted CPU.
 */
var consumer = new AsyncEventingBasicConsumer(channel);


/*
 * RabbitMQ pushes messages to your app.
 * Your handler decodes the bytes and prints the message.
 */
consumer.ReceivedAsync += OnMessageReceived;
static async Task OnMessageReceived(object sender, BasicDeliverEventArgs e)
{
    var body = e.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine("Received: {0}", message);
}

/*
 * This tells RabbitMQ to start sending messages from the "demo-queue" to our handler.
 * The autoAck: true means we tell RabbitMQ to consider messages delivered as soon as they are sent.
 * In a real app, you might want to set this to false and manually acknowledge messages after processing.
 */
await channel.BasicConsumeAsync(
    queue: "demo-queue",
    autoAck: true,
    consumer: consumer);

var sw = Stopwatch.StartNew();
while (sw.Elapsed < TimeSpan.FromMinutes(5))
{
    Console.WriteLine("Waiting for messages. type [exit] to exit.");
    var input = Console.ReadLine();
    if (input?.ToLower() == "exit")
    {
        break;
    }
}

Console.WriteLine("Exiting consumer app.");



