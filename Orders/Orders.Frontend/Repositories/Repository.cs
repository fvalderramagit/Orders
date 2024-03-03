using System.Text;
using System.Text.Json;

namespace Orders.Frontend.Repositories
{
    public class Repository : IRepository
    {
        private readonly HttpClient _httpClient;

        private JsonSerializerOptions _jsonDefaultOptions => new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public Repository(HttpClient httpClient)
        {
            httpClient = httpClient;
        }

        /// <summary>
        /// Este es un GET
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HttpResponseWrapper<T>> GetAsync<T>(string url)
        {
            var responHttp = await _httpClient.GetAsync(url);
            if (responHttp.IsSuccessStatusCode)
            {
                var response = await UnserializAnswer<T>(responHttp);
                return new HttpResponseWrapper<T>(response, false, responHttp);
            }
            return new HttpResponseWrapper<T>(default, true, responHttp);
        }

        /// <summary>
        /// Este es un POST que no devuelve una respuesta
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<HttpResponseWrapper<object>> PostAsync<T>(string url, T model)
        {
            var messageJson = JsonSerializer.Serialize(model);
            var messageContent = new StringContent(messageJson, Encoding.UTF8, "aplication/json");
            var responseHttp = await _httpClient.PostAsync(url, messageContent);

            return new HttpResponseWrapper<object>(null, !responseHttp.IsSuccessStatusCode, responseHttp);
        }

        /// <summary>
        /// Este es un POST que devuelve una respuesta
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TActionResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<HttpResponseWrapper<TActionResponse>> PostAsync<T, TActionResponse>(string url, T model)
        {
            var messageJson = JsonSerializer.Serialize(model);
            var messageContent = new StringContent(messageJson, Encoding.UTF8, "aplication/json");
            var responseHttp = await _httpClient.PostAsync(url, messageContent);
            if (responseHttp.IsSuccessStatusCode)
            {
                var response = await UnserializAnswer<TActionResponse>(responseHttp);
                return new HttpResponseWrapper<TActionResponse>(response, false, responseHttp);
            }
            return new HttpResponseWrapper<TActionResponse>(default, true, responseHttp);
        }

        private async Task<T> UnserializAnswer<T>(HttpResponseMessage responHttp)
        {
            var response = await responHttp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(response, _jsonDefaultOptions)!;
        }
    }
}