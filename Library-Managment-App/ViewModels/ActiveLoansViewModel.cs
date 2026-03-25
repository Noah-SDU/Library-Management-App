using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Library_Managment_App.ViewModels;

public class ActiveLoanItemViewModel : ViewModelBase
{
	public string BookTitle { get; }
	public string BookIsbn { get; }
	public string MemberName { get; }
	public string BorrowedAtText { get; }

	public ActiveLoanItemViewModel(string bookTitle, string bookIsbn, string memberName, DateTime borrowedAt)
	{
		BookTitle = bookTitle;
		BookIsbn = bookIsbn;
		MemberName = memberName;
		BorrowedAtText = borrowedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
	}
}

public class ActiveLoansViewModel : ViewModelBase
{
	private readonly LibraryService _libraryService;
	private const int OverdueDays = 30;
	private int _activeLoanCount;
	private int _overdueLoanCount;

	public ObservableCollection<ActiveLoanItemViewModel> ActiveLoans { get; } = new();

	public int ActiveLoanCount
	{
		get => _activeLoanCount;
		private set => SetProperty(ref _activeLoanCount, value);
	}

	public int OverdueLoanCount
	{
		get => _overdueLoanCount;
		private set => SetProperty(ref _overdueLoanCount, value);
	}

	public bool HasActiveLoans => ActiveLoans.Count > 0;
	public bool NoActiveLoans => ActiveLoans.Count == 0;

	public ActiveLoansViewModel(LibraryService libraryService)
	{
		_libraryService = libraryService;
		Refresh();
	}

	public void Refresh()
	{
		ActiveLoans.Clear();

		var loans = _libraryService.GetActiveLoans().OrderByDescending(l => l.BorrowedAt).ToList();
		var books = _libraryService.GetBooks().ToDictionary(b => b.Id);
		var members = _libraryService.GetMembers().ToDictionary(m => m.Id);

		foreach (var loan in loans)
		{
			var bookTitle = books.TryGetValue(loan.BookId, out var book) ? book.Title : $"Book #{loan.BookId}";
			var bookIsbn = books.TryGetValue(loan.BookId, out book) && !string.IsNullOrWhiteSpace(book.ISBN) ? book.ISBN : "N/A";
			var memberName = members.TryGetValue(loan.MemberId, out var member) ? member.Username : $"Member #{loan.MemberId}";
			ActiveLoans.Add(new ActiveLoanItemViewModel(bookTitle, bookIsbn, memberName, loan.BorrowedAt));
		}

		ActiveLoanCount = loans.Count;
		OverdueLoanCount = loans.Count(l => l.BorrowedAt <= DateTime.UtcNow.AddDays(-OverdueDays));

		OnPropertyChanged(nameof(HasActiveLoans));
		OnPropertyChanged(nameof(NoActiveLoans));
	}
}
