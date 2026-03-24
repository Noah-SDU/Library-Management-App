using System;

namespace Library_Managment_App;

public class Rating
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public int RatingStars { get; set; }
    public DateTime RatedAt { get; set; } = DateTime.UtcNow;
}