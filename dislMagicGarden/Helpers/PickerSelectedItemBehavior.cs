namespace dislMagicGarden.Helpers
{
    public class PickerSelectedItemBehavior : Behavior<Picker>
    {
        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(object),
            typeof(PickerSelectedItemBehavior), null,
            BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private Picker _picker;

        protected override void OnAttachedTo(Picker picker)
        {
            base.OnAttachedTo(picker);
            _picker = picker;
            picker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
        }

        protected override void OnDetachingFrom(Picker picker)
        {
            base.OnDetachingFrom(picker);
            picker.SelectedIndexChanged -= OnPickerSelectedIndexChanged;
            _picker = null;
        }

        private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_picker.SelectedIndex != -1)
            {
                SelectedItem = _picker.ItemsSource?.Cast<object>()?.ElementAt(_picker.SelectedIndex);
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var behavior = (PickerSelectedItemBehavior)bindable;
            if (behavior._picker != null && newValue != null)
            {
                var items = behavior._picker.ItemsSource?.Cast<object>()?.ToList();
                var index = items?.IndexOf(newValue) ?? -1;
                if (index != -1 && index != behavior._picker.SelectedIndex)
                {
                    behavior._picker.SelectedIndex = index;
                }
            }
        }
    }
}
