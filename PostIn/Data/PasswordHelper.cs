using Microsoft.AspNetCore.Identity;

namespace PostIn.Data;

public static class PasswordHelper
{
    // Utilizza l'hasher ufficiale di ASP.NET Core Identity
    private static readonly PasswordHasher<string> _hasher = new();

    // Genera l'hash crittografico sicuro con Salt automatico.
    public static string HashPassword(string password)
    {
        return _hasher.HashPassword("PostInUser", password);
    }

    // Verifica la corrispondenza tra password in chiaro e hash memorizzato.
    public static bool VerifyPassword(string providedPassword, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
            return false;

        var result = _hasher.VerifyHashedPassword("PostInUser", hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success;
    }
}