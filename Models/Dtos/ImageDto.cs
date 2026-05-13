using System.Text.Json.Serialization;

namespace isegoria_wpf.Models.Dtos
{
    public class ImageDto
    {
        // POST /api/images/create 요청
        public record ImageCreateRequest(
            [property: JsonPropertyName("id")] string Id,
            [property: JsonPropertyName("existingImages")] List<string> ExistingImages,
            [property: JsonPropertyName("images")] List<string> Images,
            [property: JsonPropertyName("entity")] string Entity
        );
    }
}