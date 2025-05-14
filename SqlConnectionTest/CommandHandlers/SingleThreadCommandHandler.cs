using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using SqlConnectionTest.Commands;

namespace SqlConnectionTest.CommandHandlers;

internal class SingleThreadCommandHandler(string connectionString, CancellationToken cancellationToken)
{
    public async Task Handle(SingleThreadCommand command)
    {
        Console.WriteLine("Single connection share with all threads");
        Console.WriteLine($"connection string: {command.ConnectionString}");
        Console.WriteLine($"number of threads: {command.NumberOfThreads}");
        Console.WriteLine(connectionString);

        var tasks = new Task[command.NumberOfThreads];
        await using var db = new SqlConnection(connectionString);
        await db.OpenAsync(cancellationToken);

        var stopwatch = new Stopwatch();

        for (var i = 0; i < tasks.Length; i++)
        {
            var threadId = i;
            tasks[i] = new Task(async () =>
            {
                var dbConnection = db;
                while (!cancellationToken.IsCancellationRequested)
                {
                    stopwatch.Restart();
                    try
                    {
                        await dbConnection.QueryAsync("select 1", cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Thread Id: {threadId} - Sql Connection Exception: {e.Message}");
                    }
                    Console.WriteLine($"Thread Id: {{threadId}} - select 1 elapsed time: {stopwatch.Elapsed}");
                }
            });
            tasks[i].Start();
        }

        Task.WaitAll(tasks, cancellationToken);
    }
}