using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Implementation;
using DorelAppBackend.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;

namespace DorelAppBackend.Filters
{
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            StringValues authorizationHeaderValues;
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out authorizationHeaderValues))
            {
                string authorizationHeader = authorizationHeaderValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    string token = authorizationHeader.Substring("Bearer ".Length).Trim();
                    var email = TokenHelper.GetEmailFromToken(token);
                    if (TokenHelper.IsTokenExpired(token))
                    {
                        context.Result = new StatusCodeResult(403);
                        return;
                    }
                    if(!String.IsNullOrEmpty(email))
                    {
                        context.HttpContext.Items["Email"] = email;
                        return;
                    }
                    
                }
            }
            context.Result = new StatusCodeResult(401); ;
            return;
        }
    }
}
