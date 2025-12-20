namespace RenzoAgostini.Shared.DTOs;

public class AuthResponseDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Token { get; set; }
}
