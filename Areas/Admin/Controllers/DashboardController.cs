using System.Web.Mvc;

namespace FashionStore.Areas.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Login", "Account", new { area = "" });

            return View();
        }
    }
}