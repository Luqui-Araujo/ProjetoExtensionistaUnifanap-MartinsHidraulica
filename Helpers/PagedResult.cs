namespace MartinsHidraulica.Helpers;

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages  { get; set; }
}