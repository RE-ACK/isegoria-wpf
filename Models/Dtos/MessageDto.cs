using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace isegoria_wpf.Models.Dtos
{
    public class MessageDto
    {
        // ============================================================ //
        // Request / Response 모델
        // ============================================================ //

        public record CreateMessageRequest(
            [property: JsonPropertyName("channelId")] long ChannelId,
            [property: JsonPropertyName("content")] string Content
        );

        public record GetMessageRequest(
            [property: JsonPropertyName("lastMessageId")] long lastMessageId,
            [property: JsonPropertyName("size")] long size
        );
        public record MessageResponse(
            [property: JsonPropertyName("id")] long id,
            [property: JsonPropertyName("channelId")] long channelId,
            [property: JsonPropertyName("senderId")] long senderId,
            [property: JsonPropertyName("senderName")] string senderName,
            [property: JsonPropertyName("content")] string content,
            [property: JsonPropertyName("senderImage")] string senderImage,
            [property: JsonPropertyName("createdAt")] string createdAt
        );
    }
}
