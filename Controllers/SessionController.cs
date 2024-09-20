//Robert Glowacki//
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Projekt_LiterallyCounting.Controllers
{
    public class SessionController : Controller
    {
        public IActionResult GameSession(string sessionId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister", "Account");
            }

            var mail = User.FindFirst(ClaimTypes.Email)?.Value;
            var isBlockedClaim = User.FindFirst("isBlocked")?.Value;
            bool isBlocked = isBlockedClaim != null && bool.Parse(isBlockedClaim);

            if(isBlocked)
            {
                return RedirectToAction("AccountHome", "Account");
            }

            ViewBag.SessionId = sessionId;
            ViewBag.Email = mail;
            return View();
        }
    }
}