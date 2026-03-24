using System.Linq;

namespace Library_Managment_App;

public class AuthService
{
    private readonly LibraryState _state;
    
    private const string LibrarianUsername = "librarian";
    private const string LibrarianPassword = "admin";

    public AuthService(LibraryState state)
    {
        _state = state;
    }

    public AuthResult Authenticate(string username, string password)
    {
        if (username == LibrarianUsername && password == LibrarianPassword)
        {
            return new AuthResult(true, Role.Librarian, null, null);
        }
        
        var member = _state.Members.FirstOrDefault(m => m.Username == username && m.Password == password);
        if (member != null)
        {
            return new AuthResult(true, Role.Member, member.Id, null);
        }
        
        return new AuthResult(false, null, null, "Invalid username or password.");
    }
}