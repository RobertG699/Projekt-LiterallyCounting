//Robert//
namespace Projekt_LiterallyCounting.Models
{
    public class UserViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public bool Blocked { get; set; }
        public int Points { get; set; }
    }
}