using System.Linq;

namespace Library_Managment_App;

public class AuthService
{
    private readonly LibraryState _state;

    public AuthService(LibraryState state)
    {
        _state = state;
    }

    public AuthResult Authenticate(string username, string password)
    {
        var member = _state.Members.FirstOrDefault(m => m.Username == username && m.Password == password);
        if (member != null)
        {
            return new AuthResult(true, Role.Member, member.Id, null);
        }

        var librarian = _state.Librarians.FirstOrDefault(l => l.Username == username && l.Password == password);
        if (librarian != null)
        {
            return new AuthResult(true, Role.Librarian, null, null);
        }
        
        return new AuthResult(false, null, null, "Invalid username or password.");
    }
}