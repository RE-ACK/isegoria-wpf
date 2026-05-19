using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace isegoria_wpf.Models.Dtos
{
    public class AuthDto
    {
        // ============================================================ //
        // Request / Response 모델
        // ============================================================ //

        public record RegisterRequest(
            [property: JsonPropertyName("email")] string Email,
            [property: JsonPropertyName("username")] string Username,
            [property: JsonPropertyName("password")] string Password
        );

        public record RegisterResponse(
            [property: JsonPropertyName("statusCode")] int StatusCode,
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("body")] UserInfo Body
        );

        public record LoginRequest(
            [property: JsonPropertyName("email")] string Email,
            [property: JsonPropertyName("password")] string Password
        );

        public record LoginResponse(
            [property: JsonPropertyName("statusCode")] int StatusCode,
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("body")] LoginBody? Body
        );

        public record LoginBody(
            [property: JsonPropertyName("user")] UserInfo User,
            [property: JsonPropertyName("serverTokens")] ServerTokens ServerTokens
        );

        public record UserInfo(
            [property: JsonPropertyName("id")] long Id,
            [property: JsonPropertyName("username")] string Username,
            [property: JsonPropertyName("email")] string Email,
            [property: JsonPropertyName("avatarUrl")] string? AvatarUrl,
            [property: JsonPropertyName("createdAt")] string CreatedAt
        );

        public record ServerTokens(
            [property: JsonPropertyName("accessToken")] string AccessToken,
            [property: JsonPropertyName("refreshToken")] string RefreshToken
        );

    }
}
