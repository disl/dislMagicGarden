using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dislMagicGarden.Models;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace dislMagicGarden.ViewModels
{
    //[QueryProperty(nameof(Chapter), "Chapter")]
    public partial class ChapterEditorViewModel : BaseViewModel
    {
        private readonly IEditorService _editorService;
        private EditorStroke _currentStroke;

        public ChapterEditorViewModel(IEditorService editorService)
        {
            _editorService = editorService;
            Title = "Bild bearbeiten";
        }

        [ObservableProperty]
        Chapter chapter;

        [ObservableProperty]
        string currentColor = "#FF9ECD"; // rosa Pinsel

        public ObservableCollection<EditorStroke> Strokes { get; } = new();

        [ObservableProperty]
        SKBitmap? baseBitmap;



        [RelayCommand]
        void ChangeColor()
        {
            // Beispiel: Durch Pinsel-Farben toggeln
            CurrentColor = CurrentColor == "#FF9ECD" ? "#C7A5FF" : "#FF9ECD";
        }

        [RelayCommand]
        async Task Save()
        {
            IsBusy = true;

            var editedPath = await _editorService.SaveEditedImageAsync(Chapter);

            Chapter.ImageEditedPath = editedPath;

            IsBusy = false;

            await Shell.Current.DisplayAlert("Gespeichert", "Das Bild wurde gespeichert!", "OK");
            await Shell.Current.GoToAsync(".."); // zurück
        }

        public void StartStroke(float x, float y)
        {
            StartStroke(new SKPoint(x, y));
        }

        public void StartStroke(SKPoint point)
        {
            _currentStroke = new EditorStroke
            {
                Color = SKColor.Parse(CurrentColor),
                StrokeWidth = 6f,
            };
            _currentStroke.Points.Add(point);
        }

        [RelayCommand]
        public void ContinueStroke(SKPoint point)
        {
            _currentStroke?.Points.Add(point);
        }

        [RelayCommand]
        public void EndStroke()
        {
            if (_currentStroke != null)
            {
                Strokes.Add(_currentStroke);
                Chapter.EditorStrokes.Add(_currentStroke);
                _currentStroke = null;
            }
        }

        public async Task LoadAsync()
        {
            BaseBitmap = await _editorService.EditImageAsync(Chapter.EffectiveImagePath);
        }

    }
}
