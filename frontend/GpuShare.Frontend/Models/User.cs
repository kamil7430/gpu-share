namespace GpuShare.Frontend.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    // public string Password { get; set; } = ""; // Password should not be stored in frontend models, but included here for completeness
    public bool Admin { get; set; } = false;

    public User()
    {
    }

    public User(string username, bool admin = false)
    {
        Username = username;
        Admin = admin;
    }
}