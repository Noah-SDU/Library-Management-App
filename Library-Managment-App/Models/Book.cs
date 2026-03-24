namespace Library_Managment_App;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string ISBN { get; set; } = "";
    public string Description { get; set; } = "";
    public string Genre { get; set; } = "";
    public int? YearPublished { get; set; }
    public int TotalCopies { get; set; } = 1;
}