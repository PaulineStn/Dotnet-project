using Gauniv.Network;

namespace Gauniv.Client.Repository.Api;

public class ApiGameRepository : IGameRepository
{
    private readonly ApiClient _api;

    public ApiGameRepository(ApiClient api)
    {
        _api = api;
    }

    public async Task<IReadOnlyList<GameDto>> GetAllAsync()
    {
        // await _api.GetMyPurchasesIdsAsync(
        //     false,
        //     false,
        //     new LoginRequest
        //     {
        //         Email = "d",
        //         Password = "d"
        //     }
        // ).Result
        //
        //     });
        return (IReadOnlyList<GameDto>)await _api.GetAllAsync(null, null, null, null, null, null);
        
    }
}
