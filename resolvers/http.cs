using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    internal class http
    {
        internal async static Task<string> Get(string url, ILogger log)
        {
            using (var http = new HttpClient())
            {
                Stopwatch clock = Stopwatch.StartNew();
                var data = await http.GetStringAsync(url);
                log.LogTrace($"GET '{url} in {clock.ElapsedMilliseconds} ms");
                return data;
            }
        }

        internal async static Task<bool> Head(string url, ILogger log)
        {
            using (var http = new HttpClient())
            {
                Stopwatch clock = Stopwatch.StartNew();
                var data = await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                log.LogTrace($"HEAD'{url} in {clock.ElapsedMilliseconds} ms");
                return data.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }
    }
}
