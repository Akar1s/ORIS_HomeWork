using TourSearch.TemplateEngine;
using Xunit;

namespace TourSearch.Tests;

public class SimpleTemplateEngineTests
{
    private readonly SimpleTemplateEngine _engine;

    public SimpleTemplateEngineTests()
    {
        _engine = new SimpleTemplateEngine();
    }

    [Fact]
    public void Render_SimplePlaceholder_ReplacesCorrectly()
    {
                var template = "Hello, {{Name}}!";
        var model = new Dictionary<string, object?> { ["Name"] = "World" };

                var result = _engine.Render(template, model);

                Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void Render_MultiplePlaceholders_ReplacesAll()
    {
                var template = "{{Greeting}}, {{Name}}! Welcome to {{Place}}.";
        var model = new Dictionary<string, object?>
        {
            ["Greeting"] = "Hello",
            ["Name"] = "Traveler",
            ["Place"] = "Peru"
        };

                var result = _engine.Render(template, model);

                Assert.Equal("Hello, Traveler! Welcome to Peru.", result);
    }

    [Fact]
    public void Render_MissingKey_ReturnsEmptyString()
    {
                var template = "Hello, {{Name}}!";
        var model = new Dictionary<string, object?>();

                var result = _engine.Render(template, model);

                Assert.Equal("Hello, !", result);
    }

    [Fact]
    public void Render_NullValue_ReturnsEmptyString()
    {
                var template = "Value: {{Value}}";
        var model = new Dictionary<string, object?> { ["Value"] = null };

                var result = _engine.Render(template, model);

                Assert.Equal("Value: ", result);
    }

    [Fact]
    public void Render_EachLoop_RendersCollection()
    {
                var template = "{{#each Items}}Item: {{Name}} {{/each}}";
        var model = new Dictionary<string, object?>
        {
            ["Items"] = new[]
            {
                new { Name = "A" },
                new { Name = "B" },
                new { Name = "C" }
            }
        };

                var result = _engine.Render(template, model);

                Assert.Equal("Item: A Item: B Item: C ", result);
    }

    [Fact]
    public void Render_EachLoop_EmptyCollection_ReturnsEmpty()
    {
                var template = "Before {{#each Items}}Item: {{Name}} {{/each}}After";
        var model = new Dictionary<string, object?>
        {
            ["Items"] = Array.Empty<object>()
        };

                var result = _engine.Render(template, model);

                Assert.Equal("Before After", result);
    }

    [Fact]
    public void Render_IfCondition_ShowsWhenTrue()
    {
                var template = "{{#if ShowMessage}}Hello!{{/if}}";
        var model = new Dictionary<string, object?> { ["ShowMessage"] = "yes" };

                var result = _engine.Render(template, model);

                Assert.Equal("Hello!", result);
    }

    [Fact]
    public void Render_IfCondition_HidesWhenFalse()
    {
                var template = "Before{{#if ShowMessage}}Hello!{{/if}}After";
        var model = new Dictionary<string, object?> { ["ShowMessage"] = "" };

                var result = _engine.Render(template, model);

                Assert.Equal("BeforeAfter", result);
    }

    [Fact]
    public void Render_IfCondition_HidesWhenNull()
    {
                var template = "Before{{#if ShowMessage}}Hello!{{/if}}After";
        var model = new Dictionary<string, object?> { ["ShowMessage"] = null };

                var result = _engine.Render(template, model);

                Assert.Equal("BeforeAfter", result);
    }

    [Fact]
    public void Render_NestedProperty_ResolvedCorrectly()
    {
                var template = "Tour: {{Tour.Name}}";
        var model = new Dictionary<string, object?>
        {
            ["Tour"] = new { Name = "Peru Adventure" }
        };

                var result = _engine.Render(template, model);

                Assert.Equal("Tour: Peru Adventure", result);
    }

    [Fact]
    public void Render_NumberValues_ConvertedToString()
    {
                var template = "Price: {{Price}} Days: {{Days}}";
        var model = new Dictionary<string, object?>
        {
            ["Price"] = 499.99m,
            ["Days"] = 7
        };

                var result = _engine.Render(template, model);

                Assert.Equal("Price: 499.99 Days: 7", result);
    }

    [Fact]
    public void Render_ThrowsOnNullTemplate()
    {
                var model = new Dictionary<string, object?>();

                Assert.Throws<ArgumentNullException>(() => _engine.Render(null!, model));
    }

    [Fact]
    public void Render_ThrowsOnNullModel()
    {
                var template = "Hello";

                Assert.Throws<ArgumentNullException>(() => _engine.Render(template, null!));
    }
}
