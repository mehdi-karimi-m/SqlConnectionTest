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
        
        var tasks = new Task[command.NumberOfThreads];
        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = new Task(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine(connectionString);
                    await using var db = new SqlConnection(connectionString);
                    await db.QueryAsync("select 1", cancellationToken);
                }
            });
            tasks[i].Start();
        }
        Task.WaitAll(tasks, cancellationToken);
    }
}