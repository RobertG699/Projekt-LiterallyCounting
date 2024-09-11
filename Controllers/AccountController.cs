using Microsoft.AspNetCore.Mvc;
using Projekt_LiterallyCounting.Models;
using MySQLiteApp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Projekt_LiterallyCounting.Controllers
{
    public class AccountController : Controller
    {
        private async Task<bool> SignInUser(string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim("isAdmin", UserDataAccess.userIsAdmin(email).ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user with the specified authentication scheme
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return true;
        }

        [HttpGet]
        public IActionResult AccountHome()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null && bool.Parse(isAdminClaim);

            ViewBag.Email = email;
            ViewBag.Admin = isAdmin;

            return View("AccountHome");
        }

        [HttpGet]
        public IActionResult LoginRegister()
        {
            Console.WriteLine("Words:");
            WordDataAccess.readWords();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var isAdminClaim = User.FindFirst("isAdmin")?.Value;
                bool isAdmin = isAdminClaim != null && bool.Parse(isAdminClaim);

                ViewBag.Email = email;
                ViewBag.Admin = isAdmin;

                return RedirectToAction("AccountHome");
            }

            /*UserDataAccess.createuserUserTable();
            UserDataAccess.insertUser("mail1", "123", false, false);
            UserDataAccess.insertUser("mail2", "123", false, true);
            UserDataAccess.insertUser("mail3", "123", false, true);
            UserDataAccess.insertUser("mailadmin", "123", true, true);

            WordDataAccess.createuserWordTable();
            WordDataAccess.insertWord("Baum", "pos1", "type1", null);
            WordDataAccess.insertWord("Haus", "pos2", "type3", true);
            WordDataAccess.insertWord("Lecker", "pos1", "type2", false);*/

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(LoginRegisterViewModel model)
        {
            if (String.IsNullOrEmpty(model.RegisterEmail) || String.IsNullOrEmpty(model.RegisterConfirmPassword))
            {
                ViewBag.Message = "Email and Password can not be empty.";
            }
            else if (UserDataAccess.emailExists(model.RegisterEmail))
            {
                ViewBag.Message = "Email already exists.";
            }
            else if (model.RegisterPassword != model.RegisterConfirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
            }
            else
            {
                UserDataAccess.insertUser(model.RegisterEmail, model.RegisterPassword, false, true);
                UserDataAccess.readUsers();
                await SignInUser(model.RegisterEmail);
                return RedirectToAction("AccountHome");
            }

            return View("LoginRegister");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRegisterViewModel model)
        {
            if (UserDataAccess.validatedUser(model.LoginEmail, model.LoginPassword))
            {
                await SignInUser(model.LoginEmail);

                return RedirectToAction("AccountHome");
            }
            ViewBag.Message = "Invalid credentials. Please try again.";
            UserDataAccess.readUsers();

            return View("LoginRegister");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("LoginRegister");
        }

        [HttpGet]
        public IActionResult AccountManagement()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null && bool.Parse(isAdminClaim);

            ViewBag.Email = email;
            ViewBag.Admin = isAdmin;

            return View("AccountManagement");
        }

        [HttpPost]
        public IActionResult UpdatePassword(ChangeCredentialsViewModel model)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            string? email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null || !UserDataAccess.validatedUser(email, model.ChCredCurPassword))
            {
                ViewBag.Message = "Incorrect validation.";
            }
            else if (String.IsNullOrEmpty(model.ChCredConfirmPassword))
            {
                ViewBag.Message = "New Password can not be empty.";
            }
            else if (model.ChCredPassword != model.ChCredConfirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
            }
            else
            {
                UserDataAccess.updateUserPassword(model.ChCredPassword, email);
                UserDataAccess.readUsers();

                ViewBag.Message = "Password successfully changed.";
            }

            ViewBag.Email = email;
            return View("AccountManagement");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmail(ChangeCredentialsViewModel model)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            string? email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null || !UserDataAccess.validatedUser(email, model.ChCredCurPassword))
            {
                ViewBag.Message = "Incorrect validation.";
                ViewBag.Email = email;
            }
            else if (String.IsNullOrEmpty(model.ChCredEmail))
            {
                ViewBag.Message = "New Email can not be empty.";
                ViewBag.Email = email;
            }
            else
            {
                UserDataAccess.updateUserEmail(model.ChCredEmail, email);
                UserDataAccess.readUsers();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await SignInUser(model.ChCredEmail);

                ViewBag.Email = model.ChCredEmail;

                ViewBag.Message = "Email successfully changed.";
            }

            return View("AccountManagement");
        }

        [HttpGet]
        public IActionResult AllUsers()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null && bool.Parse(isAdminClaim);

            if(!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }

            List<UserViewModel> users = UserDataAccess.readUsers();

            return View(users);
        }

        [HttpGet]
        public IActionResult AdminPanel()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null && bool.Parse(isAdminClaim);

            if(!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }

            return View();
        }

        [HttpGet]
        public IActionResult AllWords()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null && bool.Parse(isAdminClaim);

            if (!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }

            List<WordViewModel> words = WordDataAccess.readWords();

            return View(words);
        }

        [HttpPost]
        public IActionResult DeleteUser(string userEmail)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null && bool.Parse(isAdminClaim);

            if (!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }
            Console.WriteLine(userEmail);
            UserDataAccess.deleteUser(userEmail);

            return RedirectToAction("AllUsers");
        }
    }
}
