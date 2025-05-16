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

        for (var i = 1; i < tasks.Length + 1; i++)
        {
            var threadId = i;

            tasks[i] = new Task(Action);
            tasks[i].Start();
            continue;

            async void Action()
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    stopwatch.Restart();
                    try
                    {
                        await db.QueryAsync("select 1", cancellationToken);
                        Console.WriteLine($"Thread Id: {threadId:0000} - select 1 elapsed time: {stopwatch.Elapsed}");
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Thread Id: {threadId:0000} - Sql Connection Exception: {e.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}