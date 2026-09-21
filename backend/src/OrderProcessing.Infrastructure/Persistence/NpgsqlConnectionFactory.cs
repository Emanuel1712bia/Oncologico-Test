using Microsoft.Extensions.Options;
using Npgsql;
using OrderProcessing.Infrastructure.Options;

namespace OrderProcessing.Infrastructure.Persistence;

internal interface INpgsqlConnectionFactory
{
    Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}

internal sealed class NpgsqlConnectionFactory : INpgsqlConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
