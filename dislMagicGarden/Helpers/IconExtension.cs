namespace dislMagicGarden.Helpers
{
    [ContentProperty(nameof(Icon))]
    public class IconExtension : IMarkupExtension<string>
    {
        public string Icon { get; set; } = string.Empty;

        public string ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Icon))
                return string.Empty;

            var type = typeof(IconFont);
            var field = type.GetField(Icon);

            if (field == null || !field.IsLiteral || field.FieldType != typeof(string))
                return string.Empty; // oder throw, je nach Wunsch

            return (string)field.GetRawConstantValue();
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
            => ProvideValue(serviceProvider);
    }
}
