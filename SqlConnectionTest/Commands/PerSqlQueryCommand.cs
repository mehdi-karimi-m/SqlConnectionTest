using CommandLine;

namespace SqlConnectionTest.Commands;

[Verb("per-sql-query", isDefault: true, HelpText = "creates per request a SQL connection.")]
internal class PerSqlQueryCommand : Command
{
}