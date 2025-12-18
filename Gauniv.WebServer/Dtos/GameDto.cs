namespace Gauniv.WebServer.Dtos
{
    public record GameDto(
        int Id,
        string Name,
        decimal Price,
        string CurrentVersion,
        IEnumerable<string> Categories
    );

    public record GameDetailDto(
        int Id,
        string Name,
        string Description,
        decimal Price,
        string CurrentVersion,
        IEnumerable<string> Categories
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
}