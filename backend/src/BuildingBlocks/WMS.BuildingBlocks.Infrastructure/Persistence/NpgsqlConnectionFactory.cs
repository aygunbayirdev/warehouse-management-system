using System.Data;
using Npgsql;

namespace WMS.BuildingBlocks.Infrastructure.Persistence;

public sealed class NpgsqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
