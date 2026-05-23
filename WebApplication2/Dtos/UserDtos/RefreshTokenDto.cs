namespace WebApplication2.Dtos.UserDtos;

public class RefreshTokenDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}