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
        private const string AccessTokenExpiresAtKey = "ACCESS_TOKEN_EXPIRES_AT";
        private readonly ApiClient _apiClient;
        private static readonly TimeSpan ExpirySafetyMargin = TimeSpan.FromSeconds(30);

        public AuthService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public bool IsAuthenticated => TryGetAccessToken(out var _);

        public string AccessToken
        {
            get
            {
                if (TryGetAccessToken(out var token))
                    return token!;
                throw new InvalidOperationException("Access token is not set.");
            }
        }

        public Task<string> GetAccessTokenAsync()
        {
            if (TryGetAccessToken(out var token))
                return Task.FromResult(token!);
            throw new InvalidOperationException("Access token is not set.");
        }


        public void SetAuthentication(string token, long expiresIn)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token));

            // Calcul de la date d'expiration
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

            // Stockage dans Preferences
            Preferences.Set(AccessTokenKey, token);
            Preferences.Set(AccessTokenExpiresAtKey, expiresAt.ToUnixTimeSeconds());

            // Appliquer au client API
            try
            {
                _apiClient.BearerToken = token;
            }
            catch
            {
                // ignore si ApiClient ne supporte pas BearerToken
            }
        }
        
        public void Logout()
        {
            Preferences.Remove(AccessTokenKey);
            Preferences.Remove(AccessTokenExpiresAtKey);
        }

        
         public async Task LogoutAsync()
         {
             Logout();
         }
        
         public bool IsLoginExpired() 
        {
            // Pas de token → expiré
            if (!Preferences.ContainsKey(AccessTokenKey))
             return true;

            // Pas de date → expiré
            if (!Preferences.ContainsKey(AccessTokenExpiresAtKey))
             return true;

            var expiresAtUnix = Preferences.Get(AccessTokenExpiresAtKey, 0L);
            if (expiresAtUnix <= 0)
             return true;

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix);

            // Ajouter une marge de sécurité
            return expiresAt <= DateTimeOffset.UtcNow.Add(ExpirySafetyMargin);
        }
         
        public Task<bool> IsLoggedInAsync()
        {
            var isLoggedIn = TryGetAccessToken(out _) && !IsLoginExpired();
            return Task.FromResult(isLoggedIn);
        }


        private bool TryGetAccessToken(out string? token)
        {
            token = Preferences.Get(AccessTokenKey, null);
            return !string.IsNullOrEmpty(token);
        }
    }
}
