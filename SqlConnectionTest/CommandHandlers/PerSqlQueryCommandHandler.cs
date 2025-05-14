using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;
using SqlConnectionTest.Commands;

namespace SqlConnectionTest.CommandHandlers;

internal class PerSqlQueryCommandHandler(string connectionString, CancellationToken cancellationToken)
{
    public async Task Handle(PerSqlQueryCommand command)
    {
        Console.WriteLine("create a new connection per-sql-query");
        Console.WriteLine($"connection string: {command.ConnectionString}");
        Console.WriteLine($"number of threads: {command.NumberOfThreads}");
        var stopwatch = new Stopwatch();
        var tasks = new Task[command.NumberOfThreads];
        for (var i = 0; i < tasks.Length; i++)
        {
            var threadId = i;
            tasks[i] = new Task(async () =>
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
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Thread Id: {threadId} - Sql Connection Id: {connectionId} exception: {e.Message}");
                    }
                    Console.WriteLine($"Thread Id: {threadId} - Create Sql Connection id: {connectionId} and select 1 elapsed time: {stopwatch.Elapsed}");
                }
            });
            tasks[i].Start();
        }
        Task.WaitAll(tasks, cancellationToken);
    }
}