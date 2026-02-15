using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace dislMagicGarden.ViewModels
{
    public partial class QuizViewModel : ObservableObject
    {
        private readonly Color _themePrimaryColor;
        private List<QuizQuestion> _questions;
        private int _currentIndex;
        private int _score = 0;
        private bool _isAnswered = false;

        private IAudioPlayer _wowSound;

        // Event für Feuerwerk
        public event EventHandler FireworksRequested;

        [ObservableProperty]
        private string questionText;

        [ObservableProperty]
        private ObservableCollection<AnswerOption> options;

        [ObservableProperty]
        private string progressText;

        public QuizViewModel()
        {
            var res = Application.Current.Resources;  // res["ThemePrimaryColor"]

            _themePrimaryColor = (Color)res["ThemePrimaryColor"];

            // AudioManager initialisieren
            LoadSounds();
        }
        private async void LoadSounds()
        {
            var audioManager = AudioManager.Current;

            // Sound aus Embedded Resource laden
            var assembly = typeof(QuizViewModel).Assembly;
            using var stream = await FileSystem.OpenAppPackageFileAsync("applause.mp3");

            if (stream != null)
            {
                _wowSound = audioManager.CreatePlayer(stream);
            }
        }

        private void PlayWowSound()
        {
            if (_wowSound != null)
            {
                _wowSound.Play();
            }
        }

        public void LoadQuestions(List<QuizQuestion> questions)
        {
            _questions = questions;
            _currentIndex = 0;
            _score = 0;
            _isAnswered = false;
            ShowCurrentQuestion();
        }

        [RelayCommand(CanExecute = nameof(CanSelectAnswer))]
        private void SelectAnswer(AnswerOption selectedOption)
        {
            _isAnswered = true;
            SelectAnswerCommand.NotifyCanExecuteChanged();

            // Antwort auswerten
            bool isCorrect = selectedOption.Index == selectedOption.CorrectIndex;

            if (isCorrect)
            {
                selectedOption.BackgroundColor = Colors.Green;
                _score++;
            }
            else
            {
                selectedOption.BackgroundColor = Colors.Red;
                // Richtige Antwort markieren
                foreach (var option in Options)
                {
                    if (option.Index == option.CorrectIndex)
                    {
                        option.BackgroundColor = Colors.Green;
                        break;
                    }
                }
            }

            // Buttons deaktivieren
            DisableAllOptions();

            // Nächste Frage nach Verzögerung
            Task.Delay(1500).ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _isAnswered = false;
                    SelectAnswerCommand.NotifyCanExecuteChanged();
                    LoadNextQuestion();
                });
            });
        }

        private bool CanSelectAnswer()
        {
            return !_isAnswered;
        }

        private void DisableAllOptions()
        {
            foreach (var option in Options)
            {
                option.IsEnabled = false;
                // Leicht abdunkeln für deaktivierten Zustand
                if (option.BackgroundColor == Colors.Green)
                {
                    option.BackgroundColor = Color.FromRgb(0, 100, 0);
                }
                else if (option.BackgroundColor == Colors.Red)
                {
                    option.BackgroundColor = Color.FromRgb(100, 0, 0);
                }
            }
        }

        private void ShowCurrentQuestion()
        {
            if (_questions != null && _questions.Count > _currentIndex)
            {
                var question = _questions[_currentIndex];
                QuestionText = question.question;

                var optionsList = new List<AnswerOption>();
                for (int i = 0; i < question.options.Count; i++)
                {
                    var option = new AnswerOption();
                    option.SetValues(question.options[i], question.correctIndex, i);
                    option.BackgroundColor = _themePrimaryColor;
                    option.IsEnabled = true;
                    optionsList.Add(option);
                }

                Options = new ObservableCollection<AnswerOption>(optionsList);
                //ProgressText = $"Frage {_currentIndex + 1} von {_questions.Count}";
                    ProgressText = $"{Properties.Resources.Question_of_n
                        .Replace("%1", (_currentIndex + 1).ToString())
                        .Replace("%2", (_questions.Count).ToString())}";
            }
        }

        private void LoadNextQuestion()
        {
            _currentIndex++;

            if (_currentIndex >= _questions.Count)
            {
                FinishQuiz();
                return;
            }

            var question = _questions[_currentIndex];
            QuestionText = question.question;
            //ProgressText = $"Frage {_currentIndex + 1} von {_questions.Count}";
            ProgressText = $"{Properties.Resources.Question_of_n
                       .Replace("%1", (_currentIndex + 1).ToString())
                       .Replace("%2", (_questions.Count).ToString())}";

            // Neue Options für die nächste Frage erstellen
            var newOptions = new List<AnswerOption>();
            for (int i = 0; i < question.options.Count; i++)
            {
                var option = new AnswerOption();
                option.SetValues(question.options[i], question.correctIndex, i);
                option.BackgroundColor = _themePrimaryColor;
                option.IsEnabled = true;
                newOptions.Add(option);
            }

            Options = new ObservableCollection<AnswerOption>(newOptions);
        }

        private async void FinishQuiz()
        {
            string message = _score == _questions.Count
                ? Properties.Resources.Perfect_All_questions_correct
                : Properties.Resources.You_answered_out_of_questions_correctly
                    .Replace("%1", _score.ToString())
                    .Replace("%2", _questions.Count.ToString());
            //: $"Du hast {_score} von {_questions.Count} Fragen richtig beantwortet.";

            if (_score == _questions.Count)
            {
                PlayWowSound();

                // Event auslösen für Feuerwerk
                FireworksRequested?.Invoke(this, EventArgs.Empty);

                await Application.Current.MainPage.DisplayAlert(
                  "🎉🎉🎉🎉🎉🎉",
                 message + "\n\n" + "✨✨✨✨✨✨",
                  "Ok");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    Properties.Resources.Quiz_finished,
                    message,
                    "Ok");
            }

            await Shell.Current.GoToAsync("..");
        }
    }

    public partial class AnswerOption : ObservableObject
    {
        [ObservableProperty]
        private bool isEnabled = true;

        [ObservableProperty]
        private string text;

        [ObservableProperty]
        private string textCorrect = string.Empty;

        [ObservableProperty]
        private int correctIndex;

        [ObservableProperty]
        private Color backgroundColor;

        [ObservableProperty]
        private int index;

        private bool _isUpdating;

        partial void OnTextChanged(string value)
        {
            if (!_isUpdating) UpdateTextCorrect();
        }

        partial void OnCorrectIndexChanged(int value)
        {
            if (!_isUpdating) UpdateTextCorrect();
        }

        partial void OnIndexChanged(int value)
        {
            if (!_isUpdating) UpdateTextCorrect();
        }

        private void UpdateTextCorrect()
        {
            if (_isUpdating) return;

            _isUpdating = true;

            string newValue = (correctIndex == index) ? $"{text}." : text;

            if (TextCorrect != newValue)
            {
                TextCorrect = newValue;
            }

            _isUpdating = false;
        }

        public void SetValues(string text, int correctIdx, int idx)
        {
            _isUpdating = true;
            Text = text;
            CorrectIndex = correctIdx;
            Index = idx;
            TextCorrect = (correctIdx == idx) ? $"{text}." : text;
            _isUpdating = false;
        }
    }
}


//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using dislMagicGarden.Models;
//using System.Collections.ObjectModel;

//namespace dislMagicGarden.ViewModels
//{
//    public partial class QuizViewModel : ObservableObject
//    {
//        private readonly Color _themePrimaryColor;


//        private List<QuizQuestion> _questions;
//        private int _currentIndex;
//        private string _questionText;
//        private string _progressText;
//        private int _score = 0;


//        [ObservableProperty] string questionText;
//        [ObservableProperty] ObservableCollection<AnswerOption> options;
//        [ObservableProperty] string progressText;
//        //private bool _isAnswered;

//        //public ICommand SelectAnswerCommand { get; }

//        public QuizViewModel()
//        {
//            // Im Konstruktor einmalig laden
//            if (Application.Current.Resources.TryGetValue("ThemePrimaryColor", out var color))
//            {
//                _themePrimaryColor = (Color)color;
//            }
//            else
//            {
//                _themePrimaryColor = Colors.HotPink; // Fallback
//            }
//        }

//        public void LoadQuestions(List<QuizQuestion> questions)
//        {
//            _questions = questions;
//            _currentIndex = 0;
//            ShowCurrentQuestion();
//        }

//        [RelayCommand]
//        private void SelectAnswer(AnswerOption selectedOption)
//        {
//            // Diese Methode wird automatisch zum Command

//            if (selectedOption.Index == selectedOption.CorrectIndex)
//            {
//                selectedOption.BackgroundColor = Colors.Green;
//                // Richtige Antwort behandeln
//                _score++;
//            }
//            else
//            {
//                selectedOption.BackgroundColor = Colors.Red;
//                // Falsche Antwort behandeln
//            }

//            // Verhindere Mehrfachauswahl
//            DisableAllOptions();

//            // Kurze Pause, dann nächste Frage
//            Task.Delay(1000).ContinueWith(_ =>
//            {
//                MainThread.BeginInvokeOnMainThread(LoadNextQuestion);
//            });
//        }

//        private void DisableAllOptions()
//        {
//            foreach (var option in Options)
//            {
//                //option.IsEnabled = false;
//                // Optional: Farbe ändern für deaktivierten Zustand
//                option.BackgroundColor = _themePrimaryColor;  // Colors.LightGray;
//            }
//        }



//        private void ShowCurrentQuestion()
//        {
//            if (_questions != null && _questions.Count > _currentIndex)
//            {
//                var question = _questions[_currentIndex];
//                QuestionText = question.question;

//                // 1. Sammle alle Items zuerst in einer Liste
//                var optionsList = new List<AnswerOption>();
//                for (int i = 0; i < question.options.Count; i++)
//                {
//                    optionsList.Add(new AnswerOption
//                    {
//                        Text = question.options[i],
//                        //IsCorrect = (i == question.correctIndex),
//                        CorrectIndex = question.correctIndex,
//                        Index = i,
//                        BackgroundColor = _themePrimaryColor //Colors.LightGray
//                    });
//                }

//                // 2. Weise die komplette Liste auf einmal zu
//                Options = new ObservableCollection<AnswerOption>(optionsList);

//                ProgressText = $"Frage {_currentIndex + 1} von {_questions.Count}";
//            }
//        }

//        private void LoadNextQuestion()
//        {
//            _currentIndex++;
//            //_isAnswered = false;

//            if (_currentIndex >= _questions.Count)
//            {
//                FinishQuiz();
//                return;
//            }

//            var q = _questions[_currentIndex];
//            QuestionText = q.question;
//            ProgressText = $"Frage {_currentIndex + 1} von {_questions.Count}";

//            Options.Clear();
//            for (int i = 0; i < q.options.Count; i++)
//            {
//                var option = new AnswerOption();
//                // Wichtig: SetValues verwenden!
//                option.SetValues(q.options[i], q.correctIndex, i);
//                option.BackgroundColor = _themePrimaryColor; // Theme-Farbe verwenden
//                option.IsEnabled = true;
//                Options.Add(option);
//            }
//        }

//        private async void FinishQuiz()
//        {
//            // Ergebnis-Seite oder Popup
//            await Application.Current.MainPage.DisplayAlert("Quiz beendet!",
//                $"Du hast {_score} von {_questions.Count} Fragen richtig beantwortet.",
//                "Ok");

//            // Zurück zur Startseite oder Output-Seite
//            await Shell.Current.GoToAsync(".."); // Oder Navigation.PopAsync()
//        }


//    }

//    public partial class AnswerOption : ObservableObject
//    {
//        [ObservableProperty]
//        private bool isEnabled = true;

//        [ObservableProperty]
//        private string text;

//        [ObservableProperty]
//        private string textCorrect = string.Empty;

//        [ObservableProperty]
//        private int correctIndex;

//        [ObservableProperty]
//        private Color backgroundColor;

//        [ObservableProperty]
//        private int index;

//        // Flag um rekursive Aufrufe zu verhindern
//        private bool _isUpdating;

//        partial void OnTextChanged(string value)
//        {
//            if (!_isUpdating) UpdateTextCorrect();
//        }

//        partial void OnCorrectIndexChanged(int value)
//        {
//            if (!_isUpdating) UpdateTextCorrect();
//        }

//        partial void OnIndexChanged(int value)
//        {
//            if (!_isUpdating) UpdateTextCorrect();
//        }

//        private void UpdateTextCorrect()
//        {
//            _isUpdating = true;

//            if (correctIndex == index)
//            {
//                TextCorrect = Text + "."; // Richtiges Häkchen-Symbol
//            }
//            else
//            {
//                TextCorrect = Text;
//            }

//            _isUpdating = false;
//        }

//        // Optional: Methode zum Setzen aller Werte auf einmal
//        public void SetValues(string text, int correctIdx, int idx)
//        {
//            _isUpdating = true;
//            Text = text;
//            CorrectIndex = correctIdx;
//            Index = idx;
//            UpdateTextCorrect();
//            _isUpdating = false;
//        }
//    }


//}
