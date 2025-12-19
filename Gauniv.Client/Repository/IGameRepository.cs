using Gauniv.Network;

namespace Gauniv.Client.Repository;

public interface IGameRepository
{
    Task<IReadOnlyList<GameDto>> GetAllAsync();
}
