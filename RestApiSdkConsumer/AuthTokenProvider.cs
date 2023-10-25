using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace RestApiSdkConsumer;

public class AuthTokenProvider
{
    private readonly HttpClient _httpClient;
    private string _cachedToken = string.Empty;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public AuthTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken))
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_cachedToken);
            var expiryText = jwt.Claims.Single(claim => claim.Type == "exp").Value;
            var expiryDateTime = UnixTimeStampToDateTime(int.Parse(expiryText));

            if (expiryDateTime > DateTime.UtcNow)
            {
                return _cachedToken;
            }
        }

        await Lock.WaitAsync();
        var response = await _httpClient.PostAsJsonAsync("https://localhost:5003/token", new
        {
            userid = "400a4667-ef99-4ca1-a0e9-6fc08d93a27c",
            email = "zarrko1@zarrko1.com",
            customClaims = new Dictionary<string, object>
            {
                {
                    "admin", true
                },
                {
                    "trusted_member", true
                }
            }
        });

        var newToken = await response.Content.ReadAsStringAsync();
        _cachedToken = newToken;
        Lock.Release();
        return newToken;
    }

    public static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
    {
        var datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        datetime = datetime.AddSeconds(unixTimeStamp).ToLocalTime();
        return datetime;
    }
}