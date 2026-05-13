using isegoria_wpf.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using static isegoria_wpf.Models.Dtos.AuthDto;

namespace isegoria_wpf.Services
{
    public class ApiClient
    {
        internal static readonly HttpClient _client;

        static ApiClient()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string baseUrl = config["Api:BaseUrl"]!;

            Debug.Write("SERVER URL : " + baseUrl);

            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public static void SetAuthHeader(string accessToken)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        //====================================================================================//

        /**
         * [POST] 회원가입
         */
        public static async Task<RegisterResponse?> RegisterAsync(string email, string username, string password)
        {
            try
            {
                var request = new RegisterRequest(email, username, password);
                var response = await _client.PostAsJsonAsync("api/auth/register", request);
                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();


                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RegisterAsync error: {ex.Message}");
                return null;
            }
        }

        /**
         * [POST] 로그인
         * 윈도우 Credential Manager에 토큰 저장
         */
        public static async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest(email, password);
                var response = await _client.PostAsJsonAsync("api/auth/login", request);
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (response.IsSuccessStatusCode && result?.Body != null)
                {
                    CredentialManager.SaveToken(
                        result.Body.ServerTokens.AccessToken,
                        result.Body.ServerTokens.RefreshToken
                    );

                    SetAuthHeader(result.Body.ServerTokens.AccessToken);

                    User.SetCurrentUser(new User
                    {
                        Id = result.Body.User.Id,
                        Username = result.Body.User.Username,
                        Email = result.Body.User.Email,
                        AvatarUrl = result.Body.User.AvatarUrl
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoginAsync error: {ex.Message}");
                return null;
            }
        }

    }
}
