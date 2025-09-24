namespace ParcelManagement.Core.Model.Helper
{
    public class FilterPaginationRequest<TEnumCol> where TEnumCol : Enum
    {
        public string? SearchKeyword { get; set; }
        public int? Page { get; set; }
        public int? Take { get; set; }
        public TEnumCol? SortableColumn { get; set; }
        public bool IsAscending { get; set; } = true;
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}