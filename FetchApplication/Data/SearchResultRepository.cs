using Microsoft.EntityFrameworkCore;
using FetchApplication.Models;
using FetchApplication.Data;

namespace FetchApplication.Services
{
    public class SearchResultRepository
    {
        private readonly ApplicationDbContext _context;

        public SearchResultRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveResultsAsync(List<SearchResult> results)
        {
            if (results.Count > 0)
            {
                _context.SearchResults.AddRange(results);
                await _context.SaveChangesAsync();
            }
        }
    }
}
