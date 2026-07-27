using System.Text;
using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Infrastructure.Repositories;

internal sealed class ProductReadRepository(ISqlConnectionFactory connectionFactory) : IProductReadRepository
{
    private const string BaseSql = """
        SELECT p.id AS "Id", p.sku AS "Sku", p.name AS "Name",
               p.unit_of_measure_id AS "UnitOfMeasureId", u.code AS "UnitOfMeasureCode",
               p.category_id AS "CategoryId", c.name AS "CategoryName",
               p.min_stock_quantity AS "MinStockQuantity"
        FROM catalog.products p
        INNER JOIN catalog.units_of_measure u ON u.id = p.unit_of_measure_id
        LEFT JOIN catalog.categories c ON c.id = p.category_id
        """;

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"{BaseSql} WHERE p.id = @Id";

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<ProductDto>(command);
    }

    public async Task<IReadOnlyCollection<ProductDto>> GetListAsync(
        Guid? categoryId,
        string? search,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(BaseSql);
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (categoryId is not null)
        {
            sql.Append(" WHERE p.category_id = @CategoryId");
            parameters.Add("CategoryId", categoryId);
            hasWhere = true;
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql.Append(hasWhere ? " AND " : " WHERE ");
            sql.Append("(p.name ILIKE @Search OR p.sku ILIKE @Search)");
            parameters.Add("Search", $"%{search.Trim()}%");
        }

        sql.Append(" ORDER BY p.name");

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<ProductDto>(command);

        return result.ToList();
    }
}
