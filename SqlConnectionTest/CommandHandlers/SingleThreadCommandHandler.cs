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
        
        var tasks = new Task[command.NumberOfThreads];
        await using var db = new SqlConnection(connectionString);
        await db.OpenAsync(cancellationToken);
        
        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = new Task(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine(connectionString);
                    await db.QueryAsync("select 1", cancellationToken);
                }
            });
            tasks[i].Start();
        }
        Task.WaitAll(tasks, cancellationToken);
    }
}