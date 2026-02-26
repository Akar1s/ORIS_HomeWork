using System.Net;
using TourSearch.Server;
using Xunit;
using Moq;

namespace TourSearch.Tests;

public class SimpleRouterTests
{
    [Fact]
    public void RegisterHandler_AddsHandler()
    {
                var router = new SimpleRouter();
        var mockHandler = new Mock<IRouteHandler>();
        mockHandler.Setup(h => h.CanHandle(It.IsAny<HttpListenerRequest>())).Returns(true);

                router.RegisterHandler(mockHandler.Object);

                Assert.NotNull(router);
    }

    [Fact]
    public void Match_ReturnsFirstMatchingHandler()
    {
                var router = new SimpleRouter();
        
        var handler1 = new Mock<IRouteHandler>();
        handler1.Setup(h => h.CanHandle(It.IsAny<HttpListenerRequest>())).Returns(false);
        
        var handler2 = new Mock<IRouteHandler>();
        handler2.Setup(h => h.CanHandle(It.IsAny<HttpListenerRequest>())).Returns(true);
        
        var handler3 = new Mock<IRouteHandler>();
        handler3.Setup(h => h.CanHandle(It.IsAny<HttpListenerRequest>())).Returns(true);

        router.RegisterHandler(handler1.Object);
        router.RegisterHandler(handler2.Object);
        router.RegisterHandler(handler3.Object);

                var result = router.Match(null!);

                Assert.Equal(handler2.Object, result);
    }

    [Fact]
    public void Match_ReturnsNull_WhenNoMatch()
    {
                var router = new SimpleRouter();
        
        var handler = new Mock<IRouteHandler>();
        handler.Setup(h => h.CanHandle(It.IsAny<HttpListenerRequest>())).Returns(false);

        router.RegisterHandler(handler.Object);

                var result = router.Match(null!);

                Assert.Null(result);
    }

    [Fact]
    public void Match_EmptyRouter_ReturnsNull()
    {
                var router = new SimpleRouter();

                var result = router.Match(null!);

                Assert.Null(result);
    }
}

public class HealthHandlerTests
{
    [Fact]
    public void CanHandle_HealthEndpoint_ReturnsTrue()
    {
                var handler = new HealthHandler();
        var mockRequest = CreateMockRequest("/health", "GET");

                var result = handler.CanHandle(mockRequest);

                Assert.True(result);
    }

    [Fact]
    public void CanHandle_OtherEndpoint_ReturnsFalse()
    {
                var handler = new HealthHandler();
        var mockRequest = CreateMockRequest("/api/tours", "GET");

                var result = handler.CanHandle(mockRequest);

                Assert.False(result);
    }

    private static HttpListenerRequest CreateMockRequest(string path, string method)
    {
                                var mock = new Mock<HttpListenerRequest>();
        mock.Setup(r => r.Url).Returns(new Uri($"http://localhost{path}"));
        mock.Setup(r => r.HttpMethod).Returns(method);
        return mock.Object;
    }
}

public class FormHelperTests
{
    [Fact]
    public void ParseForm_SimpleKeyValue_ParsesCorrectly()
    {
                var body = "email=test%40example.com&password=secret123";

                var result = TourSearch.Infrastructure.FormHelper.ParseForm(body);

                Assert.Equal("test@example.com", result["email"]);
        Assert.Equal("secret123", result["password"]);
    }

    [Fact]
    public void ParseForm_EmptyBody_ReturnsEmptyDict()
    {
                var body = "";

                var result = TourSearch.Infrastructure.FormHelper.ParseForm(body);

                Assert.Empty(result);
    }

    [Fact]
    public void ParseForm_EncodedValues_DecodesCorrectly()
    {
                var body = "name=John%20Doe&message=Hello%21";

                var result = TourSearch.Infrastructure.FormHelper.ParseForm(body);

                Assert.Equal("John Doe", result["name"]);
        Assert.Equal("Hello!", result["message"]);
    }

    [Fact]
    public void ParseForm_SpecialCharacters_HandlesCorrectly()
    {
                var body = "query=%D0%9F%D0%B5%D1%80%D1%83"; 
                var result = TourSearch.Infrastructure.FormHelper.ParseForm(body);

                Assert.Equal("Перу", result["query"]);
    }
}
