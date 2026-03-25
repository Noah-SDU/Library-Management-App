﻿using System;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Library_Managment_App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private bool _isAuthenticated;
    private Role? _authenticatedRole;
    private int? _authenticatedMemberId;

    public LoginViewModel Login { get; }
    public LibrarianHomeViewModel LibrarianHome { get; }
    public MemberHomeViewModel MemberHome { get; }
    public ICommand ReturnToLoginCommand { get; }

    public bool ShowLoginPanel => !_isAuthenticated;

    public bool ShowMainPanel => _isAuthenticated;

    public bool ShowMemberPanel => _isAuthenticated && _authenticatedRole == Role.Member;

    public bool ShowLibrarianPanel => _isAuthenticated && _authenticatedRole == Role.Librarian;

    public string WindowBackground => "#262527";

    public MainWindowViewModel(AuthService authService, LibraryService libraryService, Action? persistChanges = null)
    {
        var librarianCatalog = new LibrarianCatalogViewModel(libraryService, persistChanges);
        var memberCatalog = new MemberCatalogViewModel(libraryService, persistChanges);
        var activeLoans = new ActiveLoansViewModel(libraryService);
        var myLoans = new MyLoansViewModel(libraryService, persistChanges);

        LibrarianHome = new LibrarianHomeViewModel(librarianCatalog, activeLoans);
        MemberHome = new MemberHomeViewModel(memberCatalog, myLoans, libraryService);

        Login = new LoginViewModel(authService);
        ReturnToLoginCommand = new RelayCommand(ReturnToLogin);
        HookEvents(activeLoans, librarianCatalog, memberCatalog, myLoans);
    }

    private void HookEvents(
        ActiveLoansViewModel activeLoans,
        LibrarianCatalogViewModel librarianCatalog,
        MemberCatalogViewModel memberCatalog,
        MyLoansViewModel myLoans)
    {
        librarianCatalog.StateChanged += (_, _) =>
        {
            activeLoans.Refresh();
            myLoans.Refresh();
            memberCatalog.RefreshBooks();
            MemberHome.RefreshDashboardStats();
        };

        memberCatalog.StateChanged += (_, _) =>
        {
            activeLoans.Refresh();
            myLoans.Refresh();
            librarianCatalog.RefreshBooks();
            memberCatalog.RefreshBooks();
            MemberHome.RefreshDashboardStats();
        };

        myLoans.StateChanged += (_, _) =>
        {
            activeLoans.Refresh();
            librarianCatalog.RefreshBooks();
            memberCatalog.RefreshBooks();
            MemberHome.RefreshDashboardStats();
        };

        Login.LoginSucceeded += (_, authResult) =>
        {
            _isAuthenticated = true;
            _authenticatedRole = authResult.Role;
            _authenticatedMemberId = authResult.MemberId;

            MemberHome.SetMemberContext(_authenticatedMemberId, Login.Username);
            MemberHome.RefreshDashboardStats();

            OnPropertyChanged(nameof(ShowLoginPanel));
            OnPropertyChanged(nameof(ShowMainPanel));
            OnPropertyChanged(nameof(ShowMemberPanel));
            OnPropertyChanged(nameof(ShowLibrarianPanel));
        };
    }

    private void ReturnToLogin()
    {
        _isAuthenticated = false;
        _authenticatedRole = null;
        _authenticatedMemberId = null;

        Login.Password = string.Empty;
        Login.ErrorMessage = null;

        OnPropertyChanged(nameof(ShowLoginPanel));
        OnPropertyChanged(nameof(ShowMainPanel));
        OnPropertyChanged(nameof(ShowMemberPanel));
        OnPropertyChanged(nameof(ShowLibrarianPanel));
    }
}
