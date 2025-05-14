using CommandLine;

namespace SqlConnectionTest.Commands;

[Verb("per-thread", HelpText = "creates per thread a SQL connection.")]
internal class PerThreadCommand : Command
{
}