using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace QRCodeMaker
{
    public static class HttpHelper
    {
        public static async Task<T> Post<T>(string content, string url)
        {
            HttpContent httpContent = new StringContent(content);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, 120);
            HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), httpContent).ConfigureAwait(false);
            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (result.Contains("errmsg"))
                {
                    Console.WriteLine($"调用接口出错 =>{url} :  {result}");
                    throw new Exception();
                }

                if (result is T resultStr)
                {
                    return resultStr;
                }
                return JsonSerializer.Deserialize<T>(result);
            }
            else
            {
                Console.WriteLine($"调用接口出错 =>{url} : {result}");
            }

            return default;
        }

        public static async Task<byte[]> PostFile(string content, string url)
        {
            try
            {
                HttpContent httpContent = new StringContent(content);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient httpClient = new HttpClient { Timeout = new TimeSpan(0, 0, 120) };
                HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), httpContent).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode || responseContent.Contains("errmsg"))
                {
                    Console.WriteLine($"调用接口出错 =>{responseContent}");
                    if (responseContent.Contains("45009"))
                    {
                        Console.WriteLine("超过调用限制：等待一分钟");
                        Thread.Sleep(TimeSpan.FromSeconds(60));
                    }
                }
                else
                {
                    return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static async Task<string> PostFile1(string content, string url)
        {
            HttpContent httpContent = new StringContent(content);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient { Timeout = new TimeSpan(0, 0, 120) };
            HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), httpContent).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return result;
        }

        public static async Task<T> Get<T>(string url)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, 120);
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);
            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (result.Contains("errmsg"))
                {
                    Console.WriteLine($"调用接口出错 =>{url} :  {result}");
                }
                return JsonSerializer.Deserialize<T>(result);
            }
            else
            {
                Console.WriteLine($"调用接口出错 =>{url} : {result}");
            }

            return default;
        }
    }
}
