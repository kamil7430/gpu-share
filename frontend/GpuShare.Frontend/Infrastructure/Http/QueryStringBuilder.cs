namespace GpuShare.Frontend.Infrastructure.Http
{
    public static class QueryStringBuilder
    {
        public static string Build(string path, object query)
        {
            var properties = query.GetType().GetProperties();

            var values = properties
                .Select(p => new
                {
                    p.Name,
                    Value = p.GetValue(query)
                })
                .Where(x => x.Value != null)
                .Select(x =>
                    $"{Uri.EscapeDataString(x.Name)}={Uri.EscapeDataString(x.Value!.ToString()!)}");

            return $"{path}?{string.Join("&", values)}";
        }
    }
}
