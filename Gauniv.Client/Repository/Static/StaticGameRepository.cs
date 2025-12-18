using Gauniv.Network;

namespace Gauniv.Client.Repository.Static;

public class StaticGameRepository : IGameRepository
{
    private static readonly List<GameDto> _games =
    [
        new GameDto
        {
            Id = 1,
            Name = "Chess",
            Description = "Jeu d'échecs"
        },
        new GameDto
        {
            Id = 2,
            Name = "Poker",
            Description = "Jeu de cartes",
        }
    ];

    public Task<IReadOnlyList<GameDto>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<GameDto>>(_games);
    }
}
