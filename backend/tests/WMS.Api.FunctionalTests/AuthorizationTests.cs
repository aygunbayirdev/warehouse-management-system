using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using WMS.Modules.Identity.Domain;

namespace WMS.Api.FunctionalTests;

[Collection("Functional")]
public sealed class AuthorizationTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ApproveGoodsReceipt_WithWarehouseStaffRole_ReturnsForbidden()
    {
        var token = TestJwt.CreateForRole(_factory, RoleNames.WarehouseStaff);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync($"/api/goods-receipts/{Guid.NewGuid()}/approve", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
