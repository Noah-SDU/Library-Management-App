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
        set => SetProperty(ref _selectedRole, value);
    }

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

        var expectedRole = SelectedRole == "Librarian" ? Role.Librarian : Role.Member;
        if (result.Role != expectedRole)
        {
            ErrorMessage = "Those credentials belong to a different account.";
            return;
        }

        ErrorMessage = null;
        LoginSucceeded?.Invoke(this, result);
    }
}