using OrderProcessing.Application;
using OrderProcessing.Infrastructure;
using OrderProcessing.Worker;
using OrderProcessing.Worker.Options;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection(WorkerOptions.SectionName));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHostedService<OrderProcessingWorker>();

    var host = builder.Build();
    host.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Order Processing Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
