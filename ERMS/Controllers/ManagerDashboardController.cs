using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMS.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
