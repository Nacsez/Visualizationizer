using Microsoft.Xna.Framework;

public class AppState
{
    public float AmplitudeSlider { get; set; } = 0.5f;
    public float CutoffSlider { get; set; } = 0.5f;
    public float FftSlider { get; set; } = 0.5f;
    public float SvgScaleSlider { get; set; } = 0.6f;
    public float SvgPerturbationSlider { get; set; } = 0.2f;
    public int ModeIndex { get; set; } = 3;
    public bool[] ColorToggles { get; set; } = new bool[33];
    public string LoadedMediaPath { get; set; } = string.Empty;
    public float SvgPositionX { get; set; }
    public float SvgPositionY { get; set; }
    public int FftLength { get; set; } = 64;

    public Vector2 GetSvgPosition()
    {
        return new Vector2(SvgPositionX, SvgPositionY);
    }

    public void SetSvgPosition(Vector2 position)
    {
        SvgPositionX = position.X;
        SvgPositionY = position.Y;
    }
}
