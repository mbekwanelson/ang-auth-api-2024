using System.ComponentModel.DataAnnotations;

namespace ang_auth_api_2024.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }

    }
}
