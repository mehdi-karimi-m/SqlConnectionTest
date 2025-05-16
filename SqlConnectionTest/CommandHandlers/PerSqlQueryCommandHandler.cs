using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using SqlConnectionTest.Commands;

namespace SqlConnectionTest.CommandHandlers;

internal class PerSqlQueryCommandHandler(string connectionString, CancellationToken cancellationToken)
{
    public Task Handle(PerSqlQueryCommand command)
    {
        Console.WriteLine("create a new connection per-sql-query");
        Console.WriteLine($"connection string: {command.ConnectionString}");
        Console.WriteLine($"number of threads: {command.NumberOfThreads}");
        
        var stopwatch = new Stopwatch();
        var tasks = new Task[command.NumberOfThreads];
        
        for (var i = 0; i < tasks.Length; i++)
        {
            var threadId = i;

            tasks[i] = new Task(Action);
            continue;

            async void Action()
            {
                var connectionId = 0L;
                while (!cancellationToken.IsCancellationRequested)
                {
                    connectionId++;
                    stopwatch.Restart();
                    try
                    {
                        await using var db = new SqlConnection(connectionString);
                        await db.QueryAsync("select 1", cancellationToken);
                        if (connectionId % 10000 == 0)
                            Console.WriteLine($"Thread Id: {threadId:0000} - Create Sql Connection id: {connectionId:000000000} and select 1 elapsed time: {stopwatch.Elapsed}");
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Thread Id: {threadId:0000} - Sql Connection Id: {connectionId:000000000} exception: {e.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }
        foreach (var task in tasks) task.Start();
        
        return Task.CompletedTask;
    }
}