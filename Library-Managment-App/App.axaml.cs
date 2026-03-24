using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Library_Managment_App.ViewModels;
using Library_Managment_App.Views;

namespace Library_Managment_App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var repository = CreateRepository();
            var state = LoadState(repository);
            var authService = new AuthService(state);
            var libraryService = new LibraryService(state);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(
                    authService,
                    libraryService,
                    CreateSaveChangesCallback(repository, state)),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static JsonLibraryRepository CreateRepository()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "library-data.json");
        return new JsonLibraryRepository(filePath);
    }

    private static LibraryState LoadState(JsonLibraryRepository repository)
    {
        return Task.Run(() => LibraryStartup.LoadOrSeedAsync(repository)).GetAwaiter().GetResult();
    }

    private static Action CreateSaveChangesCallback(JsonLibraryRepository repository, LibraryState state)
    {
        return () => Task.Run(() => repository.SaveAsync(state)).GetAwaiter().GetResult();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}