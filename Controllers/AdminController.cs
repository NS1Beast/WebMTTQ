using Microsoft.AspNetCore.Mvc;

namespace WebMTTQ.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            // Trỏ tới View của Dashboard
            return View("~/Views/Admin/Dashboard/Index.cshtml");
        }
    }
}