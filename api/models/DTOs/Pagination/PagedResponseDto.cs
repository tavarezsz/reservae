namespace Reservae.Models.DTOs;

public class PagedResponseDto<T>
{
    public IEnumerable<T> Items {get;set;} = [];
    public int TotalCount {get;set;}
}