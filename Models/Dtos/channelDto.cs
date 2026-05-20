using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace isegoria_wpf.Models.Dtos
{
    public class channelDto
    { 
        // 채널 타입
        public enum ChannelType { TEXT, VOICE }

        // 채널 정보
        public record ChannelInfo(
            [property: JsonPropertyName("id")] long Id,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("type")] string Type,
            [property: JsonPropertyName("serverId")] long ServerId,
            [property: JsonPropertyName("createdAt")] string CreatedAt
        );

        // 채널 목록 응답
        // GET /api/channels/all?serverId={serverId}
        public record ChannelListResponse(
            [property: JsonPropertyName("statusCode")] int StatusCode,
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("body")] List<ChannelInfo>? Body
        );

        // 채널 단건 응답
        public record ChannelResponse(
            [property: JsonPropertyName("statusCode")] int StatusCode,
            [property: JsonPropertyName("message")] string Message,
            [property: JsonPropertyName("body")] ChannelInfo? Body
        );

        // 채널 생성 요청
        // POST /api/channels/create
        public record CreateChannelRequest(
            [property: JsonPropertyName("serverId")] long ServerId,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("type")] string Type
        );
    }
}
