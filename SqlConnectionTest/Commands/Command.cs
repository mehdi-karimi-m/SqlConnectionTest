using CommandLine;

namespace SqlConnectionTest.Commands;

internal abstract class Command
{
    [Option('c', "connection", Required = false, Default = "",
        HelpText = "Connection String")]
    public string ConnectionString { get; set; }

    [Option('t', "number of threads", Required = false, Default = 5, HelpText = "Number of threads")]
    public ushort NumberOfThreads { get; set; }
}