using System.Linq;
using System.Threading.Tasks;

namespace Library_Managment_App;

public static class LibraryStartup
{
    public static async Task<LibraryState> LoadOrSeedAsync(ILibraryRepository repo)
    {
        var state = await repo.LoadAsync().ConfigureAwait(false);

        bool seeded = false;

        if (state.Members.Count == 0)
        {
            SeedDefaultMembers(state);
            seeded = true;
        }

        if (state.Librarians.Count == 0)
        {
            SeedDefaultLibrarians(state);
            seeded = true;
        }

        if (MigrateLibrariansFromMembers(state))
        {
            seeded = true;
        }

        if (state.Books.Count == 0)
        {
            state.Books.Add(new Book { Id = 1, Title = "The Hobbit", Author = "J.R.R. Tolkien", TotalCopies = 3 });
            state.Books.Add(new Book { Id = 2, Title = "1984", Author = "George Orwell", TotalCopies = 2 });
            state.Books.Add(new Book { Id = 3, Title = "Clean Code", Author = "Robert C. Martin", TotalCopies = 1 });
            seeded = true;
        }

        if (seeded)
        {
            await repo.SaveAsync(state).ConfigureAwait(false);
        }

        return state;
    }

    private static void SeedDefaultMembers(LibraryState state)
    {
        state.Members.Add(new Member { Id = 1, Username = "Alice", Password = "password1" });
        state.Members.Add(new Member { Id = 2, Username = "Bob", Password = "password2" });
    }

    private static void SeedDefaultLibrarians(LibraryState state)
    {
        state.Librarians.Add(new Librarian { Id = 1, Username = "librarian", Password = "l" });
    }

    private static bool MigrateLibrariansFromMembers(LibraryState state)
    {
        var membersToConvert = state.Members
            .FindAll(member => IsLibrarianUsername(member.Username));

        if (membersToConvert.Count == 0)
        {
            return false;
        }

        int nextLibrarianId = state.Librarians.Count > 0
            ? state.Librarians.Max(l => l.Id) + 1
            : 1;

        foreach (var member in membersToConvert)
        {
            if (!state.Librarians.Exists(l => l.Username == member.Username))
            {
                state.Librarians.Add(new Librarian
                {
                    Id = nextLibrarianId++,
                    Username = member.Username,
                    Password = member.Password
                });
            }

            state.Members.Remove(member);
        }

        return true;
    }

    private static bool IsLibrarianUsername(string username)
    {
        return username.StartsWith("librarian", System.StringComparison.OrdinalIgnoreCase);
    }
}