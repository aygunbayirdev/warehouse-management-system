using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WMS.BuildingBlocks.Infrastructure;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Domain;
using WMS.Modules.Catalog.Infrastructure;
using WMS.Modules.Catalog.Infrastructure.Persistence;

namespace WMS.Modules.Catalog.IntegrationTests;

/// <summary>
/// The simplest write/read pattern in the codebase, run against a real Postgres: create via the EF
/// write repository, read back via the Dapper read repository, and confirm the join to
/// category/unit-of-measure resolves correctly.
/// </summary>
[Collection("Postgres")]
public sealed class ProductRoundtripTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private ServiceProvider _serviceProvider = null!;

    public ProductRoundtripTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _fixture.ConnectionString,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDomainEventDispatching();
        services.AddSqlConnectionFactory(_fixture.ConnectionString);
        services.AddCatalogModule(configuration);
        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _serviceProvider.DisposeAsync();

    [Fact]
    public async Task Product_WrittenThroughEfCore_IsReadableThroughDapperWithJoinedNames()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfMeasureWriteRepository = scope.ServiceProvider.GetRequiredService<IUnitOfMeasureWriteRepository>();
        var categoryWriteRepository = scope.ServiceProvider.GetRequiredService<ICategoryWriteRepository>();
        var productWriteRepository = scope.ServiceProvider.GetRequiredService<IProductWriteRepository>();
        var productReadRepository = scope.ServiceProvider.GetRequiredService<IProductReadRepository>();

        var unitOfMeasure = UnitOfMeasure.Create("ADET", "Adet");
        unitOfMeasureWriteRepository.Add(unitOfMeasure);
        await unitOfMeasureWriteRepository.SaveChangesAsync(CancellationToken.None);

        var category = Category.Create("Elektronik");
        categoryWriteRepository.Add(category);
        await categoryWriteRepository.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create("SKU-100", "Test Ürün", unitOfMeasure.Id, category.Id, 5m);
        productWriteRepository.Add(product);
        await productWriteRepository.SaveChangesAsync(CancellationToken.None);

        var dto = await productReadRepository.GetByIdAsync(product.Id, CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Sku.Should().Be("SKU-100");
        dto.UnitOfMeasureCode.Should().Be("ADET");
        dto.CategoryName.Should().Be("Elektronik");
        dto.MinStockQuantity.Should().Be(5m);
    }
}
