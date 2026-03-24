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
            state.Members.Add(new Member { Id = 1, Username = "Alice", Password = "password1" });
            state.Members.Add(new Member { Id = 2, Username = "Bob", Password = "password2" });
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
}