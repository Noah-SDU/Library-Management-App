namespace Library_Managment_App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private bool _isAuthenticated;

    public string Greeting { get; } = "Welcome to Avalonia!";

    public LoginViewModel Login { get; }

    public bool ShowLoginPanel => !_isAuthenticated;

    public bool ShowMainPanel => _isAuthenticated;

    public MainWindowViewModel() : this(new AuthService(new LibraryState()))
    {
    }

    public MainWindowViewModel(AuthService authService)
    {
        Login = new LoginViewModel(authService);
        Login.LoginSucceeded += (_, _) =>
        {
            _isAuthenticated = true;
            OnPropertyChanged(nameof(ShowLoginPanel));
            OnPropertyChanged(nameof(ShowMainPanel));
        };
    }
}
