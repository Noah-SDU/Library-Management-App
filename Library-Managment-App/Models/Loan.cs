using System;

namespace Library_Managment_App;

public class Loan
{
    public int Id { get; set; }  
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
    public DateTime?  ReturnedAt { get; set; }
    public bool IsActive => ReturnedAt == null;
}