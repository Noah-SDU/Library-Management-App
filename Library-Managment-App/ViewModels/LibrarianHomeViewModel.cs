namespace Library_Managment_App.ViewModels;

public class LibrarianHomeViewModel : ViewModelBase
{
    public LibrarianCatalogViewModel Catalog { get; }
    public ActiveLoansViewModel ActiveLoans { get; }

    public LibrarianHomeViewModel(LibrarianCatalogViewModel catalog, ActiveLoansViewModel activeLoans)
    {
        Catalog = catalog;
        ActiveLoans = activeLoans;
    }
}
