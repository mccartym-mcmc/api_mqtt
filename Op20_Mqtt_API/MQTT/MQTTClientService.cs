using MQTTnet;
using MQTTnet.Packets;
using System.Text;
using System.Text.Json;

public interface IMqttClientService
{
    Task PublishOnce(string topic, object value);
    Op20 GetOp20();

    void SetServerAddress(string address);
}
public class MqttClientService : BackgroundService, IMqttClientService
{
    private IMqttClient client;
    private Op20 op20;
    private string ServerAddress {get;set;} = "127.0.0.1";
    //private MqttClientOptionsBuilder mqttClientOptions;

    
    public async Task Connect()
    {
        op20 = new Op20();

        var mqttFactory = new MqttClientFactory();
        client = mqttFactory.CreateMqttClient();
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(ServerAddress, 1883)
            .WithClientId("ClientserviceTest")
            .Build();
    
        Console.WriteLine("Server addr: " + ServerAddress);
        //set up subscription event handler
        client.ApplicationMessageReceivedAsync += HandleMessageAsync;

        await client.ConnectAsync(options);
        Console.WriteLine("Connected");

        //get subscribes
        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter("Op20")
            .Build();
        await client.SubscribeAsync(mqttSubscribeOptions);

        Console.WriteLine("Subscritpions registered");

    }

    private Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

        Console.WriteLine($"Topic: {topic}");
        Console.WriteLine($"Payload: {payload}");

        if(topic.Equals("Op20"))
        {
            op20.lastMessage = "I got it and something happened";
        }

        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //throw new NotImplementedException();
        await Connect();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (client != null && client.IsConnected)
            await client.DisconnectAsync();

        await base.StopAsync(cancellationToken);
    }

    async Task IMqttClientService.PublishOnce(string topic, object value)
    {
        if (client is null || !client.IsConnected)
            throw new InvalidOperationException("MQTT client is not connected yet.");

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(System.Text.Json.JsonSerializer.Serialize(value))
            .Build();
        await client.PublishAsync(message);
        Console.WriteLine($"Published to topic {topic} Payload {message.Payload.ToString()}");
    }

    public Op20 GetOp20()
    {
        if (client is null || !client.IsConnected)
            throw new InvalidOperationException("MQTT client is not connected yet.");
        
        return op20;
    }

    public void SetServerAddress(string address)
    {
        ServerAddress = address;
    }
}
