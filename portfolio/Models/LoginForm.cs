using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

public class LoginForm
{
    [Required(ErrorMessage = "Enter your username.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}