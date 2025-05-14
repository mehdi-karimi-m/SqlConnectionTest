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
        
        var tasks = new Task[command.NumberOfThreads];
        for (var i = 0; i < tasks.Length; i++)
        {
            await using var db = new SqlConnection(connectionString);
            await db.OpenAsync(cancellationToken);
            
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