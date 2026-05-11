using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Client;

namespace PasswordManager.UI.Services;

public static class UserIdentityHelper
{
    public static string GetStableUserId(AuthenticationResult result)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.IdToken);

        var oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
        if (!string.IsNullOrWhiteSpace(oid))
            return oid;

        var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (!string.IsNullOrWhiteSpace(sub))
            return sub;

        throw new Exception("No stable user id found in token.");
    }
}
