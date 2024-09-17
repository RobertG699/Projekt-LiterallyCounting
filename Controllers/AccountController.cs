using Microsoft.AspNetCore.Mvc;
using Projekt_LiterallyCounting.Models;
using MySQLiteApp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MySQLiteApp.Game;

namespace Projekt_LiterallyCounting.Controllers
{
    public class AccountController : Controller
    {
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController()
        {
            _passwordHasher = new PasswordHasher<User>(); // Initializes the password hasher
        }

        private async Task<bool> SignInUser(string email)
        {
            if (User.Identity != null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim("isAdmin", UserDataAccess.userIsAdmin(email).ToString()),
                new Claim("isBlocked", UserDataAccess.userIsBlocked(email).ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user with the specified authentication scheme
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return true;
        }

        private void InsertUser(string userName, string userPassword, bool admin, bool blocked){
            string passwordHash = GetHash(userName, userPassword);
            UserDataAccess.insertUser(userName, passwordHash, admin, blocked);
        }

        private string GetHash(string userName, string userPassword){
            User user = new User
            {
                Username = userName,
                Password = userPassword
            };

            string passwordHash = _passwordHasher.HashPassword(user, user.Password);
            return passwordHash;
        }

        private bool ValidatedUser(string userName, string userPassword){
            User user = new User
            {
                Username = userName,
                Password = userPassword
            };
            var result = _passwordHasher.VerifyHashedPassword(user, UserDataAccess.getPasswordHash(user.Username), user.Password);

            return result == PasswordVerificationResult.Success;
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
            bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

            ViewBag.Email = email;
            ViewBag.Admin = isAdmin;

            return View("AccountHome");
        }

        [HttpGet]
        public IActionResult LoginRegister()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var isAdminClaim = User.FindFirst("isAdmin")?.Value;
                bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

                ViewBag.Email = email;
                ViewBag.Admin = isAdmin;

                return RedirectToAction("AccountHome");
            }

            /*UserDataAccess.createuserUserTable();

            User user = new User();
            string passwordHash = _passwordHasher.HashPassword(user, "123");

            //LoginRegisterViewModel model = new LoginRegisterViewModel();
            //string passwordHash = _passwordHasher.HashPassword(model, "123");

            UserDataAccess.insertUser("mail1", passwordHash, false, false, 100);
            UserDataAccess.insertUser("mail2", passwordHash, false, false, 20);
            UserDataAccess.insertUser("mail3", passwordHash, false, true, 0);
            UserDataAccess.insertUser("mail4", passwordHash, false, true, 0);
            UserDataAccess.insertUser("mailadmin1", passwordHash, true, false, 55);
            UserDataAccess.insertUser("mailadmin2", passwordHash, true, false, 25);

            /*HighScoreDataAccess.createuserHighScoreTable();
            HighScoreDataAccess.insertUser("mail1", 54);
            HighScoreDataAccess.insertUser("mail2", 72);

            /*WordDataAccess.createuserWordTable();
            WordDataAccess.insertWord("Baum", "pos1", "type1", null);
            WordDataAccess.insertWord("Haus", "pos2", "type3", true);
            WordDataAccess.insertWord("Lecker", "pos1", "type2", false);*/

            /*UserDataAccess.readUsers();
            HighScoreDataAccess.readHighscores();
            WordDataAccess.readWords();*/

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
                InsertUser(model.RegisterEmail, model.RegisterPassword, false, true);
                await SignInUser(model.RegisterEmail);
                return RedirectToAction("AccountHome");
            }

            return View("LoginRegister");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRegisterViewModel model)
        {
            if (ValidatedUser(model.LoginEmail, model.LoginPassword))
            {
                await SignInUser(model.LoginEmail);
                return RedirectToAction("AccountHome");
            }

            ViewBag.Message = "Invalid credentials. Please try again.";

            return View("LoginRegister");
        }

        [HttpPost]
        public IActionResult AddUser(AllUserViewModel model)
        {
            if (String.IsNullOrEmpty(model.newUser.Email) || String.IsNullOrEmpty(model.newUser.Password))
            {
                TempData["Message"] = "Email und Password dürfen nicht leer sein.";
            }
            else if (UserDataAccess.emailExists(model.newUser.Email))
            {
                TempData["Message"] = "Email existiert bereits.";
            }
            else
            {
                InsertUser(model.newUser.Email, model.newUser.Password, model.newUser.IsAdmin, false);
                TempData["Message"] = "Benutzer hinzugefügt.";
            }

            return RedirectToAction("AllUsers");
        }

        [HttpPost]
        public IActionResult AddWord(AllWordsViewModel model)
        {
            if (String.IsNullOrEmpty(model.newWord.Word) || 
                String.IsNullOrEmpty(model.newWord.Pos) || 
                String.IsNullOrEmpty(model.newWord.Type))
            {
                TempData["Message"] = "Wort, Position und Typ dürfen nicht leer sein.";
            }
            else if (WordDataAccess.wordExists(model.newWord.Word))
            {
                TempData["Message"] = "Wort existiert bereits.";
            }
            else
            {
                WordDataAccess.insertWord(model.newWord.Word, model.newWord.Pos, model.newWord.Type, true);
                TempData["Message"] = "Wort hinzugefügt.";
            }

            return RedirectToAction("AllWords");
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
            bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

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

            if (email == null || !ValidatedUser(email, model.ChCredCurPassword))
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
                string passwordHash = GetHash(email, model.ChCredPassword);
                UserDataAccess.updateUserPassword(passwordHash, email);

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

            if (email == null || !ValidatedUser(email, model.ChCredCurPassword))
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

            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

            if(!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }

            List<UserViewModel> users = UserDataAccess.readUsers();
            UserViewModel user = new UserViewModel();

            AllUserViewModel viewModel = new AllUserViewModel{
                allUsers = users,
                newUser = user
            };

            return View(viewModel);
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
            bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

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

            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

            if (!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }

            List<WordViewModel> words = WordDataAccess.readWords();
            WordViewModel word = new WordViewModel();

            AllWordsViewModel viewModel = new AllWordsViewModel{
                allWords = words,
                newWord = word
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Highscores()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            List<UserViewModel> users = UserDataAccess.readUsers(true);
            UserViewModel user = new UserViewModel();

            AllUserViewModel viewModel = new AllUserViewModel{
                allUsers = users,
                newUser = user
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DeleteUser(string userEmail)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

            if (!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }

            UserDataAccess.deleteUser(userEmail);

            return RedirectToAction("AllUsers");
        }

        [HttpPost]
        public IActionResult DeleteWord (string wordToDelete)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

            if (!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }

            WordDataAccess.deleteWord(wordToDelete);

            return RedirectToAction("AllWords");
        }

        [HttpPost]
        public IActionResult UnblockUser(string userEmail)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            var isAdminClaim = User.FindFirst("isAdmin")?.Value;
            bool isAdmin = isAdminClaim != null ? bool.Parse(isAdminClaim) : false;

            if (!isAdmin)
            {
                return RedirectToAction("AccountHome");
            }

            UserDataAccess.unblockUser(userEmail);

            return RedirectToAction("AllUsers");
        }

        /*[HttpGet]
        public IActionResult Game()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            string word = "empty";
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            
            selecte word here
            word = WordDataAccess.GetWord();

            ViewBag.Word = word;

            return View();
        }*/

        [HttpPost]
        public IActionResult SendSolution()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("LoginRegister");
            }

            string word = "empty";
            string result = "";
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            
            /* selecte word here
            word = WordDataAccess.GetWord();*/

            ViewBag.Word = word;

            return RedirectToAction("Game");
        }
    }

    
}
