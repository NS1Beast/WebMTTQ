using Microsoft.AspNetCore.Mvc;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    [Route("admin")]
    public class AdminController : BaseAdminController
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
