using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Library_Managment_App.ViewModels;

public class CatalogViewModel : ViewModelBase
{
	private readonly LibraryService _libraryService;
	private readonly Action? _persistChanges;
	private int? _memberId;
	private string _memberUsername = string.Empty;
	private string _searchText = string.Empty;
	private string _newTitle = string.Empty;
	private string _newAuthor = string.Empty;
	private string _newIsbn = string.Empty;
	private string _newCopiesText = "1";
	private Book? _selectedLibrarianBook;
	private Book? _selectedMemberBook;
	private IList<object?> _selectedMemberBooks = new List<object?>();
	private string? _statusMessage;

	public event EventHandler? StateChanged;

	public ObservableCollection<Book> Books { get; } = new();

	public IEnumerable<Book> MemberCatalogBooks =>
		string.IsNullOrWhiteSpace(SearchText)
			? Books
			: Books.Where(b =>
				ContainsIgnoreCase(b.Title, SearchText) ||
				ContainsIgnoreCase(b.Author, SearchText) ||
				ContainsIgnoreCase(b.ISBN, SearchText) ||
				ContainsIgnoreCase(b.Genre, SearchText));

	public string SearchText
	{
		get => _searchText;
		set
		{
			if (!SetProperty(ref _searchText, value))
			{
				return;
			}

			OnPropertyChanged(nameof(MemberCatalogBooks));
		}
	}

	public string NewTitle
	{
		get => _newTitle;
		set => SetProperty(ref _newTitle, value);
	}

	public string NewAuthor
	{
		get => _newAuthor;
		set => SetProperty(ref _newAuthor, value);
	}

	public string NewIsbn
	{
		get => _newIsbn;
		set => SetProperty(ref _newIsbn, value);
	}

	public string NewCopiesText
	{
		get => _newCopiesText;
		set => SetProperty(ref _newCopiesText, value);
	}

	public Book? SelectedLibrarianBook
	{
		get => _selectedLibrarianBook;
		set => SetProperty(ref _selectedLibrarianBook, value);
	}

	public Book? SelectedMemberBook
	{
		get => _selectedMemberBook;
		set => SetProperty(ref _selectedMemberBook, value);
	}

	public IList<object?> SelectedMemberBooks
	{
		get => _selectedMemberBooks;
		set => SetProperty(ref _selectedMemberBooks, value);
	}

	public string? StatusMessage
	{
		get => _statusMessage;
		set => SetProperty(ref _statusMessage, value);
	}

	public ICommand AddBookCommand { get; }
	public ICommand DeleteSelectedBookCommand { get; }
	public ICommand BorrowSelectedBookCommand { get; }

	public CatalogViewModel(LibraryService libraryService, Action? persistChanges = null)
	{
		_libraryService = libraryService;
		_persistChanges = persistChanges;
		AddBookCommand = new RelayCommand(AddBook);
		DeleteSelectedBookCommand = new RelayCommand(DeleteSelectedBook);
		BorrowSelectedBookCommand = new RelayCommand(BorrowSelectedBook);

		RefreshBooks();
	}

	public void SetMemberContext(int? memberId, string? memberUsername = null)
	{
		var members = _libraryService.GetMembers();
		_memberId = memberId;
		_memberUsername = memberUsername?.Trim() ?? string.Empty;

		if (_memberId != null && members.Any(m => m.Id == _memberId.Value))
		{
			return;
		}

		if (!string.IsNullOrWhiteSpace(_memberUsername))
		{
			var byUsername = members.FirstOrDefault(
				m => m.Username.Equals(_memberUsername, StringComparison.OrdinalIgnoreCase));
			if (byUsername != null)
			{
				_memberId = byUsername.Id;
				return;
			}
		}

		_memberId = members.FirstOrDefault()?.Id;
	}

	public void RefreshBooks()
	{
		Books.Clear();
		foreach (var book in _libraryService.GetBooks())
		{
			Books.Add(book);
		}

		OnPropertyChanged(nameof(MemberCatalogBooks));
	}

	private void AddBook()
	{
		if (string.IsNullOrWhiteSpace(NewTitle) || string.IsNullOrWhiteSpace(NewAuthor))
		{
			StatusMessage = "Enter at least title and author.";
			return;
		}

		if (!int.TryParse(NewCopiesText, out var copies) || copies < 1)
		{
			StatusMessage = "Copies must be a whole number of at least 1.";
			return;
		}

		var newBook = new Book
		{
			Id = NextBookId(),
			Title = NewTitle.Trim(),
			Author = NewAuthor.Trim(),
			ISBN = NewIsbn.Trim(),
			TotalCopies = copies
		};

		_libraryService.AddBook(newBook);
		Books.Add(newBook);
		OnPropertyChanged(nameof(MemberCatalogBooks));

		NewTitle = string.Empty;
		NewAuthor = string.Empty;
		NewIsbn = string.Empty;
		NewCopiesText = "1";
		StatusMessage = "Book added.";
		StateChanged?.Invoke(this, EventArgs.Empty);
		_persistChanges?.Invoke();
	}

	private void DeleteSelectedBook()
	{
		if (SelectedLibrarianBook == null)
		{
			StatusMessage = "Select a book to delete.";
			return;
		}

		try
		{
			_libraryService.DeleteBook(SelectedLibrarianBook);
			Books.Remove(SelectedLibrarianBook);

			if (ReferenceEquals(SelectedMemberBook, SelectedLibrarianBook))
			{
				SelectedMemberBook = null;
			}

			SelectedLibrarianBook = null;
			OnPropertyChanged(nameof(MemberCatalogBooks));
			StatusMessage = "Book deleted.";
			StateChanged?.Invoke(this, EventArgs.Empty);
			_persistChanges?.Invoke();
		}
		catch (InvalidOperationException ex)
		{
			StatusMessage = ex.Message;
		}
	}

	private void BorrowSelectedBook()
	{
		if (_memberId == null)
		{
			StatusMessage = "Log in as a member to borrow books.";
			return;
		}

		var booksToBorrow = SelectedMemberBooks
			.OfType<Book>()
			.GroupBy(b => b.Id)
			.Select(g => g.First())
			.ToList();
		if (booksToBorrow.Count == 0 && SelectedMemberBook != null)
		{
			booksToBorrow.Add(SelectedMemberBook);
		}

		if (booksToBorrow.Count == 0)
		{
			StatusMessage = "Select books to borrow.";
			return;
		}

		var member = _libraryService.GetMembers().FirstOrDefault(m => m.Id == _memberId.Value);
		if (member == null && !string.IsNullOrWhiteSpace(_memberUsername))
		{
			member = _libraryService.GetMembers().FirstOrDefault(
				m => m.Username.Equals(_memberUsername, StringComparison.OrdinalIgnoreCase));
		}

		if (member == null)
		{
			StatusMessage = "Member account was not found.";
			return;
		}

		var borrowedCount = 0;
		var lastError = string.Empty;

		foreach (var book in booksToBorrow)
		{
			try
			{
				_libraryService.BorrowBook(book, member);
				borrowedCount++;
			}
			catch (InvalidOperationException ex)
			{
				lastError = ex.Message;
			}
		}

		if (borrowedCount == 0)
		{
			StatusMessage = string.IsNullOrWhiteSpace(lastError)
				? "No books were borrowed."
				: lastError;
			return;
		}

		StatusMessage = borrowedCount == 1 ? "1 book borrowed." : $"{borrowedCount} books borrowed.";
		StateChanged?.Invoke(this, EventArgs.Empty);
		_persistChanges?.Invoke();
	}

	private static bool ContainsIgnoreCase(string? value, string query)
	{
		return !string.IsNullOrWhiteSpace(value) &&
			   value.Contains(query, StringComparison.OrdinalIgnoreCase);
	}

	private int NextBookId()
	{
		var books = _libraryService.GetBooks();
		return books.Count == 0 ? 1 : books.Max(b => b.Id) + 1;
	}
}
