using System.Text.Json.Serialization;

namespace dislMagicGarden.Models
{
    /// <summary>
    /// A child the stories are written for. Name and description are only sent to the AI provider with consent.
    /// </summary>
    public record ChildProfile(
        Guid Id,
        string Name,
        // Stored as text so reordering the enum cannot silently change stored profiles
        [property: JsonConverter(typeof(JsonStringEnumConverter<GenderOption>))] GenderOption Gender,
        string Description,
        bool HasConsent);
}
