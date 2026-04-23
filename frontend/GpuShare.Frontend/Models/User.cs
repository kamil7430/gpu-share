public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool Admin { get; set; } = false;

    public User(string username, string password, bool admin = false)
    {
        Username = username;
        Password = password;
        Admin = admin;
    }
}