using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Library_Managment_App.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _selectedRole = "Member";
    private string? _errorMessage;

    private static readonly IReadOnlyDictionary<string, string> RoleToBackground =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Member"] = "#F1EAD8",
            ["Librarian"] = "#BEC5A4"
        };

    private static readonly IReadOnlyDictionary<string, string> RoleToBorder =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Member"] = "#C5B8A1",
            ["Librarian"] = "#8A8E75"
        };

    private static readonly IReadOnlyDictionary<string, Role> RoleMap =
        new Dictionary<string, Role>(StringComparer.Ordinal)
        {
            ["Member"] = Role.Member,
            ["Librarian"] = Role.Librarian
        };

    public event EventHandler<AuthResult>? LoginSucceeded;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public IReadOnlyList<string> RoleChoices { get; } = new[] { "Member", "Librarian" };

    public string SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (!SetProperty(ref _selectedRole, value))
            {
                return;
            }

            OnPropertyChanged(nameof(LoginBackground));
            OnPropertyChanged(nameof(LoginBorderBrush));
            OnPropertyChanged(nameof(LoginButtonBackground));
        }
    }

    public string LoginBackground => GetColor(RoleToBackground, "#F1EAD8");

    public string LoginBorderBrush => GetColor(RoleToBorder, "#C5B8A1");

    public string LoginButtonBackground => GetColor(RoleToBorder, "#C5B8A1");

    public ICommand LoginCommand { get; }

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        LoginCommand = new RelayCommand(Login);
    }

    private void Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Enter both username and password.";
            return;
        }

        var result = _authService.Authenticate(Username.Trim(), Password);

        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        var expectedRole = RoleMap.TryGetValue(SelectedRole, out var mappedRole)
            ? mappedRole
            : Role.Member;
        if (result.Role != expectedRole)
        {
            ErrorMessage = "Those credentials belong to a different account.";
            return;
        }

        ErrorMessage = null;
        LoginSucceeded?.Invoke(this, result);
    }

    private string GetColor(IReadOnlyDictionary<string, string> map, string fallback)
    {
        return map.TryGetValue(SelectedRole, out var color) ? color : fallback;
    }
}