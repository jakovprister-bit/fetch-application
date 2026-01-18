// Controllers/SearchController.cs
using FetchApplication.Models;
using FetchApplication.Services;
using Microsoft.AspNetCore.Mvc;

[Route("")]
public class SearchController : Controller
{
    private readonly GoogleSearchService _googleService;
    private readonly SearchResultRepository _repository;

    public SearchController(GoogleSearchService googleService, SearchResultRepository repository)
    {
        _googleService = googleService;
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest(new { success = false, message = "Search query cannot be empty" });

            var results = await _googleService.SearchAsync(request.Query);

            if (results.Count == 0)
                return Ok(new { success = true, data = new List<SearchResult>() });

            await _repository.SaveResultsAsync(results);

            return Ok(new { success = true, data = results });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
