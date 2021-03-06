using Skrawl.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Skrawl.Helpers;
using Microsoft.JSInterop;

namespace Skrawl.Services
{
    public class HttpService : IHttpService
    {
        private HttpClient _httpClient;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;
        private IConfiguration _configuration;

        public HttpService(
            HttpClient httpClient,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService,
            IConfiguration configuration
        )
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
            _configuration = configuration;
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return await SendRequestAsync<T>(request);
        }

        public async Task PutAsync(string uri, object value = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            if (value != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            }
            await SendRequestAsync(request);
        }

        public async Task<T> PostAsync<T>(string uri, object value = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            if (value != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            }
            return await SendRequestAsync<T>(request);
        }

        public async Task PostAsync(string uri, object value = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            if (value != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            }
            await SendRequestAsync(request);
        }

        public async Task<T> DeleteAsync<T>(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            return await SendRequestAsync<T>(request);
        }

        // helper methods

        private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
        {
            // add jwt auth header if user is logged in and request is to the api url
            var token = await _localStorageService.GetItem<LoginResult>("token");

            var isApiUrl = !request.RequestUri.IsAbsoluteUri;
            if (token != null && isApiUrl)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            using var response = await _httpClient.SendAsync(request);

            // auto logout on 401 response
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("logout");
                return default;
            }

            // You're on timeout till you're fixed 
            // response.EnsureSuccessStatusCode();

            response.CheckResponseSuccess();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        private async Task SendRequestAsync(HttpRequestMessage request)
        {
            var token = await _localStorageService.GetItem<LoginResult>("token");

            var isApiUrl = !request.RequestUri.IsAbsoluteUri;
            if (token != null && isApiUrl)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            using var response = await _httpClient.SendAsync(request);

            // auto logout on 401 response
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("logout");
                return;
            }

            // You're on timeout till you're fixed 
            // response.EnsureSuccessStatusCode();

            response.CheckResponseSuccess();
        }
    }
}