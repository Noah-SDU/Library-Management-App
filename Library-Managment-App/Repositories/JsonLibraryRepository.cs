using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Library_Managment_App;

public class JsonLibraryRepository : ILibraryRepository
{
    private readonly string _filePath;
    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonLibraryRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<LibraryState> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new LibraryState();
        }

        var json = await File.ReadAllTextAsync(_filePath);
        if(string.IsNullOrWhiteSpace(json))
        {
            return new LibraryState();
        }
        return JsonSerializer.Deserialize<LibraryState>(json, _jsonOptions) ?? new LibraryState();
    }
    
    public async Task SaveAsync(LibraryState state)
    {
        var json = JsonSerializer.Serialize(state, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}