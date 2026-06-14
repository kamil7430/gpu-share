using GpuShare.Frontend.Models;

public interface IGpuRankingService
{
    Task<List<int>> RankAsync(string query);
}
