using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using SqlConnectionTest.Commands;

namespace SqlConnectionTest.CommandHandlers;

internal class PerThreadCommandHandler(string connectionString, CancellationToken cancellationToken)
{
    public async Task Handle(PerThreadCommand command)
    {
        Console.WriteLine("use a new connection per-threads");
        Console.WriteLine($"connection string: {command.ConnectionString}");
        Console.WriteLine($"number of threads: {command.NumberOfThreads}");

        var stopwatch = new Stopwatch();
        
        var tasks = new Task[command.NumberOfThreads];
        for (var i = 0; i < tasks.Length; i++)
        {
            var threadId = i;
            await using var db = new SqlConnection(connectionString);
            await db.OpenAsync(cancellationToken);

            var connectionId = i;
            tasks[i] = new Task(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    stopwatch.Restart();
                    try
                    {
                        await db.QueryAsync("select 1", cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Thread Id: {threadId} - Sql Connection Id: {connectionId} exception: {e.Message}");
                    }
                    Console.WriteLine($"Thread Id: {threadId} - Sql Connection Id: {connectionId} - select 1 elapsed time: {stopwatch.Elapsed}");
                }
            });
            tasks[i].Start();
        }

        Task.WaitAll(tasks, cancellationToken);
    }
}