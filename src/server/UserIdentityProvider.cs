using Jose;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using Monik.Common;

namespace Monik.Service
{
    public class UserIdentity : IUserIdentity
    {
        public string UserName { get; set; }
        public IEnumerable<string> Claims { get; set; }
    }

    public class UserIdentityProvider: IUserIdentityProvider
    {
        private const string BearerDeclaration = "Bearer ";
        private readonly IMonikServiceSettings _settings;
        private readonly IMonik _monik;

        public UserIdentityProvider(IMonikServiceSettings settings, IMonik monik)
        {
            _settings = settings;
            _monik = monik;
        }

        public IUserIdentity GetUserIdentity(NancyContext ctx)
        {
            try
            {
                var authorizationHeader = ctx.Request.Headers.Authorization;
                var jwt = authorizationHeader.Substring(BearerDeclaration.Length);

                var authToken = JWT.Decode<AuthToken>(jwt, _settings.AuthSecretKey, JwsAlgorithm.HS256);

                var tokenExpires = DateTimeOffset.FromUnixTimeSeconds(authToken.exp).UtcDateTime;

                if (tokenExpires > DateTime.UtcNow)
                {
                    return new UserIdentity
                    {
                        UserName = authToken.sub
                    };
                }

                return null;


            }
            catch (Exception ex)
            {
                _monik.SecurityWarning($"Auth exception: {ex}");
                return null;
            }
        }

    }//end of class
}
