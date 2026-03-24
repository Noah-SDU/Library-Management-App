namespace Library_Managment_App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private bool _isAuthenticated;
    private Role? _authenticatedRole;
    public LoginViewModel Login { get; }

    public bool ShowLoginPanel => !_isAuthenticated;

    public bool ShowMainPanel => _isAuthenticated;

    public bool ShowMemberPanel => _isAuthenticated && _authenticatedRole == Role.Member;

    public bool ShowLibrarianPanel => _isAuthenticated && _authenticatedRole == Role.Librarian;

    public string WindowBackground => "#262527";

    public MainWindowViewModel() : this(new AuthService(new LibraryState()))
    {
    }

    public MainWindowViewModel(AuthService authService)
    {
        Login = new LoginViewModel(authService);
        Login.LoginSucceeded += (_, authResult) =>
        {
            _isAuthenticated = true;
            _authenticatedRole = authResult.Role;
            OnPropertyChanged(nameof(ShowLoginPanel));
            OnPropertyChanged(nameof(ShowMainPanel));
            OnPropertyChanged(nameof(ShowMemberPanel));
            OnPropertyChanged(nameof(ShowLibrarianPanel));
        };
    }

}
