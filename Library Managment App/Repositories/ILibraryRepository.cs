using System.Threading.Tasks;

namespace Library_Managment_App;

public interface ILibraryRepository
{
    Task<LibraryState> LoadAsync();
    Task SaveAsync(LibraryState state);
}