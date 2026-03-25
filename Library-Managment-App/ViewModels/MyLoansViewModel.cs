using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Library_Managment_App.ViewModels;

public class MemberLoanItemViewModel : ViewModelBase
{
	private const int LoanPeriodDays = 30;

	public int BookId { get; }
	public string BookTitle { get; }
	public string BookIsbn { get; }
	public string BorrowedAtText { get; }
	public string DueDateText { get; }
	public string LoanStatusText { get; }
	public int? UserRating { get; }
	public string UserRatingText => UserRating == null ? "Your rating: not rated" : $"Your rating: {UserRating}/5";

	public MemberLoanItemViewModel(int bookId, string bookTitle, string bookIsbn, DateTime borrowedAt, int? userRating)
	{
		BookId = bookId;
		BookTitle = bookTitle;
		BookIsbn = bookIsbn;
		BorrowedAtText = borrowedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
		var dueDate = borrowedAt.AddDays(LoanPeriodDays);
		DueDateText = $"Due: {dueDate.ToLocalTime():yyyy-MM-dd}";
		LoanStatusText = dueDate < DateTime.UtcNow ? "Status: overdue" : "Status: on time";
		UserRating = userRating;
	}
}

public class MyLoansViewModel : ViewModelBase
{
	private readonly LibraryService _libraryService;
	private readonly Action? _persistChanges;

	private int? _memberId;
	private string _memberUsername = string.Empty;
	private int _selectedRating = 5;
	private MemberLoanItemViewModel? _selectedLoan;
	private IList<object?> _selectedLoans = new List<object?>();
	private string? _statusMessage;

	public event EventHandler? StateChanged;

	public ObservableCollection<MemberLoanItemViewModel> Loans { get; } = new();

	public MemberLoanItemViewModel? SelectedLoan
	{
		get => _selectedLoan;
		set => SetProperty(ref _selectedLoan, value);
	}

	public IList<object?> SelectedLoans
	{
		get => _selectedLoans;
		set => SetProperty(ref _selectedLoans, value);
	}

	public string? StatusMessage
	{
		get => _statusMessage;
		set => SetProperty(ref _statusMessage, value);
	}

	public IReadOnlyList<int> RatingChoices { get; } = new[] { 1, 2, 3, 4, 5 };

	public int SelectedRating
	{
		get => _selectedRating;
		set => SetProperty(ref _selectedRating, value);
	}

	public bool HasLoans => Loans.Count > 0;

	public bool NoLoans => Loans.Count == 0;

	public ICommand ReturnSelectedBooksCommand { get; }
	public ICommand RateSelectedBookCommand { get; }

	public MyLoansViewModel(LibraryService libraryService, Action? persistChanges = null)
	{
		_libraryService = libraryService;
		_persistChanges = persistChanges;
		ReturnSelectedBooksCommand = new RelayCommand(ReturnSelectedBooks);
		RateSelectedBookCommand = new RelayCommand(RateSelectedBook);
	}

	public void SetMemberContext(int? memberId, string? memberUsername = null)
	{
		var members = _libraryService.GetMembers();
		_memberId = memberId;
		_memberUsername = memberUsername?.Trim() ?? string.Empty;

		if (_memberId != null && members.Any(m => m.Id == _memberId.Value))
		{
			Refresh();
			return;
		}

		if (!string.IsNullOrWhiteSpace(_memberUsername))
		{
			var byUsername = members.FirstOrDefault(
				m => m.Username.Equals(_memberUsername, StringComparison.OrdinalIgnoreCase));
			if (byUsername != null)
			{
				_memberId = byUsername.Id;
				Refresh();
				return;
			}
		}

		_memberId = null;
		Refresh();
	}

	public void Refresh()
	{
		Loans.Clear();

		if (_memberId != null)
		{
			var books = _libraryService.GetBooks().ToDictionary(b => b.Id);
			var loans = _libraryService.GetActiveLoans()
				.Where(l => l.MemberId == _memberId.Value)
				.OrderByDescending(l => l.BorrowedAt);

			foreach (var loan in loans)
			{
				var title = books.TryGetValue(loan.BookId, out var book) ? book.Title : $"Book #{loan.BookId}";
				var isbn = books.TryGetValue(loan.BookId, out book) && !string.IsNullOrWhiteSpace(book.ISBN) ? book.ISBN : "N/A";
				var userRating = _libraryService.GetUserRating(_memberId.Value, loan.BookId);
				Loans.Add(new MemberLoanItemViewModel(loan.BookId, title, isbn, loan.BorrowedAt, userRating));
			}
		}

		if (SelectedLoan != null && Loans.All(l => l.BookId != SelectedLoan.BookId))
		{
			SelectedLoan = null;
		}

		OnPropertyChanged(nameof(HasLoans));
		OnPropertyChanged(nameof(NoLoans));
	}

	private void ReturnSelectedBooks()
	{
		if (_memberId == null)
		{
			StatusMessage = "Log in as a member to manage loans.";
			return;
		}

		var selectedBookIds = Loans
			.Where(l => SelectedLoans.OfType<MemberLoanItemViewModel>().Any(s => s.BookId == l.BookId))
			.Select(l => l.BookId)
			.Distinct()
			.ToList();

		if (selectedBookIds.Count == 0 && SelectedLoan != null)
		{
			selectedBookIds.Add(SelectedLoan.BookId);
		}

		if (selectedBookIds.Count == 0)
		{
			StatusMessage = "Select books to return.";
			return;
		}

		var member = _libraryService.GetMembers().FirstOrDefault(m => m.Id == _memberId.Value);

		if (member == null)
		{
			StatusMessage = "Could not find the selected books for return.";
			return;
		}

		var returnedCount = 0;

		foreach (var bookId in selectedBookIds)
		{
			var book = _libraryService.GetBooks().FirstOrDefault(b => b.Id == bookId);
			if (book == null)
			{
				continue;
			}

			try
			{
				_libraryService.ReturnBook(book, member);
				returnedCount++;
			}
			catch (InvalidOperationException)
			{
				// Ignore individual failures and continue processing selected books.
			}
		}

		if (returnedCount == 0)
		{
			StatusMessage = "No books were returned.";
			return;
		}

		StatusMessage = returnedCount == 1 ? "1 book returned." : $"{returnedCount} books returned.";
		Refresh();
		StateChanged?.Invoke(this, EventArgs.Empty);
		_persistChanges?.Invoke();
	}

	private void RateSelectedBook()
	{
		if (_memberId == null)
		{
			StatusMessage = "Log in as a member to rate books.";
			return;
		}

		if (SelectedLoan == null)
		{
			StatusMessage = "Select a loan to rate.";
			return;
		}

		try
		{
			_libraryService.RateBook(_memberId.Value, SelectedLoan.BookId, SelectedRating);
			StatusMessage = $"Rated \"{SelectedLoan.BookTitle}\" {SelectedRating}/5.";
			Refresh();
			StateChanged?.Invoke(this, EventArgs.Empty);
			_persistChanges?.Invoke();
		}
		catch (ArgumentException ex)
		{
			StatusMessage = ex.Message;
		}
		catch (InvalidOperationException ex)
		{
			StatusMessage = ex.Message;
		}
	}
}
