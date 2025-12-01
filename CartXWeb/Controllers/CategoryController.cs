using Microsoft.AspNetCore.Mvc;

namespace CartXWeb.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
