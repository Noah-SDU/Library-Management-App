using System;
using System.Collections.Generic;
using System.Linq;

namespace Library_Managment_App;

public class LibraryService
{
    private readonly LibraryState  _state;

    public LibraryService(LibraryState state)
    {
        _state = state;
    }
    
    public IReadOnlyList<Member> GetMembers() => _state.Members;
    public IReadOnlyList<Book> GetBooks() => _state.Books;

    public void AddMember(Member member)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));
        _state.Members.Add(member);
    }

    public void AddBook(Book book)
    {
        if (book == null) throw new ArgumentNullException(nameof(book));
        _state.Books.Add(book);
    }
    
    public void BorrowBook(Book book, Member member)
    {
        if (book == null) throw new ArgumentNullException(nameof(book));
        if (member == null) throw new ArgumentNullException(nameof(member));
        
        int activeLoans = _state.Loans.Count(l => l.BookId == book.Id && l.ReturnedAt == null);
        if(activeLoans >= book.TotalCopies)
        {
            throw new InvalidOperationException("All copies of the book are currently borrowed.");
        }

        if (_state.Loans.Any(l => l.BookId == book.Id && l.MemberId == member.Id && l.ReturnedAt == null))
        {
            throw new InvalidOperationException("You already have this book on loan.");
        }

        var loan = new Loan
        {
            BookId = book.Id,
            MemberId = member.Id,
            BorrowedAt = DateTime.UtcNow
        };
        _state.Loans.Add(loan);

    }

    public void EditBook(Book book, string title, string author, string isbn, string description, string genre, int? yearPublished, int totalCopies)
    {
        if (book == null) throw new ArgumentNullException(nameof(book));
        
        book.Title = title;
        book.Author = author;
        book.ISBN = isbn;
        book.Description = description;
        book.Genre = genre;
        book.YearPublished = yearPublished;
        book.TotalCopies = totalCopies;
    }

    public void ReturnBook(Book book, Member member)
    {
        if(book == null) throw new ArgumentNullException(nameof(book));
        if(member == null) throw new ArgumentNullException(nameof(member));
        var activeLoan = _state.Loans.FirstOrDefault(l => l.BookId == book.Id && l.MemberId == member.Id && l.ReturnedAt == null);
        if(activeLoan == null)
        {
            throw new InvalidOperationException("This book is not currently borrowed.");
        }
        
        activeLoan.ReturnedAt = DateTime.UtcNow;
    }
    
    public void DeleteBook(Book book)
    {
        if (book == null) throw new ArgumentNullException(nameof(book));
        if(_state.Loans.Any(l => l.BookId == book.Id && l.ReturnedAt == null))
        {
            throw new InvalidOperationException("Cannot delete a book that is currently borrowed.");
        }
        
        _state.Books.Remove(book);
    }

    public List<Loan> GetActiveLoans()
    {
        return _state.Loans.Where(l => l.ReturnedAt == null).ToList();
    }

    public void RateBook(int memberId, int bookId, int stars)
    {
        if (stars < 1 || stars > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5 stars.");
        }
        
        bool hasBorrowed = _state.Loans.Any(l => l.MemberId == memberId && l.BookId == bookId);
        if (!hasBorrowed)
        {
            throw new InvalidOperationException("You can only rate books you have borrowed and returned.");
        }
        
        var rating = _state.Ratings.FirstOrDefault(r => r.MemberId == memberId && r.BookId == bookId);
        if (rating != null)
        {
            rating.RatingStars = stars;
            rating.RatedAt = DateTime.UtcNow;
        }
        else
        {
            int newId = _state.Ratings.Count > 0 ? _state.Ratings.Max(r => r.Id) + 1 : 1;
            _state.Ratings.Add(new Rating
            {
                Id = newId,
                BookId = bookId,
                MemberId = memberId,
                RatingStars = stars,
                RatedAt = DateTime.UtcNow
            });
        }
    }

    public double? GetAverageRating(int bookId)
    {
        var ratings = _state.Ratings.Where(r => r.BookId == bookId).ToList();      
        if (ratings.Count == 0) return null;
        return ratings.Average(r => r.RatingStars);
    }

    public int? GetUserRating(int memberId, int bookId)
    {
        var rating = _state.Ratings.FirstOrDefault(r => r.MemberId == memberId && r.BookId == bookId);
        return rating?.RatingStars;
    }

    public int GetRatedBooksCount(int memberId)
    {
        return _state.Ratings
            .Where(r => r.MemberId == memberId)
            .Select(r => r.BookId)
            .Distinct()
            .Count();
    }

    public int GetAvailableCopies(int bookId)
    {
        var book = _state.Books.FirstOrDefault(b => b.Id == bookId);
        if (book == null) throw new ArgumentException("Book not found.");
        int activeLoans = _state.Loans.Count(l => l.BookId == bookId && l.ReturnedAt == null);
        return book.TotalCopies - activeLoans;
    }
}