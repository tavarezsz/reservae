namespace Reservae.Modesl.DTOs;

public class PagedRequestDto<T>{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}