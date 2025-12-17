namespace Gauniv.WebServer.Data;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } =  string.Empty;

    public ICollection<Game> Games { get; set; } = new List<Game>();   

}
