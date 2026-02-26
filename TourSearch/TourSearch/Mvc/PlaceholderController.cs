using System.Net;
using TourSearch.Mvc;

namespace TourSearch.Controllers;

public class PlaceholderController : IController
{
    public Task<ControllerResult> HandleAsync(HttpListenerContext context)
    {
        var html = @"
<!DOCTYPE html>
<html lang='ru'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Coming Soon</title>
    <style>
        body {
            margin: 0;
            font-family: 'Inter', -apple-system, sans-serif;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            background: linear-gradient(135deg, #422f65, #5c4587);
            color: white;
            text-align: center;
            padding: 20px;
        }
        .container {
            max-width: 600px;
        }
        h1 {
            font-size: 3rem;
            margin-bottom: 1rem;
        }
        p {
            font-size: 1.25rem;
            margin-bottom: 2rem;
            opacity: 0.9;
        }
        a {
            display: inline-block;
            padding: 12px 32px;
            background: white;
            color: #422f65;
            text-decoration: none;
            border-radius: 25px;
            font-weight: 600;
            transition: transform 0.2s;
        }
        a:hover {
            transform: scale(1.05);
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚧 Coming Soon</h1>
        <p>This page is under construction. Check back soon!</p>
        <a href='/'>← Back to Home</a>
    </div>
</body>
</html>";

        return Task.FromResult<ControllerResult>(new HtmlResult(html, 200));
    }
}
