using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml;

namespace YandexTranslateCSharpSdk
{
    /// <summary>
    /// Wrapper for Get the list of supported languages methods
    /// https://tech.yandex.com/translate/doc/dg/reference/getLangs-docpage/
    /// </summary>
    internal class LanguagesManager
    {
        internal string ApiKey { get; set; }

        internal async Task<List<string>> GetLanguagesXmlAsync()
        {
            List<string> languages = new List<string>();
            string response = await PostDataAsync("https://translate.yandex.net/api/v1.5/tr/getLangs?", "application/xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(response);            
            XmlNodeList list = xmlDoc.GetElementsByTagName("Item");
            foreach(XmlNode node in list)
            {
                languages.Add(node.Attributes["key"].Value);
            }
            return languages;
        }

        internal async Task<List<string>> GetLanguagesJsonAsync()
        {            
            string response = await PostDataAsync("https://translate.yandex.net/api/v1.5/tr.json/getLangs?", "application/json");
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            var lang = JsonConvert.DeserializeObject<Dictionary<string, object>>(dict["langs"].ToString());
            return new List<string>(lang.Keys);           
        }

        private async Task<string> PostDataAsync(string url, string mediaType)
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
                    postData.Add(new KeyValuePair<string, string>("ui", "en"));

                    HttpContent content = new FormUrlEncodedContent(postData);
                    request.Content = content;
                    HttpResponseMessage response = await httpClient.SendAsync(request)
                           .ContinueWith((postTask) => postTask.Result.EnsureSuccessStatusCode());
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch(HttpRequestException)
            {
                throw new YandexTranslateException(
                    "Bad parameters or other problem communicating Yandex.Translate API");
            }
        }
    }
}
