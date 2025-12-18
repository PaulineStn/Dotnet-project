namespace Gauniv.WebServer.Dtos
{
    public record GameDto(
        int Id,
        string Name,
        decimal Price,
        string CurrentVersion,
        string Description,
        IEnumerable<CategoryDto> Categories
    );

    public record GameDetailDto(
        int Id,
        string Name,
        string Description,
        decimal Price,
        string CurrentVersion,
        IEnumerable<CategoryDto> Categories
    );

    public record CreateGameDto(
        string Name,
        string Description,
        decimal Price,
        string CurrentVersion,
        IEnumerable<int> CategoryIds
    );

    public record UpdateGameDto(
        string Name,
        string Description,
        decimal Price,
        string CurrentVersion,
        IEnumerable<int> CategoryIds
    );

    public record PurchasedGameDto(
        string Message,
        int GameId
    );
}