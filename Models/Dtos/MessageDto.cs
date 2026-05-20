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
            [property: JsonPropertyName("channelId")] int ChannelId,
            [property: JsonPropertyName("content")] string Content
        );
    }
}
