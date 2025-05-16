// See https://aka.ms/new-console-template for more information

using System.Net.Sockets;
using CommandLine;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SqlConnectionTest.CommandHandlers;
using SqlConnectionTest.Commands;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var telnetConfiguration = configuration.GetSection(TelnetConfiguration.SectionKey).Get<TelnetConfiguration>();
MakeSureTelnetConfigurationIsValid(telnetConfiguration);

var connectionString = configuration.GetConnectionString("DefaultConnection");
MakeSureConnectionStringIsValid(connectionString);

var cancellationTokenSource = new CancellationTokenSource();

var telnetTask = new Task(TcpConnectionTest);
telnetTask.Start();

var result = Parser.Default.ParseArguments<SingleThreadCommand, PerThreadCommand, PerSqlQueryCommand>(args);
var singleThreadCommandHandler = new SingleThreadCommandHandler(connectionString, cancellationTokenSource.Token);
var perThreadCommandHandler = new PerThreadCommandHandler(connectionString, cancellationTokenSource.Token);
var perSqlQueryCommandHandler = new PerSqlQueryCommandHandler(connectionString, cancellationTokenSource.Token);

result.WithParsed<SingleThreadCommand>(command => _ = singleThreadCommandHandler.Handle(command))
    .WithParsed<PerThreadCommand>(command => _ = perThreadCommandHandler.Handle(command))
    .WithParsed<PerSqlQueryCommand>(command => _ = perSqlQueryCommandHandler.Handle(command))
    .WithNotParsed(errors =>
    {
        foreach (var error in errors)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine(error.ToString());
        }
    });

Console.WriteLine("Press any key to exit...");
Console.ReadLine();
return;

async void TcpConnectionTest()
{
    var hostname = telnetConfiguration.Hostname;
    var port = telnetConfiguration.Port;
    while (true)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(hostname, port);
            await Task.Delay(1000, cancellationTokenSource.Token);
            Console.WriteLine($"TCP client connected to {hostname}:{port}");
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("*******************************************");
            Console.WriteLine($"TCP client could not be connected to {hostname}:{port}, error: {e.Message}");
            Console.ResetColor();
        }
    }
}

void MakeSureConnectionStringIsValid(string? sqlConnectionString)
{
    using var db = new SqlConnection(sqlConnectionString);
    db.Open();
    db.Query("select 1");
}

void MakeSureTelnetConfigurationIsValid(TelnetConfiguration? telnetConfig)
{
    if(telnetConfig == null) throw new NullReferenceException("Telnet configuration is null");
    using var client = new TcpClient();
    client.Connect(telnetConfig.Hostname, telnetConfig.Port);
}