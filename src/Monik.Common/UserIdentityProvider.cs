using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Monik.Common;
using Nancy;

namespace Monik.Service
{
    public class UserIdentityProvider : IUserIdentityProvider
    {
        private const string TokenPrefix = "Bearer ";
        private readonly IMonikServiceSettings _settings;
        private readonly IMonik _monik;

        public UserIdentityProvider(IMonikServiceSettings settings, IMonik monik)
        {
            _settings = settings;
            _monik = monik;
        }

        public ClaimsPrincipal GetUserIdentity(NancyContext ctx)
        {
            try
            {
                var authorization = ctx.Request.Headers.Authorization;

                if (string.IsNullOrWhiteSpace(authorization))
                    return null;

                if (!authorization.StartsWith(TokenPrefix, StringComparison.OrdinalIgnoreCase))
                    return null;

                var jwtToken = authorization.Substring(TokenPrefix.Length);

                var handler = new JwtSecurityTokenHandler {SetDefaultTimesOnTokenCreation = false};

                var principal = handler.ValidateToken(
                    jwtToken,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = new[]
                        {
                            new SymmetricSecurityKey(Convert.FromBase64String(_settings.AuthSecretKey)),
                        },
                    },
                    out _);

                return principal;
            }
            catch (Exception ex)
            {
                _monik.SecurityWarning($"Auth exception: {ex}");
                return null;
            }
        }
    } //end of class
}