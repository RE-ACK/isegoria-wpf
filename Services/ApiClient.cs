using isegoria_wpf.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using static isegoria_wpf.Models.Dtos.AuthDto;
using static isegoria_wpf.Models.Dtos.MessageDto;
using static isegoria_wpf.Models.Dtos.channelDto;
using static isegoria_wpf.Models.Dtos.ServerDto;

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

                    // 성공시 C++ 서버 연결 및 AUTH 패킷 전송
                    var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();

                    string host = config["Realtime:Host"]!;
                    int port = int.Parse(config["Realtime:Port"]!);

                    await RealtimeClient.Instance.ConnectAsync(host, port);
                    await RealtimeClient.Instance.SendPacketAsync(new
                    {
                        type = "AUTH",
                        token = result.Body.ServerTokens.AccessToken
                    });

                    RealtimeClient.Instance.OnPacketReceived += (type, json) =>
                    {
                        if (type == "AUTH_OK")
                        {
                            // sessionToken 저장
                            var sessionToken = json.GetProperty("sessionToken").ToString();
                            CredentialManager.SaveSessionToken(sessionToken);
                        }
                        else if (type == "AUTH_FAIL")
                        {
                            // 연결 끊기
                            RealtimeClient.Instance.Disconnect();
                        }
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoginAsync error: {ex.Message}");
                return null;
            }
        }

        /*
         [PUT] 유저 정보 업데이트
         */

        public static async Task<UpdateUserResponse?> UpdateUserAsync(string username, string? avatarUrl)
        {
            try
            {
                var request = new UpdateUserRequest(username, avatarUrl);
                var response = await _client.PutAsJsonAsync("api/user/update", request);

                var raw = await response.Content.ReadAsStringAsync();

                var result = System.Text.Json.JsonSerializer.Deserialize<UpdateUserResponse>(raw);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateUserAsync error: {ex.Message}");
                return null;
            }
        }
        //====================================================================================//

        // [POST] 메세지 생성
        // POST /api/message/create        
        public static async Task<bool> CreateMessageAsync(int channelId, string content)
        {
            try
            {
                var request = new CreateMessageRequest(channelId, content);
                var response = await ApiClient._client.PostAsJsonAsync("api/messages/create", request);

                var raw = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"상태코드: {response.StatusCode}");
                Debug.WriteLine($"응답 원문: {raw}");

                //var result = System.Text.Json.JsonSerializer.Deserialize<ServerResponse>(raw);

                //Debug.WriteLine($"=== 메세지 생성 결과 ===");
                //Debug.WriteLine($"server.Name: {result?.Body?.Name ?? "null"}");
                //Debug.WriteLine($"server.IconUrl: {result?.Body?.IconUrl ?? "null"}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateServerAsync error: {ex.Message}");
                return false;
            }
        }

        //====================================================================================//

        // [POST] 서버 생성
        // POST /api/servers        
        public static async Task<ServerInfo?> CreateServerAsync(string name, string? iconUrl)
        {
            try
            {
                var request = new CreateServerRequest(name, iconUrl);
                var response = await ApiClient._client.PostAsJsonAsync("api/servers", request);

                var raw = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"상태코드: {response.StatusCode}");
                Debug.WriteLine($"응답 원문: {raw}");

                var result = System.Text.Json.JsonSerializer.Deserialize<ServerResponse>(raw);

                Debug.WriteLine($"=== 서버 생성 결과 ===");
                Debug.WriteLine($"server.Name: {result?.Body?.Name ?? "null"}");
                Debug.WriteLine($"server.IconUrl: {result?.Body?.IconUrl ?? "null"}");

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
        public static async Task<(ServerInfo? Server, string? Message)> JoinServerAsync(string inviteCode)
        {
            try
            {
                var request = new JoinServerRequest(inviteCode);
                var response = await ApiClient._client.PostAsJsonAsync("api/servers/join", request);

                var raw = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"JoinServer 응답: {raw}");

                var result = System.Text.Json.JsonSerializer.Deserialize<ServerResponse>(raw);

                return (result?.Body, result?.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JoinServerAsync error: {ex.Message}");
                return (null, null);
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

        // [GET] 서버 멤버 목록
        // GET /api/servers/{serverId}/members
        public static async Task<List<MemberInfo>?> GetServerMembersAsync(long serverId)
        {
            try
            {
                var response = await ApiClient._client.GetAsync($"api/servers/{serverId}/members");
                var raw = await response.Content.ReadAsStringAsync();
                //Debug.WriteLine($"멤버 목록 응답: {raw}");

                var result = System.Text.Json.JsonSerializer.Deserialize<MemberListResponse>(raw);
                return result?.Body;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetServerMembersAsync error: {ex.Message}");
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

                var response = await ApiClient._client.PostAsync("api/images/upload", form);
                var raw = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<List<string>>(raw);

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UploadImagesAsync error: {ex.Message}");
                return null;
            }
        }

        // [GET] 채널 목록 조회
        // GET /api/channels/all?serverId={serverId}
        public static async Task<List<ChannelInfo>?> GetChannelsAsync(long serverId)
        {
            try
            {
                var response = await ApiClient._client.GetAsync($"api/channels/all?serverId={serverId}");
                var raw = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"채널 목록 응답: {raw}");

                var result = System.Text.Json.JsonSerializer.Deserialize<ChannelListResponse>(raw);
                return result?.Body;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetChannelsAsync error: {ex.Message}");
                return null;
            }
        }

        // [POST] 채널 생성
        // POST /api/channels/create
        public static async Task<ChannelInfo?> CreateChannelAsync(long serverId, string name, string type)
        {
            try
            {
                var request = new CreateChannelRequest(serverId, name, type);
                var response = await ApiClient._client.PostAsJsonAsync("api/channels/create", request);

                var raw = await response.Content.ReadAsStringAsync();

                var result = System.Text.Json.JsonSerializer.Deserialize<ChannelResponse>(raw);
                return result?.Body;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateChannelAsync error: {ex.Message}");
                return null;
            }
        }
    }
}
