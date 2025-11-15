using Google.Apis.Auth;

namespace Infrastructure.Security;

public sealed class GoogleAuthSettings
{
    public string ClientId { get; set; } = default!;
}

public sealed class GoogleUserInfo
{
    public string Sub { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
}

public sealed class GoogleTokenValidator
{
    private readonly GoogleAuthSettings _settings;

    public GoogleTokenValidator(GoogleAuthSettings settings)
    {
        _settings = settings;
    }

    public async Task<GoogleUserInfo> ValidateAsync(string idToken)
    {
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _settings.ClientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

        return new GoogleUserInfo
        {
            Sub = payload.Subject,
            Email = payload.Email,
            Name = payload.Name ?? payload.Email
        };
    }
}
