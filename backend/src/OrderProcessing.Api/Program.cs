using OrderProcessing.Api.ErrorHandling;
using OrderProcessing.Api.OpenApi;
using OrderProcessing.Api.Security;
using OrderProcessing.Application;
using OrderProcessing.Application.Security;
using OrderProcessing.Infrastructure;
using OrderProcessing.Infrastructure.Security;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithBearerAuth();

    var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
        .WithOrigins(corsAllowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddKeycloakAuthentication(builder.Configuration);

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var app = builder.Build();

    app.UseExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("Frontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Order Processing API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
