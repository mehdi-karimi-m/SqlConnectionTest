using CommandLine;

namespace SqlConnectionTest.Commands;

[Verb("single", HelpText = "creates a single SQL connection and share it with all threads.")]
internal class SingleThreadCommand : Command
{
}