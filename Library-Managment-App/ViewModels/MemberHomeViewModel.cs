using System.Linq;

namespace Library_Managment_App.ViewModels;

public class MemberHomeViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;
    private int? _memberId;
    private int _currentLoansCount;
    private int _booksRatedCount;

    public CatalogViewModel Catalog { get; }
    public MyLoansViewModel MyLoans { get; }

    public int CurrentLoansCount
    {
        get => _currentLoansCount;
        private set => SetProperty(ref _currentLoansCount, value);
    }

    public int BooksRatedCount
    {
        get => _booksRatedCount;
        private set => SetProperty(ref _booksRatedCount, value);
    }

    public MemberHomeViewModel(CatalogViewModel catalog, MyLoansViewModel myLoans, LibraryService libraryService)
    {
        Catalog = catalog;
        MyLoans = myLoans;
        _libraryService = libraryService;
    }

    public void SetMemberContext(int? memberId, string? memberUsername = null)
    {
        _memberId = memberId;
        Catalog.SetMemberContext(memberId, memberUsername);
        MyLoans.SetMemberContext(memberId, memberUsername);
        RefreshDashboardStats();
    }

    public void RefreshDashboardStats()
    {
        if (_memberId == null)
        {
            CurrentLoansCount = 0;
            BooksRatedCount = 0;
            return;
        }

        CurrentLoansCount = _libraryService.GetActiveLoans().Count(l => l.MemberId == _memberId.Value);
        BooksRatedCount = _libraryService.GetRatedBooksCount(_memberId.Value);
    }
}
