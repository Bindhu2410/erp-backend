namespace ERP.API.Models
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public int TotalPages { get; set; }

        public PagedResponse()
        {
            Data = new List<T>();
        }

        public PagedResponse(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords, int filteredRecords)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            FilteredRecords = filteredRecords;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(filteredRecords / (double)pageSize) : 0;
        }
    }
}
