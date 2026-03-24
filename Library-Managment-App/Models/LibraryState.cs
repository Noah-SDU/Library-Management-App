using System.Collections.Generic;

namespace Library_Managment_App;

public class LibraryState
{
    public List<Book> Books { get; set; } = new();
    public List<Member> Members { get; set; } = new();
    public List<Loan> Loans { get; set; } = new();
    public List<Rating> Ratings { get; set; } = new();
}