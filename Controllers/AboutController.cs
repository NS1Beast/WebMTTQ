using Microsoft.AspNetCore.Mvc;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers;

public class AboutController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

   
}