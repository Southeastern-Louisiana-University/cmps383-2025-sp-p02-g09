using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Selu383.SP25.P02.Test.Controllers.Authentication;
using Selu383.SP25.P02.Test.Dtos;
using Selu383.SP25.P02.Test.Helpers;

namespace Selu383.SP25.P02.Test.Controllers.Theaters;

[TestClass]
public class TheatersControllerTests
{
    private WebTestContext context = new();

    [TestInitialize]
    public void Init()
    {
        context = new WebTestContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task ListAllTheaters_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/theaters");

        //assert
        await httpResponse.AssertTheaterListAllFunctions();
    }

    [TestMethod]
    public async Task GetTheaterById_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetTheater();
        if (target == null)
        {
            Assert.Fail("Make List All theaters work first");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync($"/api/theaters/{target.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/theaters/{id} ");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<TheaterDto>();
        resultDto.Should().BeEquivalentTo(target, "we expect get theater by id to return the same data as the list all theaters endpoint");
    }

    [TestMethod]
    public async Task GetTheaterById_NoSuchId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/theaters/999999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/theaters/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateTheater_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new TheaterDto
        {
            Address = "asd",
            ManagerId = context.GetBobUserId(),
            SeatCount = 100
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/theaters with no name");
    }

    [TestMethod]
    public async Task CreateTheater_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new TheaterDto
        {
            Name = "a".PadLeft(121, '0'),
            Address = "asd",
            ManagerId = context.GetBobUserId(),
            SeatCount = 100
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/theaters with a name that is too long");
    }

    [TestMethod]
    public async Task CreateTheater_NoAddress_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetTheater();
        await webClient.AssertLoggedInAsAdmin();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        var request = new TheaterDto
        {
            Name = "asd",
            ManagerId = context.GetBobUserId(),
            SeatCount = 100
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/theaters with no description");
    }

    [TestMethod]
    public async Task CreateTheater_Returns201AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "asd",
            SeatCount = 100
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        await httpResponse.AssertCreateTheaterFunctions(request, webClient);
    }

    [TestMethod]
    public async Task CreateTheater_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "asd",
            SeatCount = 100
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling POST /api/theaters when not logged in");
    }

    [TestMethod]
    public async Task CreateTheater_LoggedInAsBob_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsBob();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "asd",
            SeatCount = 100
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/theaters", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling POST /api/theaters when logged in as bob");
    }

    [TestMethod]
    public async Task UpdateTheater_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
            SeatCount = 100
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Name = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/theaters/{id} with a missing name");
    }

    [TestMethod]
    public async Task UpdateTheater_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
            SeatCount = 100
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Name = "a".PadLeft(121, '0');

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/theaters/{id} with a name that is too long");
    }

    [TestMethod]
    public async Task UpdateTheater_NoAddress_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
            SeatCount = 100
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Address = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/theaters/{id} with a missing description");
    }

    [TestMethod]
    public async Task UpdateTheater_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var sueId = context.GetSueUserId();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
            SeatCount = 100
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Address = "cool new address";
        request.ManagerId = sueId;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        await httpResponse.AssertTheaterUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateTheater_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
            SeatCount = 100
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling PUT /api/theaters/{id} without being logged in");
    }

    [TestMethod]
    public async Task UpdateTheater_LoggedInAsBob_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
            SeatCount = 100
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsBob();

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        await httpResponse.AssertTheaterUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateTheater_LoggedInAsWrongUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
            SeatCount = 100
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/theaters/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling PUT /api/theaters/{id} against a theater bob manages while logged in as sue");
    }

    [TestMethod]
    public async Task DeleteTheater_NoSuchItem_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Address = "asd",
            Name = "asd",
            SeatCount = 100
        };
        await using var itemHandle = await webClient.CreateTheater(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/theaters/{request.Id + 21}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/theaters/{id} with an invalid Id");
    }

    [TestMethod]
    public async Task DeleteTheater_ValidItem_ReturnsOk()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Address = "asd",
            Name = "asd",
            SeatCount = 100
        };
        await using var itemHandle = await webClient.CreateTheater(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/theaters/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/theaters/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteTheater_SameItemTwice_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new TheaterDto
        {
            Address = "asd",
            Name = "asd",
            SeatCount = 100
        };
        await using var itemHandle = await webClient.CreateTheater(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        await webClient.DeleteAsync($"/api/theaters/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/theaters/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/theaters/{id} on the same item twice");
    }

    [TestMethod]
    public async Task DeleteTheater_LoggedInAsWrongUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new TheaterDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
            SeatCount = 100
        };
        await using var target = await webClient.CreateTheater(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        //act
        await webClient.DeleteAsync($"/api/theaters/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/theaters/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling DELETE /api/theaters/{id} against a theater bob manages while logged in as sue");
    }
}
