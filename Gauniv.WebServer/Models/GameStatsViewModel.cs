namespace Gauniv.WebServer.Models;

public class GameStatsViewModel
{
    public int TotalGames { get; set; }

    public List<CategoryCount> GamesByCategory { get; set; } = new();

    public decimal AverageGamesPerAccount { get; set; }

    public class CategoryCount
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int GameCount { get; set; }
    }
}
