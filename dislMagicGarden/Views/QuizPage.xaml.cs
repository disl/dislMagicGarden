using dislMagicGarden.Models;
using dislMagicGarden.ViewModels;

namespace dislMagicGarden.Views;

[QueryProperty(nameof(Questions), "Questions")]
public partial class QuizPage : FairyBasePage 
{
    private List<QuizQuestion> _questions;

    public List<QuizQuestion> Questions
    {
        get => _questions;
        set
        {
            _questions = value;

            if (BindingContext is QuizViewModel vm)
            {
                vm.LoadQuestions(value);
            }

            OnPropertyChanged();
            // Hier kannst du die Fragen weiterverarbeiten
        }
    }



    public QuizPage(QuizViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(true);
    }

}