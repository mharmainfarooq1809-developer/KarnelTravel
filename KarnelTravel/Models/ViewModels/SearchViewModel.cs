namespace KarnelTravel.Models.ViewModels
{
    public class SearchViewModel
    {
        public string? Keyword { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public int TotalResults { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalResults / PageSize);

        public List<SearchResultItem> Results { get; set; } = new();
    }

    public class SearchResultItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Location { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal? StartingPrice { get; set; }
        public string DetailsUrl { get; set; } = "";
    }
}