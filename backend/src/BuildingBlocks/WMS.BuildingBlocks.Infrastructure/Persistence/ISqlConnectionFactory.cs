using System.Data;

namespace WMS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>Shared by every module's Dapper read repositories to open ADO.NET connections.</summary>
public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
