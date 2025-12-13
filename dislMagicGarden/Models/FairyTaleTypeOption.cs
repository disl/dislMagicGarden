namespace dislMagicGarden.Models
{
    public class FairyTaleTypeOption
    {
        public FairyTaleType Type { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";

        public string Title_description { get { return $"{Title} - {Description}"; } }

        public string Emoji { get; set; } = "";
    }

}
