using FluentAssertions;
using NSubstitute;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Categories;
using WMS.Modules.Catalog.Domain;

namespace WMS.Modules.Catalog.UnitTests;

public class DeleteCategoryCommandHandlerTests
{
    private readonly ICategoryWriteRepository _categoryWriteRepository = Substitute.For<ICategoryWriteRepository>();
    private readonly IProductWriteRepository _productWriteRepository = Substitute.For<IProductWriteRepository>();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(_categoryWriteRepository, _productWriteRepository);
    }

    [Fact]
    public async Task Handle_WithUnusedCategory_RemovesItAndSaves()
    {
        var category = Category.Create("Elektronik");
        _categoryWriteRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _productWriteRepository.ExistsWithCategoryIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _categoryWriteRepository.Received(1).Remove(category);
        await _categoryWriteRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCategoryUsedByAProduct_ReturnsConflictAndDoesNotRemove()
    {
        var category = Category.Create("Elektronik");
        _categoryWriteRepository.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _productWriteRepository.ExistsWithCategoryIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.InUse");
        _categoryWriteRepository.DidNotReceive().Remove(Arg.Any<Category>());
    }

    [Fact]
    public async Task Handle_WithUnknownCategory_ReturnsNotFound()
    {
        var missingId = Guid.NewGuid();
        _categoryWriteRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Category?)null);

        var result = await _handler.Handle(new DeleteCategoryCommand(missingId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
    }
}
