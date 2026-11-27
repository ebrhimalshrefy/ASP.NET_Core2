using Week5Api.Models;

namespace Week5Api.Services
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, IList<string>? roles = null);
        DateTime GetTokenExpiration();
    }
}
