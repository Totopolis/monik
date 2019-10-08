using System.Security.Claims;
using Nancy;

namespace Monik.Service
{
    public interface IUserIdentityProvider
    {
        ClaimsPrincipal GetUserIdentity(NancyContext ctx);
    }
}