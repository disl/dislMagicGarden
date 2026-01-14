using System.Collections.ObjectModel;

namespace dislMagicGarden.Views;

public partial class AdventureHistoryPage : FairyBasePage
{
    public ObservableCollection<HistoryItem> HistoryItems { get; } = new();

    public AdventureHistoryPage()
    {
        InitializeComponent();
        BindingContext = this;
        HistoryCollectionView.ItemsSource = HistoryItems;
    }

    public void LoadHistory(List<string> history)
    {
        HistoryItems.Clear();

        for (int i = 0; i < history.Count; i++)
        {
            var item = history[i];
            string icon = GetIconForStep(i, item);
            string stepNumber = $"Schritt {i + 1}";

            HistoryItems.Add(new HistoryItem
            {
                Icon = icon,
                StepNumber = stepNumber,
                Text = item
            });
        }

        // Zum letzten Eintrag scrollen
        if (HistoryItems.Count > 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                HistoryCollectionView.ScrollTo(HistoryItems.Count - 1,
                    position: ScrollToPosition.End,
                    animate: false);
            });
        }
    }

    private string GetIconForStep(int index, string text)
    {
        if (index == 0) return "🎯";
        if (text.StartsWith("👉")) return "👉";
        if (text.StartsWith("📖")) return "📖";
        if (text.Contains("gewählt") || text.Contains("Option")) return "✅";
        return "📝";
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}

public class HistoryItem
{
    public string Icon { get; set; }
    public string StepNumber { get; set; }
    public string Text { get; set; }
}