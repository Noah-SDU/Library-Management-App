using System;
using Xunit;
using Library_Managment_App;
using Library_Managment_App.ViewModels;

namespace Library_Management_App.Tests;

public class LibraryManagerTests
{
    [Fact]
    public void Members_Can_Borrow_Books()
    {
        // Arrange
        var state = new LibraryState();
        var member = new Member { Id = 1, Username = "Alice", Password = "pw" };
        var book = new Book { Id = 100, Title = "Test Book", TotalCopies = 1 };
        state.Members.Add(member);
        state.Books.Add(book);
        state.Loans.Add(new Loan { Id = 1, BookId = book.Id, MemberId = member.Id });
        
        //Act
        int activeLoans = state.Loans.Count(l => l.BookId == book.Id && l.IsActive);
        bool isAvailable = activeLoans < book.TotalCopies;
        
        //Assert
        Assert.False(isAvailable);
    }

    [Fact]
    public void Members_Can_Return_Books()
    {
        //Arrange
        var member = new Member { Id = 1, Username = "Alice", Password = "pw" };
        var book = new Book { Id = 100, Title = "Test Book", TotalCopies = 1 };
        var state = new LibraryState();
        state.Members.Add(member);
        state.Books.Add(book);
        var loan = new Loan { Id = 1, BookId = book.Id, MemberId = member.Id };
        state.Loans.Add(loan);
        
        //Act
        loan.ReturnedAt = DateTime.UtcNow;
       
        //Assert
        Assert.False(loan.IsActive);
        int activeLoans = state.Loans.Count(l => l.BookId == book.Id && l.IsActive);
        Assert.Equal(0, activeLoans);
    }

    [Fact]
    public void Librarian_Can_Add_Book()
    {
        //Arrange
        var librarian = new Librarian { Id = 1, Username = "Admin", Password = "pw" };
        var book = new Book { Id = 200, Title = "New Book", TotalCopies = 1 };
        var state = new LibraryState();
        state.Librarians.Add(librarian);
        
        //Act
        state.Books.Add(book);
        
        //Assert
        Assert.Contains(book, state.Books);
    }

    
}
