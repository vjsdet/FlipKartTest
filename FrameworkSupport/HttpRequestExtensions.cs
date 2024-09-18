using Microsoft.AspNetCore.Http;

namespace FrameworkSupport
{
    public static class HttpRequestExtensions
    {
        ///// <summary>
        ///// Gets the IP address from "REMOTE_ADDR" or from "HTTP_CF_CONNECTING_IP" if it exists.
        ///// If a client in production uses Cloudflare, the request comes from cloudflare so REMOTE_ADDR is no longer the user's IP.
        ///// Cloudflare adds the "HTTP_CF_CONNECTING_IP" header for the user's IP, so if this exists on the request we want to use that instead.
        ///// Since CloudFlare Pesudo IPv4 is enabled, we can grab the ipaddress from HTTP_CF_PSEUDO_IPV4
        ///// </summary>
        ///// <param name="httpContext"></param>
        ///// <returns></returns>
        public static string GetUserIP(this HttpRequest httpRequest)
        {
            if (httpRequest.Headers.TryGetValue("HTTP_CF_PSEUDO_IPV4", out var cloudflarePseudoIPv4)) // This will be an IPv4 Address
            {
                if (!string.IsNullOrWhiteSpace(cloudflarePseudoIPv4))
                {
                    return cloudflarePseudoIPv4;
                }
            }

            if (httpRequest.Headers.TryGetValue("HTTP_CF_CONNECTING_IP", out var cloudflareConnectingIP)) //This could be an IPv6 Address
            {
                if (!string.IsNullOrWhiteSpace(cloudflareConnectingIP))
                {
                    return cloudflareConnectingIP;
                }
            }

            if (httpRequest.Headers.TryGetValue("REMOTE_ADDR", out var ip)) //This could be an IPv6 Address
            {
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    return ip;
                }
            }

            return httpRequest.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}