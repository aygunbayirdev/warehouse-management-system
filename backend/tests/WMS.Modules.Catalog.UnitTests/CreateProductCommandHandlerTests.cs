using FluentAssertions;
using NSubstitute;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Products;
using WMS.Modules.Catalog.Domain;

namespace WMS.Modules.Catalog.UnitTests;

public class CreateProductCommandHandlerTests
{
    private readonly IProductWriteRepository _productWriteRepository = Substitute.For<IProductWriteRepository>();
    private readonly IUnitOfMeasureWriteRepository _unitOfMeasureWriteRepository = Substitute.For<IUnitOfMeasureWriteRepository>();
    private readonly ICategoryWriteRepository _categoryWriteRepository = Substitute.For<ICategoryWriteRepository>();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_productWriteRepository, _unitOfMeasureWriteRepository, _categoryWriteRepository);
    }

    [Fact]
    public async Task Handle_WithValidData_AddsProductAndReturnsItsId()
    {
        var unitOfMeasure = UnitOfMeasure.Create("ADET", "Adet");

        _productWriteRepository.GetBySkuAsync("SKU-100", Arg.Any<CancellationToken>()).Returns((Product?)null);
        _unitOfMeasureWriteRepository.GetByIdAsync(unitOfMeasure.Id, Arg.Any<CancellationToken>()).Returns(unitOfMeasure);

        var command = new CreateProductCommand("SKU-100", "Test Ürün", unitOfMeasure.Id, null, 0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _productWriteRepository.Received(1).Add(Arg.Is<Product>(product => product.Sku == "SKU-100"));
        await _productWriteRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateSku_ReturnsConflict()
    {
        var existingProduct = Product.Create("SKU-100", "Existing", Guid.NewGuid(), null, 0m);
        _productWriteRepository.GetBySkuAsync("SKU-100", Arg.Any<CancellationToken>()).Returns(existingProduct);

        var command = new CreateProductCommand("SKU-100", "Yeni Ürün", Guid.NewGuid(), null, 0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.SkuAlreadyExists");
        _productWriteRepository.DidNotReceive().Add(Arg.Any<Product>());
    }

    [Fact]
    public async Task Handle_WithUnknownUnitOfMeasure_ReturnsNotFound()
    {
        _productWriteRepository.GetBySkuAsync("SKU-100", Arg.Any<CancellationToken>()).Returns((Product?)null);
        _unitOfMeasureWriteRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((UnitOfMeasure?)null);

        var command = new CreateProductCommand("SKU-100", "Test Ürün", Guid.NewGuid(), null, 0m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UnitOfMeasure.NotFound");
    }
}
