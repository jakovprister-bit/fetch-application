using FetchApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FetchApplication.Data { 
    public class ApplicationDbContext : DbContext
    {
        public DbSet<SearchResult> SearchResults { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}