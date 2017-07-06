using Palmtrio.Web.Models;
using Palmtrio.Domain.Interfaces;
using Palmtrio.Domain.Models;
using System;
using System.Web.Mvc;

namespace Palmtrio.Web.Controllers
{
    public class LandingController : Controller
    {
        private ISessionContextManager _sessionContextManager;
        private IPageHitCounter _hitCounter;

        private IApiResourceManager _videoStreamManager;

        public LandingController(
            ISessionContextManager InjSessionContextManager,
            IPageHitCounter InjHitCounter,
            IApiResourceManager InjVideoStreamManager
            )
        {
            _sessionContextManager = InjSessionContextManager;
            _hitCounter = InjHitCounter;
            _videoStreamManager = InjVideoStreamManager;
        }

        [HttpGet]
        public ActionResult Index(string panel = "")
        {
            if (panel != "")
            {
                TempData["panelRequest"] = panel;
                return RedirectToAction("Index");
            }

            if(TempData["panelRequest"] != null)
            {
                string panelRequested = TempData["panelRequest"].ToString();
                switch(panelRequested)
                {
                    case "view-contact":
                    case "view-team":
                        ViewData["panelRequested"] = panelRequested;
                        break;
                }
            }


            string ip = "<ip>" + Request.UserHostAddress.ToString() + "</ip>";
            _hitCounter.Count("<hit>" + ip + "</hit>");

            _sessionContextManager.SetUserIp(ip);

            return View("~/Views/Landing/Index.cshtml");
        }

        [ActionName("view-contact")]
        public PartialViewResult ViewContact()
        {
            return PartialView("~/Views/Landing/Partials/ContactPartial.cshtml");
        }

        [ActionName("view-team")]
        public PartialViewResult ViewTeam()
        {
            return PartialView("~/Views/Landing/Partials/TeamPartial.cshtml", _sessionContextManager);
        }

        [ActionName("view-team-detail")]
        public PartialViewResult ViewTeamDetail(int TeamDetailId)
        {
            var videoStreamId = Guid.NewGuid();
            _videoStreamManager.RouteApiResource(videoStreamId);

            var model = new VideoStreamPartialViewModel();
            model.VideoStreamId = videoStreamId;

            switch(TeamDetailId)
            {
                case 1:
                    return PartialView("~/Views/Landing/Partials/TeamDetail1Partial.cshtml", model);
                case 2:
                    return PartialView("~/Views/Landing/Partials/TeamDetail2Partial.cshtml", model);
                case 3:
                    return PartialView("~/Views/Landing/Partials/TeamDetail3Partial.cshtml", model);

                case -1:
                default:
                    /*
                     * Logging!
                     */
                    return PartialView("~/Views/Landing/Partials/TeamDetail1Partial.cshtml", model);
            }
        }

        [ActionName("contact-submit")]
        public JsonResult ContactSubmit(ContactInfoModel ContactInfo)
        {

            return Json(new { isSuccess = true }, JsonRequestBehavior.DenyGet);
        }

#if DEBUG
        [HttpGet]
        public ViewResult ViewStyleGuide()
        {

            return View("~/Views/Landing/StyleGuide.cshtml");
        }
#endif



    }
}