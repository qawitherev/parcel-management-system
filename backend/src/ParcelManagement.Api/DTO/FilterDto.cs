namespace ParcelManagement.Api.DTO
{
    public class BaseFilterDto
    {
        public string? SearchKeyword { get; set; }
        public string? Status { get; set; }
        public string? Column { get; set; }
        public int? Page { get; set; }
        public int? Take { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}