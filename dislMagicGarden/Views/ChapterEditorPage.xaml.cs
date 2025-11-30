using dislMagicGarden.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui; // Hinzugefügt für SKPaintSurfaceEventArgs

namespace dislMagicGarden.Views;

public partial class ChapterEditorPage : ContentPage
{
    ChapterEditorViewModel ViewModel => BindingContext as ChapterEditorViewModel;

    public ChapterEditorPage(ChapterEditorViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        // Grafik neu rendern, wenn Striche kommen
        ViewModel.PropertyChanged += (_, __) => CanvasView.InvalidateSurface();
    }

    // --------------------------------------
    //  ?? 1. Zeichenlogik (Rendering)                       
    // --------------------------------------
    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (ViewModel.BaseBitmap != null)
        {
            canvas.DrawBitmap(ViewModel.BaseBitmap, 0, 0);
        }

        // Striche zeichnen
        foreach (var stroke in ViewModel.Strokes)
        {
            using var paint = new SKPaint
            {
                Color = stroke.Color,
                StrokeWidth = stroke.StrokeWidth,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var p1 = stroke.Points[i - 1];
                var p2 = stroke.Points[i];
                canvas.DrawLine(p1, p2, paint);
            }
        }
    }

    // --------------------------------------
    //  ?? 2. Touch Events (Kinder malen)
    // --------------------------------------
    private void OnCanvasTouch(object sender, SKTouchEventArgs e)
    {
        var p = e.Location;

        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                ViewModel.StartStroke(p.X, p.Y);
                break;

            case SKTouchAction.Moved:
                if (e.InContact)
                    ViewModel.ContinueStroke(p);
                break;

            case SKTouchAction.Released:
                ViewModel.EndStroke();
                break;
        }

        CanvasView.InvalidateSurface();
        e.Handled = true;
    }
}
