namespace Library_Managment_App.ViewModels;

public class LibrarianHomeViewModel : ViewModelBase
{
    public CatalogViewModel Catalog { get; }
    public ActiveLoansViewModel ActiveLoans { get; }

    public LibrarianHomeViewModel(CatalogViewModel catalog, ActiveLoansViewModel activeLoans)
    {
        Catalog = catalog;
        ActiveLoans = activeLoans;
    }
}
