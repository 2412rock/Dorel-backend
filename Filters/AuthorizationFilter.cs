using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace DorelAppBackend.Filters
{
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {

        private string GetEmailFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken != null)
            {
                // Extract token claims
                foreach (var claim in jsonToken.Claims)
                {
                    if (claim.Type == JwtRegisteredClaimNames.Sub)
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            StringValues authorizationHeaderValues;
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out authorizationHeaderValues))
            {
                string authorizationHeader = authorizationHeaderValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    string token = authorizationHeader.Substring("Bearer ".Length).Trim();
                    var email = GetEmailFromToken(token);
                    if(!String.IsNullOrEmpty(email))
                    {
                        context.HttpContext.Items["Email"] = email;
                        return;
                    }
                    
                }
            }
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
