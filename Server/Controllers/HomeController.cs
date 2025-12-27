using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    [HttpGet("/")]
    public ActionResult Home()
    {
        return Content("Solitaire Solver up and running! 🚀", "text/plain", Encoding.UTF8);
    }
}
