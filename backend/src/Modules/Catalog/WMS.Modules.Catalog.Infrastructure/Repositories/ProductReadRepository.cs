using System.Text;
using Dapper;
using WMS.BuildingBlocks.Application.Models;
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

    public async Task<PagedResult<ProductDto>> GetListAsync(
        Guid? categoryId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var whereClause = new StringBuilder();
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (categoryId is not null)
        {
            whereClause.Append(" WHERE p.category_id = @CategoryId");
            parameters.Add("CategoryId", categoryId);
            hasWhere = true;
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClause.Append(hasWhere ? " AND " : " WHERE ");
            whereClause.Append("(p.name ILIKE @Search OR p.sku ILIKE @Search)");
            parameters.Add("Search", $"%{search.Trim()}%");
        }

        parameters.Add("PageSize", pageSize);
        parameters.Add("Skip", (page - 1) * pageSize);

        var sql = $"""
            SELECT COUNT(*) FROM catalog.products p{whereClause};
            {BaseSql}{whereClause} ORDER BY p.name LIMIT @PageSize OFFSET @Skip;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);
        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<ProductDto>()).ToList();

        return new PagedResult<ProductDto>(items, totalCount, page, pageSize);
    }
}
