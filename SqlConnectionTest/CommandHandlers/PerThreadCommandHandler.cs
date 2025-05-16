using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using SqlConnectionTest.Commands;

namespace SqlConnectionTest.CommandHandlers;

internal class PerThreadCommandHandler(string connectionString, CancellationToken cancellationToken)
{
    public Task Handle(PerThreadCommand command)
    {
        Console.WriteLine("use a new connection per-threads");
        Console.WriteLine($"connection string: {command.ConnectionString}");
        Console.WriteLine($"number of threads: {command.NumberOfThreads}");

        var stopwatch = new Stopwatch();
        
        var tasks = new Task[command.NumberOfThreads];
        for (var i = 0; i < tasks.Length; i++)
        {
            var threadId = i + 1;

            tasks[i] = new Task(Action);
            continue;

            async void Action()
            {
                await using var db = new SqlConnection(connectionString);
                await db.OpenAsync(cancellationToken);
                while (!cancellationToken.IsCancellationRequested)
                {
                    stopwatch.Restart();
                    try
                    {
                        await db.QueryAsync("select 1", cancellationToken);
                        if (DateTime.Now.Ticks % 10000 == 0)
                            Console.WriteLine($"Thread Id: {threadId:0000} - Sql Connection Id: {threadId:0000} - select 1 elapsed time: {stopwatch.Elapsed}");
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Thread Id: {threadId:0000} - Sql Connection Id: {threadId:0000} exception: {e.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }
        foreach (var task in tasks) task.Start();
        
        return Task.CompletedTask;
    }
}