using Microsoft.AspNetCore.Mvc;
using MQTTnet;

var builder = WebApplication.CreateBuilder(args);

//add MqttClientService
/* old
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService<MqttClientService>();
*/
//new
builder.Services.AddSingleton<MqttClientService>();
builder.Services.AddSingleton<IMqttClientService>(sp =>
    sp.GetRequiredService<MqttClientService>());
// Important: make the hosted service use the SAME singleton instance`
builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<MqttClientService>());


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "Op20 MQTT API";
    config.Title = "Op20 MQTT API v1";
    config.Version = "v0";
});

var app = builder.Build();

// Retrieve the singleton service instance directly and call a method
var singletonService = app.Services.GetRequiredService<IMqttClientService>();
singletonService.SetServerAddress("172.16.1.32");

if(app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Op20 MQTT API";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/", () => "Hello World!");

app.MapPost("/publish_data", async (IMqttClientService client) =>
{  
    await client.PublishOnce("Test", "Hello World!");
    Console.WriteLine("Hello World!"); 
});

app.MapGet("/get_op20", async (IMqttClientService client) =>
{
    var op20 = client.GetOp20();
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(op20).ToString());
});

app.Run();