using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace YandexTranslateCSharpSdk
{
    class TranslateRequest
    {
        public string SourceLanguageCode { get; set; }
        public string TargetLanguageCode { get; set; }
        public string Format { get; set; } = "PLAIN_TEXT";
        public string[] Texts { get; set; }
    }

    class TranslateResponse
    {
        public TranslateResult[] Translations { get; set; }
    }

    class TranslateResult
    {
        public string Text { get; set; }
        public string DetectedLanguageCode { get; set; }
    }
    /// <summary>
    /// Wrapper for Translate a text methods
    /// https://tech.yandex.com/translate/doc/dg/reference/translate-docpage/
    /// </summary>
    internal class TranslateManager
    {
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly string apiKey;
        
        public TranslateManager(string apiKey)
        {
            this.apiKey = apiKey;
        }


        internal async Task<string[]> TranslateTextJsonAsync(string[] texts, string source, string target)
        {
            var response = await PostDataAsync(texts, source, target,
             "https://translate.api.cloud.yandex.net/translate/v2/translate", "application/json");
            return response.Translations.Select(x => x.Text).ToArray();
        }

        private async Task<TranslateResponse> PostDataAsync(string[] texts, string source, string target, string url, string mediaType)
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);

            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue(mediaType));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", apiKey);

            var request = new HttpRequestMessage(HttpMethod.Post, "");

            var translateRequest = new TranslateRequest
            {
                SourceLanguageCode = source,
                TargetLanguageCode = target,
                Texts = texts,
            };
            var content = JsonContent.Create(translateRequest, options: jsonSerializerOptions);
            request.Content = content;
            var response = await httpClient.SendAsync(request)
                .ContinueWith((postTask) => postTask.Result.EnsureSuccessStatusCode());
            return await response.Content.ReadFromJsonAsync<TranslateResponse>(jsonSerializerOptions);
        }
    }
}
