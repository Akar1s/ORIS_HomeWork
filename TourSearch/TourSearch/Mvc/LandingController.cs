using System.Net;
using TourSearch.Mvc;

namespace TourSearch.Controllers;

public class LandingController : IController
{
    private readonly string _projectRoot;

    public LandingController(string projectRoot)
    {
        _projectRoot = projectRoot;
    }

    public async Task<ControllerResult> HandleAsync(HttpListenerContext context)
    {
        var viewPath = Path.Combine(_projectRoot, "Views", "Home", "Landing.html");

        var model = new Dictionary<string, object?>
        {
            ["Title"] = "G Adventures - Travel your heart out"
        };

        return new ViewResult(viewPath, model, 200);
    }
}
