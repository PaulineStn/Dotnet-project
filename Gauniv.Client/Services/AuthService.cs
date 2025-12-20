using Gauniv.Network;


using Newtonsoft.Json;
using Microsoft.Maui.Storage;

// public class AuthService : IAuthService
// {
//     private const string TokenKey = "access_token_response";
//
//     public async Task<bool> IsLoggedInAsync()
//     {
//         var tokenJson = Preferences.Get(TokenKey, null);
//         if (string.IsNullOrWhiteSpace(tokenJson))
//             return false;
//
//         var token = JsonConvert.DeserializeObject<AccessTokenResponse>(tokenJson);
//         if (token == null)
//             return false;
//
//         // vérifier si accessToken expiré
//         var expiryTime = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
//         return expiryTime > DateTimeOffset.UtcNow;
//     }
//
//     public async Task<AccessTokenResponse?> GetAccessTokenAsync()
//     {
//         var tokenJson = Preferences.Get(TokenKey, null);
//         if (string.IsNullOrWhiteSpace(tokenJson))
//             return null;
//
//         return JsonConvert.DeserializeObject<AccessTokenResponse>(tokenJson);
//     }
//
//     public async Task LoginAsync(string username, string password)
//     {
//         // Appel API pour récupérer AccessTokenResponse
//         var token = await CallLoginApi(username, password);
//
//         // Stocker localement
//         var json = JsonConvert.SerializeObject(token);
//         Preferences.Set(TokenKey, json);
//     }
//
//     public async Task LogoutAsync()
//     {
//         Preferences.Remove(TokenKey);
//     }
//
//     public async Task RefreshTokenIfNeededAsync()
//     {
//         var token = await GetAccessTokenAsync();
//         if (token == null)
//             return;
//
//         // Simple check pour rafraîchir si expiration < 1 min
//         var expiryTime = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
//         if ((expiryTime - DateTimeOffset.UtcNow).TotalSeconds < 60)
//         {
//             var newToken = await CallRefreshTokenApi(token.RefreshToken);
//             Preferences.Set(TokenKey, JsonConvert.SerializeObject(newToken));
//         }
//     }
//
//     // 👇 Implémentation fictive des appels API
//     private async Task<AccessTokenResponse> CallLoginApi(string username, string password)
//     {
//         // appeler ton API Auth et retourner AccessTokenResponse
//         throw new NotImplementedException();
//     }
//
//     private async Task<AccessTokenResponse> CallRefreshTokenApi(string refreshToken)
//     {
//         // appeler ton API refresh token et retourner AccessTokenResponse
//         throw new NotImplementedException();
//     }
// }

using System;

namespace Gauniv.Client.Services
{
    public class AuthService : IAuthService
    {
        private const string AccessTokenKey = "ACCESS_TOKEN";

        public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

        public string AccessToken
        {
            get
            {
                var token = Preferences.Get(AccessTokenKey, null);
                if (token == null)
                    throw new InvalidOperationException("Access token is not set.");
                return token;
            }
        }

        public Task<string> GetAccessTokenAsync()
        {
            var token = Preferences.Get(AccessTokenKey, null);
            if (token == null)
                throw new InvalidOperationException("Access token is not set.");
            return Task.FromResult(token);
        }


        public void SetAuthentication(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            Preferences.Set(AccessTokenKey, token);
        }

        public void Logout()
        {
            Preferences.Remove(AccessTokenKey);
        }
        
        public async Task<bool> IsLoggedInAsync()
        {
            try
            {
                var token = await GetAccessTokenAsync();
                return !string.IsNullOrEmpty(token);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }
}
