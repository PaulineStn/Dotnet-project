using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gauniv.WebServer.Data;

public class Purchase
{
    public string UserId { get; set; } =  string.Empty;
    public User User { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}
