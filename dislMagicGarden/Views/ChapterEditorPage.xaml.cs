namespace dislMagicGarden.Views;

public partial class ChapterEditorPage : ContentPage
{
	public ChapterEditorPage()
	{
		InitializeComponent();
	}

    void OnDraw(ICanvas canvas, RectF rect)
    {
        // 1. Basisbild zeichnen
        canvas.DrawImage(_baseImage, 0, 0, _baseImage.Width, _baseImage.Height);

        // 2. Striche des Kindes zeichnen
        foreach (var stroke in ViewModel.Strokes)
        {
            canvas.StrokeColor = Color.FromUint(stroke.Color);
            canvas.StrokeSize = stroke.StrokeWidth;

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var p1 = stroke.Points[i - 1];
                var p2 = stroke.Points[i];
                canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
            }
        }
    }

}