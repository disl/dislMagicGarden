namespace dislMagicGarden.Models
{
    public static class FairyTaleTypes
    {
        public static List<FairyTaleTypeOption> All { get; } = new()
    {
        new() { Type = FairyTaleType.Grimm, Title=Properties.Resources.Brothers_Grimm, Description=Properties.Resources.Classic_German_fairy_tales, Emoji="🐺" },
        new() { Type = FairyTaleType.Andersen, Title=Properties.Resources.H_C_Andersen, Description=Properties.Resources.Poetic_fairy_tales_with_feeling, Emoji="🧜" },
        new() { Type = FairyTaleType.Modern, Title=Properties.Resources.Modern_fairy_tales, Description=Properties.Resources.Contemporary_and_welcoming, Emoji="🧁" },
        new() { Type = FairyTaleType.Fantasy, Title=Properties.Resources.fantasy, Description=Properties.Resources.Magical_worlds_and_creatures, Emoji="🐉" },
        new() { Type = FairyTaleType.Funny, Title=Properties.Resources.Funny, Description=Properties.Resources.Humorous_stories, Emoji="😂" },
        new() { Type = FairyTaleType.Educational, Title=Properties.Resources.Educational_fairy_tales, Description=Properties.Resources.Values_and_knowledge_through_play, Emoji="🌱" }
    };
    }

}
