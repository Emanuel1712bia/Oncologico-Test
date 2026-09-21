using DbUp;

var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=orderprocessing;Username=orderprocessing;Password=orderprocessing";

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly, script => script.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
    Environment.Exit(1);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Database migration completed successfully.");
Console.ResetColor();
