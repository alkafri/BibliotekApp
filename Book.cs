using System.ComponentModel.DataAnnotations;

public class Book
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Author { get; set; }
    [Required]
    public string ISBN { get; set; }

    public bool IsAvailable { get; set; } = true;
}
