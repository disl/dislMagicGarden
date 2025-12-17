namespace dislMagicGarden.Models
{
    public static class GenderOptionExtensions
    {
        public static string ToDisplay(this GenderOption gender) => gender switch
        {
            GenderOption.Male => Properties.Resources.boy,
            GenderOption.Female => Properties.Resources.girl,
            _ => Properties.Resources.Neutral
        };
    }

}
