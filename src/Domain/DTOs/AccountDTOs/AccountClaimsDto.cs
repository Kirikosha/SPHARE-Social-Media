namespace Domain.DTOs.AccountDTOs;
public class AccountClaimsDto
{
    public required string Username { get; set; }
    public required string UniqueNameIdentifier { get; set; }
    public required string Token { get; set; }
    public required string Role { get; set; }
    public required bool Blocked { get; set; }
}
