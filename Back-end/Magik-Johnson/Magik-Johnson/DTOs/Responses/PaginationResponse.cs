using Magik_Johnson.Models;

namespace DTOs.Responses;

public class PaginationResponse<T>
{
    public required T[] Cards { get; set; }

    public required PaginationInfo Pagination { get; set; }
}
