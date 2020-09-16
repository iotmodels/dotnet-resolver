using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    internal class http
    {
        internal async static Task<string> Get(string url)
        {
            using (var http = new HttpClient())
            {
                var data = await http.GetStringAsync(url);
                return data;
            }
        }

        internal async static Task<bool> Head(string url)
        {
            using (var http = new HttpClient())
            {
                var data = await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                return data.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

    }
}
