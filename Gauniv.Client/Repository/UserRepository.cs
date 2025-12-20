using Gauniv.Client.Services;


using System.Net.Http.Headers;
using System.Net.Http.Json;
using Gauniv.Network;

namespace Gauniv.Client.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        public UserRepository(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<IReadOnlyCollection<int>> GetMyPurchasedGameIdsAsync()
        {
            var token = await _authService.GetAccessTokenAsync();
            if (token == null)
                return Array.Empty<int>();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(token.TokenType ?? "Bearer", token.AccessToken);

            var response = await _httpClient.GetAsync("/api/users/me/purchases");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UserPurchasedGamesIdsDto>();

            return (IReadOnlyCollection<int>) (result?.GameIds ?? Array.Empty<int>());
        }

        public async Task BuyGameAsync(int gameId)
        {
            var token = await _authService.GetAccessTokenAsync();
            if (token == null)
                throw new InvalidOperationException("Utilisateur non connecté");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(token.TokenType ?? "Bearer", token.AccessToken);

            var response = await _httpClient.PostAsync($"/api/games/{gameId}/buy", null);

            response.EnsureSuccessStatusCode();
        }
    }
}
