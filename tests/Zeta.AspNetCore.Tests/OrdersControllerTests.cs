using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Zeta.AspNetCore.Tests;

public class OrdersControllerTests : IntegrationTestBase
{
    public OrdersControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    #region Create Order

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreated()
    {
        var request = new
        {
            customerId = "11111111-1111-1111-1111-111111111111",
            items = new[]
            {
                new { productId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", quantity = 2, notes = "Gift wrap please" },
                new { productId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", quantity = 1, notes = (string?)null }
            },
            shippingAddress = new
            {
                street = "456 Oak Avenue",
                city = "Los Angeles",
                state = "CA",
                zipCode = "90001",
                country = "USA"
            },
            billingAddress = (object?)null,
            couponCode = "SAVE10",
            paymentMethod = "credit_card"
        };

        var response = await Client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_CustomerNotFound_ReturnsBadRequest()
    {
        var request = new
        {
            customerId = "99999999-9999-9999-9999-999999999999",
            items = new[]
            {
                new { productId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", quantity = 1, notes = (string?)null }
            },
            shippingAddress = new
            {
                street = "456 Oak Avenue",
                city = "Los Angeles",
                state = "CA",
                zipCode = "90001",
                country = (string?)null
            },
            billingAddress = (object?)null,
            couponCode = (string?)null,
            paymentMethod = "credit_card"
        };

        var response = await Client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Message!.Contains("Customer not found"));
    }

    [Fact]
    public async Task CreateOrder_ProductNotFound_ReturnsBadRequest()
    {
        var request = new
        {
            customerId = "11111111-1111-1111-1111-111111111111",
            items = new[]
            {
                new { productId = "99999999-9999-9999-9999-999999999999", quantity = 1, notes = (string?)null }
            },
            shippingAddress = new
            {
                street = "456 Oak Avenue",
                city = "Los Angeles",
                state = "CA",
                zipCode = "90001",
                country = (string?)null
            },
            billingAddress = (object?)null,
            couponCode = (string?)null,
            paymentMethod = "credit_card"
        };

        var response = await Client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Message!.Contains("products not found"));
    }

    [Fact]
    public async Task CreateOrder_InvalidCoupon_ReturnsBadRequest()
    {
        var request = new
        {
            customerId = "11111111-1111-1111-1111-111111111111",
            items = new[]
            {
                new { productId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", quantity = 1, notes = (string?)null }
            },
            shippingAddress = new
            {
                street = "456 Oak Avenue",
                city = "Los Angeles",
                state = "CA",
                zipCode = "90001",
                country = (string?)null
            },
            billingAddress = (object?)null,
            couponCode = "INVALID-COUPON",
            paymentMethod = "credit_card"
        };

        var response = await Client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Message!.Contains("Invalid coupon"));
    }

    [Fact]
    public async Task CreateOrder_MultipleNestedErrors_ReturnsBadRequest()
    {
        var request = new
        {
            customerId = "99999999-9999-9999-9999-999999999999",
            items = new[]
            {
                new { productId = "99999999-9999-9999-9999-999999999999", quantity = 0, notes = (string?)null }
            },
            shippingAddress = new
            {
                street = "123",
                city = "X",
                state = "CALIFORNIA",
                zipCode = "invalid",
                country = (string?)null
            },
            billingAddress = (object?)null,
            couponCode = "FAKE",
            paymentMethod = "bitcoin"
        };

        var response = await Client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);

        // Verify multiple errors are collected
        // Context-aware validations (at object level) have empty path
        Assert.Contains(errors, e => e.Message!.Contains("Customer not found"));
        Assert.Contains(errors, e => e.Message!.Contains("products not found"));
        Assert.Contains(errors, e => e.Message!.Contains("Invalid coupon"));
        // Field-level validations keep their paths
        Assert.Contains(errors, e => e.Path == "items[0].quantity");
        Assert.Contains(errors, e => e.Path == "shippingAddress.street");
        Assert.Contains(errors, e => e.Path == "shippingAddress.city");
        Assert.Contains(errors, e => e.Path == "shippingAddress.state");
        Assert.Contains(errors, e => e.Path == "shippingAddress.zipCode");
        Assert.Contains(errors, e => e.Path == "paymentMethod");
    }

    #endregion

    #region Schedule Delivery

    [Fact]
    public async Task ScheduleDelivery_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            orderId = "11111111-0000-0000-0000-000000000001",
            deliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10)).ToString("yyyy-MM-dd"),
            preferredTimeStart = "09:00:00",
            preferredTimeEnd = "17:00:00"
        };

        var response = await Client.PostAsJsonAsync("/api/orders/delivery", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ScheduleDelivery_PastDate_ReturnsBadRequest()
    {
        var request = new
        {
            orderId = "11111111-0000-0000-0000-000000000001",
            deliveryDate = "2020-01-01",
            preferredTimeStart = (string?)null,
            preferredTimeEnd = (string?)null
        };

        var response = await Client.PostAsJsonAsync("/api/orders/delivery", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Path == "deliveryDate");
    }

    [Fact]
    public async Task ScheduleDelivery_EndTimeBeforeStartTime_ReturnsBadRequest()
    {
        var request = new
        {
            orderId = "11111111-0000-0000-0000-000000000001",
            deliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10)).ToString("yyyy-MM-dd"),
            preferredTimeStart = "17:00:00",
            preferredTimeEnd = "09:00:00"
        };

        var response = await Client.PostAsJsonAsync("/api/orders/delivery", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await GetValidationErrors(response);
        Assert.Contains(errors, e => e.Code == "invalid_time_range");
    }

    #endregion
}
