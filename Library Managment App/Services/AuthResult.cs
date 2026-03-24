namespace Library_Managment_App;

public record AuthResult(bool Success, Role? Role, int? MemberId, string? ErrorMessage);
