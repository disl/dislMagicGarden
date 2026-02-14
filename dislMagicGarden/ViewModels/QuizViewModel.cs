using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace dislMagicGarden.ViewModels
{
    public partial class QuizViewModel : ObservableObject
    {
        private List<QuizQuestion> _questions;
        private int _currentIndex;
        private string _questionText;
        private string _progressText;
        private int _score = 0;


        [ObservableProperty] string questionText;
        [ObservableProperty] ObservableCollection<AnswerOption> options;
        [ObservableProperty] string progressText;
        private bool _isAnswered;

        //public ICommand SelectAnswerCommand { get; }

        public void LoadQuestions(List<QuizQuestion> questions)
        {
            _questions = questions;
            _currentIndex = 0;
            ShowCurrentQuestion();
        }

        [RelayCommand]
        private void SelectAnswer(AnswerOption selectedOption)
        {
            // Diese Methode wird automatisch zum Command

            if (selectedOption.IsCorrect)
            {
                selectedOption.BackgroundColor = Colors.Green;
                // Richtige Antwort behandeln
            }
            else
            {
                selectedOption.BackgroundColor = Colors.Red;
                // Falsche Antwort behandeln
            }

            // Verhindere Mehrfachauswahl
            DisableAllOptions();

            // Kurze Pause, dann nächste Frage
            Task.Delay(1000).ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(LoadNextQuestion);
            });
        }

        private void DisableAllOptions()
        {
            foreach (var option in Options)
            {
                // Buttons deaktivieren oder Farbe ändern
            }
        }



        private void ShowCurrentQuestion()
        {
            if (_questions != null && _questions.Count > _currentIndex)
            {
                var question = _questions[_currentIndex];
                QuestionText = question.question;

                // 1. Sammle alle Items zuerst in einer Liste
                var optionsList = new List<AnswerOption>();
                for (int i = 0; i < question.options.Count; i++)
                {
                    optionsList.Add(new AnswerOption
                    {
                        Text = question.options[i],
                        IsCorrect = (i == question.correctIndex),
                        BackgroundColor = Colors.LightGray
                    });
                }

                // 2. Weise die komplette Liste auf einmal zu
                Options = new ObservableCollection<AnswerOption>(optionsList);

                ProgressText = $"Frage {_currentIndex + 1} von {_questions.Count}";
            }
        }

        private void LoadNextQuestion()
        {
            _currentIndex++;
            _isAnswered = false;

            if (_currentIndex >= _questions.Count)
            {
                FinishQuiz();
                return;
            }

            var q = _questions[_currentIndex];
            QuestionText = q.question;
            ProgressText = $"Frage {_currentIndex + 1} von {_questions.Count}";

            Options.Clear();
            for (int i = 0; i < q.options.Count; i++)
            {
                Options.Add(new AnswerOption
                {
                    Text = q.options[i],
                    //Index = i,
                    BackgroundColor = Colors.White // Reset
                });
            }
        }

        private async void FinishQuiz()
        {
            // Ergebnis-Seite oder Popup
            await Application.Current.MainPage.DisplayAlert("Quiz beendet!",
                $"Du hast {_score} von {_questions.Count} Fragen richtig beantwortet.",
                "Super!");

            // Zurück zur Startseite oder Output-Seite
            await Shell.Current.GoToAsync(".."); // Oder Navigation.PopAsync()
        }


    }

    public partial class AnswerOption : ObservableObject
    {
        [ObservableProperty] public string text;

        //partial void OnTextChanged(string value)
        //{
        //    Text_correct = CorrectIndex ==Index ? value + "." : value;
        //}


        //partial void OnTextChang(string value)
        //{
        //    // Hier kannst du die Logik hinzufügen, um die Länge des Textes zu überprüfen
        //    Text_correct = IsCorrect ? Text + "." : Text;
        //}

        [ObservableProperty] bool isCorrect;
        [ObservableProperty] public string text_correct=string.Empty;
        [ObservableProperty] public int correctIndex;
        partial void OnCorrectIndexChanged(int value)
        {
            IsCorrect = value == CorrectIndex;
        }
        [ObservableProperty] public Color backgroundColor = Colors.LightGray;
        //[ObservableProperty] public int index;
        //partial void OnIndexChanged(int value)
        //{
        //    IsCorrect = value == CorrectIndex;
        //    Text_correct = IsCorrect ? Text + "." : Text;
        //}
    }

    //    public partial class QuizViewModel : BaseViewModel
    //    {

    //    //    private List<QuizQuestion> _allQuestions;
    //    //    private int _currentIndex = 0;
    //    //    private int _score = 0;
    //    //    private bool _isAnswered = false;

    //    //    [ObservableProperty] private string _questionText;
    //    //    [ObservableProperty] private string _progressText;
    //    //    [ObservableProperty] private bool _isBusy;

    //    //    public ObservableCollection<QuizOption> Options { get; set; } = new();

    //    //    public ICommand SelectAnswerCommand { get; }

    //    //    public QuizViewModel()
    //    //    {
    //    //        SelectAnswerCommand = new Command<QuizOption>(OnOptionSelected);
    //    //    }

    //    //    // Diese Methode wird aufgerufen, wenn man die Seite öffnet (Parameter Übergabe)
    //    //    public void LoadQuestions(List<QuizQuestion> questions)
    //    //    {
    //    //        _allQuestions = questions;
    //    //        _currentIndex = -1; // Start bei -1, damit LoadNext bei 0 anfängt
    //    //        _score = 0;
    //    //        LoadNextQuestion();
    //    //    }

    //    //    private void LoadNextQuestion()
    //    //    {
    //    //        _currentIndex++;
    //    //        _isAnswered = false;

    //    //        if (_currentIndex >= _allQuestions.Count)
    //    //        {
    //    //            FinishQuiz();
    //    //            return;
    //    //        }

    //    //        var q = _allQuestions[_currentIndex];
    //    //        QuestionText = q.question;
    //    //        ProgressText = $"Frage {_currentIndex + 1} von {_allQuestions.Count}";

    //    //        Options.Clear();
    //    //        for (int i = 0; i < q.options.Count; i++)
    //    //        {
    //    //            Options.Add(new QuizOption
    //    //            {
    //    //                Text = q.options[i],
    //    //                Index = i,
    //    //                BackgroundColor = Colors.White // Reset
    //    //            });
    //    //        }
    //    //    }

    //    //    private async void OnOptionSelected(QuizOption selectedOption)
    //    //    {
    //    //        if (_isAnswered) return; // Verhindern, dass man mehrfach klickt
    //    //        _isAnswered = true;

    //    //        var currentQuestion = _allQuestions[_currentIndex];
    //    //        bool isCorrect = selectedOption.Index == currentQuestion.correctIndex;

    //    //        // 1. Visuelles Feedback setzen
    //    //        if (isCorrect)
    //    //        {
    //    //            // Richtig: Grün
    //    //            selectedOption.BackgroundColor = Color.FromArgb("#4CAF50"); // Grün
    //    //            _score++;
    //    //        }
    //    //        else
    //    //        {
    //    //            // Falsch: Rot für Auswahl, Grün für richtige Antwort
    //    //            selectedOption.BackgroundColor = Color.FromArgb("#F44336"); // Rot

    //    //            // Die richtige Antwort finden und grün markieren
    //    //            var correctOption = Options.FirstOrDefault(o => o.Index == currentQuestion.correctIndex);
    //    //            if (correctOption != null)
    //    //                correctOption.BackgroundColor = Color.FromArgb("#4CAF50");
    //    //        }

    //    //        // UI aktualisieren triggern (bei ObservableCollection passiert das automatisch bei Property Change)
    //    //        // Da wir die Property im Objekt ändern, müssen wir das Collection-Update triggern oder
    //    //        // besser: Wir machen es einfach und nutzen INotifyPropertyChanged im QuizOption Modell.
    //    //        // Alternative Hack für einfache Demos: Objekt ersetzen.
    //    //        var indexInList = Options.IndexOf(selectedOption);
    //    //        Options[indexInList] = selectedOption;

    //    //        // 2. Kurze Pause, damit der Nutzer die Farbe sieht
    //    //        await Task.Delay(1500);

    //    //        // 3. Nächste Frage
    //    //        LoadNextQuestion();
    //    //    }

    //    //    private async void FinishQuiz()
    //    //    {
    //    //        // Ergebnis-Seite oder Popup
    //    //        await Application.Current.MainPage.DisplayAlert("Quiz beendet!",
    //    //            $"Du hast {_score} von {_allQuestions.Count} Fragen richtig beantwortet.",
    //    //            "Super!");

    //    //        // Zurück zur Startseite oder Output-Seite
    //    //        await Shell.Current.GoToAsync(".."); // Oder Navigation.PopAsync()
    //    //    }
    //    //}

    //    //// WICHTIG: Damit die Farbe im Objekt aktualisiert wird, muss QuizOption INotifyPropertyChanged implementieren
    //    //// oder du nutzt das CommunityToolkit 'ObservableProperty' auch hierfür. 
    //    //// Einfachste Lösung für das Model:
    //    //public partial class QuizOption : ObservableObject
    //    //{
    //    //    [ObservableProperty] private string _text;
    //    //    [ObservableProperty] private int _index;
    //    //    [ObservableProperty] private Color _backgroundColor;
    //    //}
}
