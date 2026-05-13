using System.Text.Json.Serialization;

namespace isegoria_wpf.Models.Dtos
{
    public class ServerDto
    {
        // ============================================================ //
        // Request 모델
        // ============================================================ //

        // POST /api/servers
        public record CreateServerRequest(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("iconUrl")] string? IconUrl
        );

        // POST /api/servers/join
        public record JoinServerRequest(
            [property: JsonPropertyName("inviteCode")] string InviteCode
        );

        // ============================================================ //
        // Response 모델
        // ============================================================ //

        // 서버 정보 (body 안에 담기는 것)
        public record ServerInfo(
            [property: JsonPropertyName("id")] long Id,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("iconUrl")] string? IconUrl,
            [property: JsonPropertyName("inviteCode")] string InviteCode
        );

        // 서버 생성 / 입장 응답
        // POST /api/servers
        // POST /api/servers/join
        public record ServerResponse(
            [property: JsonPropertyName("statusCode")] int StatusCode,
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("body")] ServerInfo? Body
        );

        // 내 서버 목록 응답
        // GET /api/servers/my
        public record ServerListResponse(
            [property: JsonPropertyName("statusCode")] int StatusCode,
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("body")] List<ServerInfo>? Body
        );

        // 초대 코드 재발급 응답
        // POST /api/servers/{serverId}/invite
        public record InviteCodeInfo(
            [property: JsonPropertyName("inviteCode")] string InviteCode
        );

        public record InviteCodeResponse(
            [property: JsonPropertyName("statusCode")] int StatusCode,
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("body")] InviteCodeInfo? Body
        );

        // 파일 업로드 응답
        // POST /api/file/upload
        public record FileUploadResponse(
            [property: JsonPropertyName("statusCode")] int StatusCode,
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("body")] string? Url
        );
    }
}