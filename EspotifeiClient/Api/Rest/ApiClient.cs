using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Api.GrpcClients;
using Api.Rest.ApiLogin;

namespace Api.Rest
{
    public class ApiClient
    {
        private static ApiClient _apiClient;
        private static HttpClient _httpClient;

        private ApiClient()
        {
        }

        /// <summary>
        ///     Regresa la instancia del HttpClient
        /// </summary>
        /// <returns>El HttpClient</returns>
        public static HttpClient GetApiClient()
        {
            if (_apiClient == null)
            {
                _apiClient = new ApiClient();
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.BaseAddress = new Uri(Configuration.URIRestServer);
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-access-token", ApiServiceLogin.GetServiceLogin().GetAccessToken());
            return _httpClient;
        }
    }
}