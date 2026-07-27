using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Domain;

public sealed class Product : BaseEntity
{
    private Product()
    {
    }

    private Product(Guid id, string sku, string name, Guid unitOfMeasureId, Guid? categoryId, decimal minStockQuantity)
        : base(id)
    {
        Sku = sku;
        Name = name;
        UnitOfMeasureId = unitOfMeasureId;
        CategoryId = categoryId;
        MinStockQuantity = minStockQuantity;
    }

    /// <summary>Business identifier. Immutable after creation — not exposed on <see cref="Update"/>.</summary>
    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public Guid UnitOfMeasureId { get; private set; }

    public Guid? CategoryId { get; private set; }

    public decimal MinStockQuantity { get; private set; }

    public static Product Create(string sku, string name, Guid unitOfMeasureId, Guid? categoryId, decimal minStockQuantity)
    {
        Guard.AgainstNullOrWhiteSpace(sku, nameof(sku));
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstEmpty(unitOfMeasureId, nameof(unitOfMeasureId));
        Guard.AgainstNegative(minStockQuantity, nameof(minStockQuantity));

        return new Product(
            Guid.CreateVersion7(),
            sku.Trim().ToUpperInvariant(),
            name.Trim(),
            unitOfMeasureId,
            categoryId,
            minStockQuantity);
    }

    public void Update(string name, Guid unitOfMeasureId, Guid? categoryId, decimal minStockQuantity)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstEmpty(unitOfMeasureId, nameof(unitOfMeasureId));
        Guard.AgainstNegative(minStockQuantity, nameof(minStockQuantity));

        Name = name.Trim();
        UnitOfMeasureId = unitOfMeasureId;
        CategoryId = categoryId;
        MinStockQuantity = minStockQuantity;
    }
}
