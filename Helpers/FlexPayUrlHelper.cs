namespace ProsocAPI.Helpers
{
    public static class FlexPayUrlHelper
    {
        public static string ResolveCallbackUrl(
            HttpContext? httpContext,
            string? callbackBaseUrl,
            bool forceProductionCallbackInDev)
        {
            if (!string.IsNullOrWhiteSpace(callbackBaseUrl)
                && (forceProductionCallbackInDev || httpContext == null || !IsPrivateHost(httpContext.Request.Host.Host)))
            {
                return callbackBaseUrl.Trim();
            }

            if (httpContext != null && !IsPrivateHost(httpContext.Request.Host.Host))
            {
                return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/FlexPay/callback";
            }

            if (!string.IsNullOrWhiteSpace(callbackBaseUrl))
                return callbackBaseUrl.Trim();

            throw new InvalidOperationException(
                "FlexPay:CallbackBaseUrl doit être configuré pour les environnements locaux.");
        }

        public static string DeriveRedirectUrl(string callbackBaseUrl, string action)
        {
            var baseUrl = callbackBaseUrl.Trim();
            if (baseUrl.EndsWith("/callback", StringComparison.OrdinalIgnoreCase))
                return baseUrl[..^"/callback".Length] + "/" + action;
            return baseUrl.TrimEnd('/') + "/" + action;
        }

        private static bool IsPrivateHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return true;
            return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                   || host.StartsWith("127.", StringComparison.Ordinal)
                   || host.StartsWith("10.", StringComparison.Ordinal)
                   || host.StartsWith("192.168.", StringComparison.Ordinal);
        }
    }
}
