using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace YandexTranslateCSharpSdk
{
    /// <summary>
    /// Wrapper for Translate a text methods
    /// https://tech.yandex.com/translate/doc/dg/reference/translate-docpage/
    /// </summary>
    internal class TranslateManager
    {
        internal string ApiKey { get; set; }

        internal async Task<string> TranslateTextXmlAsync(string text, string direction)
        {
            string response = await PostDataAsync(text, direction,
                "https://translate.yandex.net/api/v1.5/tr/translate?", "application/xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(response);
            XmlNodeList list = xmlDoc.GetElementsByTagName("text");
            if (list.Count > 0)
            {
                return list[0].InnerText;
            }
            return null;
        }
        internal async Task<string> TranslateTextJsonAsync(string text, string direction)
        {
            string response = await PostDataAsync(text, direction,
             "https://translate.yandex.net/api/v1.5/tr.json/translate?", "application/json");
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(response); 
            var outputText = dict["text"];
            if (outputText == null)
            {
                return null; 
            }
            else
            {
                JArray list = outputText as JArray;
                if (list.Count > 0)
                {
                    return list[0].ToString();
                }

            }
            return null;
        }

        private async Task<string> PostDataAsync(string text, string direction, string url, string mediaType)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(url);

                    httpClient.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue(mediaType));

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                        "");

                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("key", ApiKey));
                    postData.Add(new KeyValuePair<string, string>("text", text));
                    postData.Add(new KeyValuePair<string, string>("lang", direction));

                    HttpContent content = new FormUrlEncodedContent(postData);
                    request.Content = content;
                    HttpResponseMessage response = await httpClient.SendAsync(request)
                           .ContinueWith((postTask) => postTask.Result.EnsureSuccessStatusCode());
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException)
            {
                throw new YandexTranslateException(
                    "Bad parameters or other problem communicating Yandex.Translate API");
            }
        }
    }
}
