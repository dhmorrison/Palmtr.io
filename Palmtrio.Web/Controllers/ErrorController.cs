using System.Web.Mvc;

namespace Palmtrio.Web.Controllers
{
    public class ErrorController : Controller
    {



        public ViewResult NotFound()
        {
            var httpNotFound = new HttpNotFoundResult();
            Response.Clear();
            Response.StatusCode = httpNotFound.StatusCode;
            Response.StatusDescription = httpNotFound.StatusDescription;
            Response.TrySkipIisCustomErrors = true;

            return View("~/Views/Error/NotFound.cshtml");
        }





    }
}