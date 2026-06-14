using GpuShare.Frontend.Services.Interfaces;

public class GpuRankingService : IGpuRankingService
{
    private readonly HttpClient _httpClient;

    public GpuRankingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<int>> RankAsync(string query)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "http://10.5.0.6:2140/query",
            new { query });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<int>>() ?? [];
    }
}
