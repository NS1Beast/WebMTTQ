using Microsoft.AspNetCore.Mvc;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers;

public class NewsController : Controller
{
    public IActionResult Index(string category = "tin-tuc", int page = 1)
    {
        return View();
    }

   
    
}