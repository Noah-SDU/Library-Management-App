using System;
using System.Linq;
using Library_Managment_App;
using Library_Managment_App.ViewModels;

namespace Library_Management_App.Tests;

public class ViewModelsHeadlessTests
{
    [Fact]
    public void LoginViewModel_SetsError_WhenCredentialsMissing()
    {
        //Arrange
        var auth = new AuthService(new LibraryState());
        var vm = new LoginViewModel(auth)
        {
            Username = "",
            Password = ""
        };

        //Act
        vm.LoginCommand.Execute(null);

        //Assert
        Assert.Equal("Enter both username and password.", vm.ErrorMessage);
    }

    [Fact]
    public void LoginViewModel_RejectsRoleMismatch()
    {
        //Arrange
        var state = new LibraryState();
        state.Members.Add(new Member { Id = 7, Username = "alice", Password = "pw" });
        var auth = new AuthService(state);
        var vm = new LoginViewModel(auth)
        {
            Username = "alice",
            Password = "pw",
            SelectedRole = "Librarian"
        };

        //Act
        vm.LoginCommand.Execute(null);

        //Assert
        Assert.Equal("Those credentials belong to a different account.", vm.ErrorMessage);
    }

    [Fact]
    public void LoginViewModel_RaisesLoginSucceeded_ForValidMember()
    {
        //Arrange
        var state = new LibraryState();
        state.Members.Add(new Member { Id = 1, Username = "Alice", Password = "password1" });
        var auth = new AuthService(state);
        var vm = new LoginViewModel(auth)
        {
            Username = "Alice",
            Password = "password1",
            SelectedRole = "Member"
        };

        AuthResult? received = null;
        vm.LoginSucceeded += (_, result) => received = result;

        //Act
        vm.LoginCommand.Execute(null);

        //Assert
        Assert.Null(vm.ErrorMessage);
        Assert.NotNull(received);
        Assert.True(received!.Success);
        Assert.Equal(Role.Member, received.Role);
        Assert.Equal(1, received.MemberId);
    }

    [Fact]
    public void LibrarianCatalogViewModel_AddBook_AddsBookAndSetsStatus()
    {
        //Arrange
        var state = new LibraryState();
        var service = new LibraryService(state);
        var vm = new LibrarianCatalogViewModel(service)
        {
            NewTitle = "Domain-Driven Design",
            NewAuthor = "Eric Evans",
            NewIsbn = "9780321125217",
            NewCopiesText = "2"
        };

        //Act
        vm.AddBookCommand.Execute(null);

        //Assert
        Assert.Single(service.GetBooks());
        var added = service.GetBooks().Single();
        Assert.Equal("Domain-Driven Design", added.Title);
        Assert.Equal(2, added.TotalCopies);
        Assert.Equal("Book added.", vm.StatusMessage);
    }

    [Fact]
    public void MemberHomeViewModel_RefreshDashboardStats_UpdatesCounts()
    {
        //Arrange
        var state = new LibraryState();
        state.Members.Add(new Member { Id = 1, Username = "Alice", Password = "pw" });
        state.Books.Add(new Book { Id = 100, Title = "Book", Author = "Author", TotalCopies = 2 });
        state.Loans.Add(new Loan { Id = 1, BookId = 100, MemberId = 1, BorrowedAt = DateTime.UtcNow });
        state.Ratings.Add(new Rating { Id = 1, BookId = 100, MemberId = 1, RatingStars = 4, RatedAt = DateTime.UtcNow });

        var service = new LibraryService(state);
        var catalog = new MemberCatalogViewModel(service);
        var myLoans = new MyLoansViewModel(service);
        var home = new MemberHomeViewModel(catalog, myLoans, service);

        //Act
        home.SetMemberContext(1, "Alice");

        //Assert
        Assert.Equal(1, home.CurrentLoansCount);
        Assert.Equal(1, home.BooksRatedCount);
    }

    [Fact]
    public void MyLoansViewModel_ReturnSelectedBooks_ReturnsLoanAndUpdatesStatus()
    {
        //Arrange
        var state = new LibraryState();
        var member = new Member { Id = 1, Username = "Alice", Password = "pw" };
        var book = new Book { Id = 10, Title = "The Hobbit", Author = "Tolkien", TotalCopies = 1 };
        state.Members.Add(member);
        state.Books.Add(book);
        state.Loans.Add(new Loan { Id = 1, BookId = 10, MemberId = 1, BorrowedAt = DateTime.UtcNow });

        var service = new LibraryService(state);
        var vm = new MyLoansViewModel(service);
        vm.SetMemberContext(1, "Alice");

        var selected = vm.Loans.Single();
        vm.SelectedLoan = selected;

        //Act
        vm.ReturnSelectedBooksCommand.Execute(null);

        //Assert
        Assert.Equal("1 book returned.", vm.StatusMessage);
        Assert.Empty(service.GetActiveLoans());
    }

    [Fact]
    public void MyLoansViewModel_RateSelectedBook_CreatesRatingAndUpdatesStatus()
    {
        //Arrange
        var state = new LibraryState();
        var member = new Member { Id = 1, Username = "Alice", Password = "pw" };
        var book = new Book { Id = 10, Title = "The Hobbit", Author = "Tolkien", TotalCopies = 1 };
        state.Members.Add(member);
        state.Books.Add(book);
        state.Loans.Add(new Loan { Id = 1, BookId = 10, MemberId = 1, BorrowedAt = DateTime.UtcNow });

        var service = new LibraryService(state);
        var vm = new MyLoansViewModel(service);
        vm.SetMemberContext(1, "Alice");
        vm.SelectedLoan = vm.Loans.Single();
        vm.SelectedRating = 5;

        //Act
        vm.RateSelectedBookCommand.Execute(null);

        //Assert
        Assert.Equal("Rated \"The Hobbit\" 5/5.", vm.StatusMessage);
        Assert.Equal(5, service.GetUserRating(1, 10));
    }
}
