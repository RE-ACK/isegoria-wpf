using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.IO;

using static isegoria_wpf.Models.Dtos.ServerDto;
using static System.Net.WebRequestMethods;

namespace isegoria_wpf.Services
{
    public class ApiServer
    {


        static ApiServer()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string baseUrl = config["Api:BaseUrl"]!;
            Debug.WriteLine("SERVER URL : " + baseUrl);

        }

        public static void SetAuthHeader(string accessToken)
        {
            ApiClient._client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        // ──────────────────────────────────────────────────────── //

        // [POST] 서버 생성
        // POST /api/servers        
        public static async Task<ServerInfo?> CreateServerAsync(string name, string? iconUrl)
        {
            try
            {
                var request = new CreateServerRequest(name, iconUrl);
                var response = await ApiClient._client.PostAsJsonAsync("api/servers", request);

                Debug.WriteLine($"Authorization 헤더: {ApiClient._client.DefaultRequestHeaders.Authorization}");

                // 원문 확인
                var raw = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"상태코드: {response.StatusCode}");
                Debug.WriteLine($"응답 원문: {raw}");


                var result = await response.Content
                    .ReadFromJsonAsync<ServerResponse>();

                return result?.Body;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateServerAsync error: {ex.Message}");
                return null;
            }
        }

        // [POST] 초대 코드로 서버 입장
        // POST /api/servers/join
        public static async Task<ServerInfo?> JoinServerAsync(string inviteCode)
        {
            try
            {
                var request = new JoinServerRequest(inviteCode);
                var response = await ApiClient._client.PostAsJsonAsync("api/servers/join", request);
                var result = await response.Content
                    .ReadFromJsonAsync<ServerResponse>();

                return result?.Body;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JoinServerAsync error: {ex.Message}");
                return null;
            }
        }

        // [GET] 내 서버 목록
        // GET /api/servers/my
        public static async Task<List<ServerInfo>?> GetMyServersAsync()
        {
            try
            {
                var response = await ApiClient._client.GetAsync("api/servers/my");
                var result = await response.Content
                    .ReadFromJsonAsync<ServerListResponse>();

                return result?.Body;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMyServersAsync error: {ex.Message}");
                return null;
            }
        }

        // [POST] 초대 코드 재발급
        // POST /api/servers/{serverId}/invite
        public static async Task<string?> RegenerateInviteCodeAsync(long serverId)
        {
            try
            {
                var response = await ApiClient._client.PostAsync($"api/servers/{serverId}/invite", null);
                var result = await response.Content
                    .ReadFromJsonAsync<InviteCodeResponse>();

                return result?.Body?.InviteCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RegenerateInviteCodeAsync error: {ex.Message}");
                return null;
            }
        }

        // [DELETE] 서버 나가기
        // DELETE /api/servers/{serverId}/leave
        public static async Task<bool> LeaveServerAsync(long serverId)
        {
            try
            {
                var response = await ApiClient._client.DeleteAsync($"api/servers/{serverId}/leave");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LeaveServerAsync error: {ex.Message}");
                return false;
            }
        }

        // [DELETE] 멤버 추방
        // DELETE /api/servers/{serverId}/members/{targetUserId}
        public static async Task<bool> KickMemberAsync(long serverId, long targetUserId)
        {
            try
            {
                var response = await ApiClient._client.DeleteAsync($"api/servers/{serverId}/members/{targetUserId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KickMemberAsync error: {ex.Message}");
                return false;
            }
        }

        // [DELETE] 서버 삭제
        // DELETE /api/servers/{serverId}
        public static async Task<bool> DeleteServerAsync(long serverId)
        {
            try
            {
                var response = await ApiClient._client.DeleteAsync($"api/servers/{serverId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteServerAsync error: {ex.Message}");
                return false;
            }
        }

        // [POST] 이미지 업로드
        // POST /api/images/upload
        public static async Task<List<string>?> UploadImagesAsync(string filePath)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileStream = System.IO.File.OpenRead(filePath);
                form.Add(new StreamContent(fileStream), "files", Path.GetFileName(filePath));

                Debug.WriteLine($"요청 URL: {ApiClient._client.BaseAddress}api/images/upload");
                var response = await ApiClient._client.PostAsync("api/images/upload", form);


                var result = await response.Content
                    .ReadFromJsonAsync<List<string>>();

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UploadImagesAsync error: {ex.Message}");
                return null;
            }
        }

    }
}