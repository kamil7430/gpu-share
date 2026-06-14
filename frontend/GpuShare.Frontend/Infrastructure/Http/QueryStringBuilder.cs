namespace GpuShare.Frontend.Infrastructure.Http
{
    public static class QueryStringBuilder
    {
        public static string Build(string path, object query)
        {
            var pairs = new List<string>();

            foreach (var prop in query.GetType().GetProperties())
            {
                var value = prop.GetValue(query);
                if (value is null) continue;

                // Expand collections into repeated key=value pairs
                if (value is System.Collections.IEnumerable enumerable and not string)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is not null)
                            pairs.Add($"{Uri.EscapeDataString(prop.Name)}={Uri.EscapeDataString(item.ToString()!)}");
                    }
                }
                else
                {
                    pairs.Add($"{Uri.EscapeDataString(prop.Name)}={Uri.EscapeDataString(value.ToString()!)}");
                }
            }

            return pairs.Count == 0 ? path : $"{path}?{string.Join("&", pairs)}";
        }
    }
}
