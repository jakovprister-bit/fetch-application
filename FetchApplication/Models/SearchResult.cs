namespace FetchApplication.Models
{
    public class SearchResult
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Snippet { get; set; }
        public string SearchTerm { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}