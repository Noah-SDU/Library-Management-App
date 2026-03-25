using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Library_Managment_App.ViewModels;

public abstract class CatalogViewModel : ViewModelBase
{
	protected readonly LibraryService LibraryService;

	public ObservableCollection<Book> Books { get; } = new();

	protected CatalogViewModel(LibraryService libraryService)
	{
		LibraryService = libraryService;
		RefreshBooks();
	}

	public virtual void RefreshBooks()
	{
		Books.Clear();
		foreach (var book in LibraryService.GetBooks())
		{
			Books.Add(book);
		}
	}
}

public class LibrarianCatalogViewModel : CatalogViewModel
{
	public class LibrarianCatalogBookItemViewModel : ViewModelBase
	{
		public Book Book { get; }
		public int BookId => Book.Id;
		public string Title => Book.Title;
		public string Author => Book.Author;
		public string ISBN => Book.ISBN;
		public int TotalCopies => Book.TotalCopies;
		public int AvailableCopies { get; }
		public string CopiesText => $"Copies: {TotalCopies} (Available: {AvailableCopies})";
		public double? AverageRating { get; }
		public string AverageRatingText => AverageRating == null
			? "Rating: not rated yet"
			: $"Rating: {AverageRating.Value:F1}/5.0";

		public LibrarianCatalogBookItemViewModel(Book book, int availableCopies, double? averageRating)
		{
			Book = book;
			AvailableCopies = availableCopies;
			AverageRating = averageRating;
		}
	}

	private readonly Action? _persistChanges;
	private string _newTitle = string.Empty;
	private string _newAuthor = string.Empty;
	private string _newIsbn = string.Empty;
	private string _newCopiesText = "1";
	private LibrarianCatalogBookItemViewModel? _selectedLibrarianBook;
	private string? _statusMessage;

	public event EventHandler? StateChanged;

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

	public IEnumerable<LibrarianCatalogBookItemViewModel> LibrarianCatalogBooks =>
		Books.Select(CreateLibrarianCatalogBookItem);

	public LibrarianCatalogBookItemViewModel? SelectedLibrarianBook
	{
		get => _selectedLibrarianBook;
		set
		{
			if (!SetProperty(ref _selectedLibrarianBook, value))
			{
				return;
			}

			if (value == null)
			{
				return;
			}

			NewTitle = value.Book.Title;
			NewAuthor = value.Book.Author;
			NewIsbn = value.Book.ISBN;
			NewCopiesText = value.Book.TotalCopies.ToString();
		}
	}

	public string? StatusMessage
	{
		get => _statusMessage;
		set => SetProperty(ref _statusMessage, value);
	}

	public ICommand AddBookCommand { get; }
	public ICommand EditSelectedBookCommand { get; }
	public ICommand DeleteSelectedBookCommand { get; }

	public LibrarianCatalogViewModel(LibraryService libraryService, Action? persistChanges = null)
		: base(libraryService)
	{
		_persistChanges = persistChanges;
		AddBookCommand = new RelayCommand(AddBook);
		EditSelectedBookCommand = new RelayCommand(EditSelectedBook);
		DeleteSelectedBookCommand = new RelayCommand(DeleteSelectedBook);
	}

	public override void RefreshBooks()
	{
		base.RefreshBooks();
		OnPropertyChanged(nameof(LibrarianCatalogBooks));
	}

	private void AddBook()
	{
		if (!TryGetValidatedBookInputs(out var title, out var author, out var isbn, out var copies))
		{
			return;
		}

		var newBook = new Book
		{
			Id = NextBookId(),
			Title = title,
			Author = author,
			ISBN = isbn,
			TotalCopies = copies
		};

		LibraryService.AddBook(newBook);
		RefreshBooks();
		SelectedLibrarianBook = LibrarianCatalogBooks.FirstOrDefault(b => b.BookId == newBook.Id);
		ClearEditor();
		StatusMessage = "Book added.";
		StateChanged?.Invoke(this, EventArgs.Empty);
		_persistChanges?.Invoke();
	}

	private void EditSelectedBook()
	{
		if (SelectedLibrarianBook == null)
		{
			StatusMessage = "Select a book to edit.";
			return;
		}

		if (!TryGetValidatedBookInputs(out var title, out var author, out var isbn, out var copies))
		{
			return;
		}

		var bookToEdit = SelectedLibrarianBook.Book;
		LibraryService.EditBook(
			bookToEdit,
			title,
			author,
			isbn,
			bookToEdit.Description,
			bookToEdit.Genre,
			bookToEdit.YearPublished,
			copies);

		RefreshBooks();
		SelectedLibrarianBook = LibrarianCatalogBooks.FirstOrDefault(b => b.BookId == bookToEdit.Id);
		StatusMessage = "Book updated.";
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
			LibraryService.DeleteBook(SelectedLibrarianBook.Book);
			RefreshBooks();
			SelectedLibrarianBook = null;
			StatusMessage = "Book deleted.";
			StateChanged?.Invoke(this, EventArgs.Empty);
			_persistChanges?.Invoke();
		}
		catch (InvalidOperationException ex)
		{
			StatusMessage = ex.Message;
		}
	}

	private bool TryGetValidatedBookInputs(out string title, out string author, out string isbn, out int copies)
	{
		title = NewTitle.Trim();
		author = NewAuthor.Trim();
		isbn = NewIsbn.Trim();

		if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
		{
			StatusMessage = "Enter at least title and author.";
			copies = 0;
			return false;
		}

		if (!int.TryParse(NewCopiesText, out copies) || copies < 1)
		{
			StatusMessage = "Copies must be a whole number of at least 1.";
			return false;
		}

		return true;
	}

	private void ClearEditor()
	{
		NewTitle = string.Empty;
		NewAuthor = string.Empty;
		NewIsbn = string.Empty;
		NewCopiesText = "1";
	}

	private int NextBookId()
	{
		var books = LibraryService.GetBooks();
		return books.Count == 0 ? 1 : books.Max(b => b.Id) + 1;
	}

	private LibrarianCatalogBookItemViewModel CreateLibrarianCatalogBookItem(Book book)
	{
		return new LibrarianCatalogBookItemViewModel(
			book,
			LibraryService.GetAvailableCopies(book.Id),
			LibraryService.GetAverageRating(book.Id));
	}
}

public class MemberCatalogViewModel : CatalogViewModel
{
	public class MemberCatalogBookItemViewModel : ViewModelBase
	{
		public Book Book { get; }
		public int BookId => Book.Id;
		public string Title => Book.Title;
		public string Author => Book.Author;
		public string ISBN => Book.ISBN;
		public int AvailableCopies { get; }
		public string AvailableCopiesText => $"Available copies: {AvailableCopies}";
		public double? AverageRating { get; }
		public string AverageRatingText => AverageRating == null
			? "Rating: not rated yet"
			: $"Rating: {AverageRating.Value:F1}/5.0";

		public MemberCatalogBookItemViewModel(Book book, int availableCopies, double? averageRating)
		{
			Book = book;
			AvailableCopies = availableCopies;
			AverageRating = averageRating;
		}
	}

	private readonly Action? _persistChanges;
	private int? _memberId;
	private string _memberUsername = string.Empty;
	private string _searchText = string.Empty;
	private MemberCatalogBookItemViewModel? _selectedMemberBook;
	private IList<object?> _selectedMemberBooks = new List<object?>();
	private string? _statusMessage;

	public event EventHandler? StateChanged;

	public IEnumerable<MemberCatalogBookItemViewModel> MemberCatalogBooks =>
		string.IsNullOrWhiteSpace(SearchText)
			? Books.Select(CreateMemberCatalogBookItem)
			: Books.Where(b =>
				ContainsIgnoreCase(b.Title, SearchText) ||
				ContainsIgnoreCase(b.Author, SearchText) ||
				ContainsIgnoreCase(b.ISBN, SearchText) ||
				ContainsIgnoreCase(b.Genre, SearchText))
			  .Select(CreateMemberCatalogBookItem);

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

	public MemberCatalogBookItemViewModel? SelectedMemberBook
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

	public ICommand BorrowSelectedBookCommand { get; }

	public MemberCatalogViewModel(LibraryService libraryService, Action? persistChanges = null)
		: base(libraryService)
	{
		_persistChanges = persistChanges;
		BorrowSelectedBookCommand = new RelayCommand(BorrowSelectedBook);
	}

	public void SetMemberContext(int? memberId, string? memberUsername = null)
	{
		var members = LibraryService.GetMembers();
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

	public override void RefreshBooks()
	{
		base.RefreshBooks();
		OnPropertyChanged(nameof(MemberCatalogBooks));
	}

	private void BorrowSelectedBook()
	{
		if (_memberId == null)
		{
			StatusMessage = "Log in as a member to borrow books.";
			return;
		}

		var booksToBorrow = SelectedMemberBooks
			.OfType<MemberCatalogBookItemViewModel>()
			.Select(i => i.Book)
			.GroupBy(b => b.Id)
			.Select(g => g.First())
			.ToList();
		if (booksToBorrow.Count == 0 && SelectedMemberBook != null)
		{
			booksToBorrow.Add(SelectedMemberBook.Book);
		}

		if (booksToBorrow.Count == 0)
		{
			StatusMessage = "Select books to borrow.";
			return;
		}

		var member = LibraryService.GetMembers().FirstOrDefault(m => m.Id == _memberId.Value);
		if (member == null && !string.IsNullOrWhiteSpace(_memberUsername))
		{
			member = LibraryService.GetMembers().FirstOrDefault(
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
				LibraryService.BorrowBook(book, member);
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
		RefreshBooks();
		StateChanged?.Invoke(this, EventArgs.Empty);
		_persistChanges?.Invoke();
	}

	private static bool ContainsIgnoreCase(string? value, string query)
	{
		return !string.IsNullOrWhiteSpace(value) &&
			   value.Contains(query, StringComparison.OrdinalIgnoreCase);
	}

	private MemberCatalogBookItemViewModel CreateMemberCatalogBookItem(Book book)
	{
		return new MemberCatalogBookItemViewModel(
			book,
			LibraryService.GetAvailableCopies(book.Id),
			LibraryService.GetAverageRating(book.Id));
	}
}
