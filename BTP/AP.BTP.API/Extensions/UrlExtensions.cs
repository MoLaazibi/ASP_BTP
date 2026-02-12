namespace AP.BTP.API.Extensions
{
    public static class UrlExtensions
    {
        public static string ToAbsoluteUrl(this HttpRequest request, string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return relativePath ?? "";

            if (relativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return relativePath;

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
            return $"{baseUrl}{relativePath}";
        }
    }
}
