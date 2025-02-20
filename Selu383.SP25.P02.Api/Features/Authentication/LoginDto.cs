namespace Selu383.SP25.P02.Api.Features.Authentication
{
    public class LoginDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
