using Newtonsoft.Json.Linq;
using FetchApplication.Models;

namespace FetchApplication.Services
{
    public class GoogleSearchService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private const int RESULTS_PER_PAGE = 10;
        private const int MAX_PARALLEL_REQUESTS = 3;

        public GoogleSearchService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<List<SearchResult>> SearchAsync(string query, int totalResults = 100)
        {
            var apiKey = _config["GoogleCustomSearch:ApiKey"];
            var cxId = _config["GoogleCustomSearch:CxId"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(cxId))
            {
                throw new InvalidOperationException("Google Custom Search API key or CX ID are not configured");
            }

            var results = new List<SearchResult>();
            var numPages = (int)Math.Ceiling(totalResults / (double)RESULTS_PER_PAGE);

            try
            {
                var tasks = new List<Task<List<SearchResult>>>();

                for (int page = 0; page < numPages; page++)
                {
                    int start = (page * RESULTS_PER_PAGE) + 1;
                    var task = FetchPageAsync(query, apiKey, cxId, start);
                    tasks.Add(task);
                }

                var pageResults = await ProcessInBatches(tasks, MAX_PARALLEL_REQUESTS);

                foreach (var pageResult in pageResults)
                {
                    results.AddRange(pageResult);
                }

                if (results.Count == 0)
                {
                    throw new InvalidOperationException($"No search results found for query: '{query}'. Check API key, CX ID, and query parameters.");
                }

                return results;
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new InvalidOperationException("Google Custom Search API rate limit exceeded. Try again tomorrow at 9:00 AM CET or upgrade your plan.", httpEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Check if Google Custom Search API key or CX ID are configured", ex);
            }
        }

        private static async Task<List<List<SearchResult>>> ProcessInBatches(
            List<Task<List<SearchResult>>> tasks,
            int maxConcurrency)
        {
            var results = new List<List<SearchResult>>();
            var runningTasks = new List<Task<List<SearchResult>>>();

            foreach (var task in tasks)
            {
                runningTasks.Add(task);

                if (runningTasks.Count >= maxConcurrency)
                {
                    var completedTask = await Task.WhenAny(runningTasks);
                    results.Add(await completedTask);
                    runningTasks.Remove(completedTask);
                }
            }

            var remainingResults = await Task.WhenAll(runningTasks);
            results.AddRange(remainingResults);

            return results;
        }

        private async Task<List<SearchResult>> FetchPageAsync(
            string query,
            string apiKey,
            string cxId,
            int start)
        {
            var pageResults = new List<SearchResult>();

            try
            {
                var url = $"https://www.googleapis.com/customsearch/v1" +
                    $"?key={apiKey}" +
                    $"&cx={cxId}" +
                    $"&q={Uri.EscapeDataString(query)}" +
                    $"&num={RESULTS_PER_PAGE}" +
                    $"&start={start}";

                var response = await RetryPolicy(async () => await _httpClient.GetAsync(url));
                response.EnsureSuccessStatusCode();

                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                var items = (JArray)json["items"];

                if (items == null || items.Count == 0)
                {
                    return pageResults; // Empty page is OK
                }

                foreach (JObject item in items)
                {
                    pageResults.Add(new SearchResult
                    {
                        Title = item["title"]?.ToString() ?? "",
                        Url = item["link"]?.ToString() ?? "",
                        Snippet = item["snippet"]?.ToString() ?? "",
                        SearchTerm = query,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                return pageResults;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<HttpResponseMessage> RetryPolicy(
            Func<Task<HttpResponseMessage>> action,
            int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var response = await action();
                if (response.IsSuccessStatusCode) return response;
                if ((int)response.StatusCode == 429)
                {
                    await Task.Delay(1000 * (i + 1));
                    continue;
                }

                return response;
            }
            return await action();
        }
    }
}
