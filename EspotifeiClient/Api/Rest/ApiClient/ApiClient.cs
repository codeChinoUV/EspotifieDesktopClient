using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Api.Rest.ApiClient
{
    public class ApiClient
    {
        private static ApiClient _apiClient;
        private static HttpClient _httpClient;

        private ApiClient()
        {
            
        }

        public static HttpClient GetApiClient()
        {
            if (_apiClient == null)
            {
                _apiClient = new ApiClient();
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.BaseAddress = new Uri("http://ec2-54-160-126-163.compute-1.amazonaws.com:5000/");
                _httpClient.DefaultRequestHeaders.Add("x-access-token", ApiServiceLogin.GetServiceLogin().GetAccessToken());
            }

            return _httpClient;
        }
    }
}