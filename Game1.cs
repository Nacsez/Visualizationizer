
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NAudio.Wave;
using System;
using System.Linq;
using NAudio.Dsp;  // Include the DSP namespace for FFT
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using Svg;

public class Vizualizationizer : Game
{
    private GraphicsDeviceManager graphics;
    private Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch;
    private Rectangle sidebarArea;
    private Rectangle closeButtonRect;
    private AudioVisualizer visualizer;
    private AudioManager audioManager;
    private bool sidebarVisible;
    private Texture2D sidebarTexture, sliderTexture2, sliderTexture3, sliderTexture4, sliderTexture5;
    private Texture2D closeButtonTexture;
    private int sidebarWidth = 260;
    private Texture2D sliderTexture;
    private Vector2 sliderPosition, sliderPosition2, sliderPosition3, sliderPosition4, sliderPosition5;
    private float sliderValue = 0.5f, sliderValue2 = 0.5f, sliderValue3 = 0.5f;
    private float frequencyCutoff = 0.5f; // Default cutoff at half of the frequency range
    private float minScaleFactor = 0.01f;
    private float maxScaleFactor = 1f;
    private float svgScaleSliderValue = 1.0f; // Start at 100%
    private float svgBaseScale = 1.0f; // Base scale for the SVG texture
    private float perturbationSliderValue = 0.0f; // Start at zero for no purterbation movement
    private TimeSpan lastUpdate = TimeSpan.Zero;
    private TimeSpan lastButtonPressTime;
    private const double UpdateThreshold = 0.1; // seconds
    private Color[] colors = new Color[]
{
    // Colors availabile in the color matrix. Colors appear in the listed order in the grid which is displayed in the sidebar menu
    Color.Salmon, Color.Red, Color.DarkRed,
    Color.Goldenrod, Color.Orange, Color.DarkOrange,
    Color.Gold, Color.Yellow, Color.DarkGoldenrod,
    Color.LightGreen, Color.Chartreuse, Color.DarkGreen,
    Color.SpringGreen, Color.Green, Color.DarkOliveGreen,
    Color.Aqua, Color.DodgerBlue, Color.DarkBlue,
    Color.Plum, Color.Blue, Color.MidnightBlue,
    Color.Violet, Color.DarkOrchid, Color.Indigo,
    Color.PeachPuff, Color.MediumSlateBlue, Color.BlueViolet,
    Color.LightYellow, Color.Silver, Color.Gray,
    Color.MintCream, Color.DarkGray, Color.DimGray
};
    private bool[] colorToggles = new bool[33]; // Adjust to 33 toggles
    private Rectangle[] colorButtons = new Rectangle[33];
    private Texture2D colorButtonTexture;
    private Texture2D svgTexture;
    private Rectangle svgButtonRect;
    private Rectangle[] modeButtons = new Rectangle[4];
    private Texture2D modeButtonTexture;
    private string[] modeLabels = { "Standard", "MirroredMiddle", "MirroredCorners", "Radial" };
    private Vector2 svgPosition;
    private Vector2 svgDragOffset;
    private bool isDraggingSvg = false;
    public Vizualizationizer()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.SynchronizeWithVerticalRetrace = true;  // Sync with monitor refresh rate
        IsFixedTimeStep = true;  // Enable fixed time steps
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 60.0);  // 60 FPS
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        sidebarVisible = false;
        Window.AllowUserResizing = true;
        graphics.HardwareModeSwitch = false; // Keep the window borderless on full screen toggle
        graphics.PreferredBackBufferWidth = 1400; // initial width
        graphics.PreferredBackBufferHeight = 1200; // initial height
        Window.ClientSizeChanged += OnResize;
    }

    private Texture2D CreateColorTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(GraphicsDevice, width, height);
        Color[] colorData = new Color[width * height];
        for (int i = 0; i < colorData.Length; ++i)
        {
            colorData[i] = color;
        }
        texture.SetData(colorData);
        return texture;
    }

    protected override void Initialize()
    {
        base.Initialize();
        sliderPosition = new Vector2(20, 200);
        sliderPosition2 = new Vector2(20, 100);
        sliderPosition3 = new Vector2(20, 150);
        sliderPosition4 = new Vector2(20, 250);
        sliderPosition5 = new Vector2(20, 300); 
        svgPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        sidebarArea = new Rectangle(0, 0, sidebarWidth, GraphicsDevice.Viewport.Height);
        closeButtonRect = new Rectangle(10, 10, 80, 30);
        svgButtonRect = new Rectangle(100, 10, 80, 30); // Positioned 100 pixels from the left
        int buttonSize = 40;  // Size of each color toggle button
        int padding = 15;     // Padding between buttons
        int startX = 40;      // Starting X offset
        int startY = 350;     // Starting Y offset from the top of the sidebar
        int rows = 11; // Eight rows
        int columns = 3; // Three columns
        colorButtons = new Rectangle[rows * columns]; // Initialize for 33 buttons

        for (int i = 0; i < colorButtons.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            int x = startX + col * (buttonSize + padding);
            int y = startY + row * (buttonSize + padding);
            colorButtons[i] = new Rectangle(x, y, buttonSize, buttonSize);
        }

        colorButtonTexture = CreateColorTexture(buttonSize, buttonSize, Color.White);  // White used as a mask for color

        int modeButtonWidth = 60;
        int modeButtonHeight = 30;
        int modeStartY = 1000; // Adjust this value based on your actual layout
        int modePadding = 25;

        modeButtonTexture = CreateColorTexture(modeButtonWidth, modeButtonHeight, Color.White); // You can customize this color or texture later

        for (int i = 0; i < modeButtons.Length; i++)
        {
            int row = i / 2; // Arrange in two rows
            int col = i % 2; // Two columns
            modeButtons[i] = new Rectangle(40 + col * (modeButtonWidth + modePadding), modeStartY + row * (modeButtonHeight + modePadding), modeButtonWidth, modeButtonHeight);
        }
        visualizer = new AudioVisualizer(GraphicsDevice, spriteBatch)
        {
            Colors = colors,
            ColorToggles = colorToggles
        };
        svgPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
    }
    private void OpenSvgFileDialog()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "SVG Files (*.svg)|*.svg",
            Title = "Open SVG File"
        };
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            // Load the SVG file here or set the path to be used later
            // Assuming you have a method to process and set the SVG texture
            svgTexture = LoadSvgTexture(openFileDialog.FileName);
        }
    }

    private Texture2D LoadSvgTexture(string filePath)
    {
        SvgDocument svgDocument = SvgDocument.Open(filePath);
        int width = (int)svgDocument.Width.Value;
        int height = (int)svgDocument.Height.Value;

        // Check for viewBox attribute and adjust dimensions accordingly
        if (svgDocument.ViewBox != SvgViewBox.Empty)
        {
            width = (int)svgDocument.ViewBox.Width;
            height = (int)svgDocument.ViewBox.Height;
        }

        // Use a higher resolution for the bitmap
        const int resolutionMultiplier = 4;
        width *= resolutionMultiplier;
        height *= resolutionMultiplier;

        // Ensure minimum dimensions to avoid issues
        width = Math.Max(width, 1);
        height = Math.Max(height, 1);

        using (var bitmap = new System.Drawing.Bitmap(width, height))
        {
            using (var gfx = System.Drawing.Graphics.FromImage(bitmap))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gfx.Clear(System.Drawing.Color.Transparent);

                // Scale the graphics object to match the higher resolution
                gfx.ScaleTransform(resolutionMultiplier, resolutionMultiplier);

                svgDocument.Draw(gfx);
            }
            return CreateTextureFromBitmap(graphics.GraphicsDevice, bitmap);
        }
    }
    private static Texture2D CreateTextureFromBitmap(GraphicsDevice device, System.Drawing.Bitmap bitmap)
    {
        Texture2D texture = new Texture2D(device, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);
        var buffer = new byte[bitmap.Width * bitmap.Height * 4];

        var bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, buffer.Length);
        bitmap.UnlockBits(bitmapData);

        // Swap the red and blue channels
        for (int i = 0; i < buffer.Length; i += 4)
        {
            byte temp = buffer[i];        // Store blue channel
            buffer[i] = buffer[i + 2];    // Assign red to blue
            buffer[i + 2] = temp;         // Assign stored blue to red
        }

        texture.SetData(buffer);
        return texture;
    }
    protected override void LoadContent()
    {
        spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);
        visualizer = new AudioVisualizer(GraphicsDevice, spriteBatch);
        audioManager = new AudioManager();
        audioManager.Initialize();
        sidebarTexture = CreateColorTexture(sidebarWidth, GraphicsDevice.Viewport.Height, new Color(123, 104, 238));
        closeButtonTexture = CreateColorTexture(80, 30, new Color(245, 245, 245));
        sliderTexture = CreateColorTexture(220, 20, Color.Gray);
        sliderTexture2 = CreateColorTexture(220, 20, Color.Gray);
        sliderTexture3 = CreateColorTexture(220, 20, Color.Gray);
        sliderTexture4 = CreateColorTexture(220, 20, Color.Gray);
        sliderTexture5 = CreateColorTexture(220, 20, Color.Gray);
    }

    private void OnResize(object sender, EventArgs e)
    {
        graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        graphics.ApplyChanges();
        sidebarArea.Height = GraphicsDevice.Viewport.Height;
    }

    protected override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        if (!sidebarVisible && mouse.X <= 5)
        {
            sidebarVisible = true;
        }
        else if (sidebarVisible && closeButtonRect.Contains(mouse.X, mouse.Y) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        {
            sidebarVisible = false;
        }
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i].Contains(mouse.Position) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && lastButtonPressTime + TimeSpan.FromMilliseconds(250) < gameTime.TotalGameTime)
            {
                colorToggles[i] = !colorToggles[i];
                visualizer.UpdateActiveColors(colors, colorToggles);  // Update active colors in visualizer
                lastButtonPressTime = gameTime.TotalGameTime;
                break;
            }
        }
        if (sidebarVisible)
        {
            // Handle slider interactions
            HandleSlider(mouse, sliderPosition, sliderTexture, ref sliderValue, 0.1f, 1f, v =>
            {
                float scaledValue = 0.25f + (100f - 0.25f) * v;  // Assuming v is between 0 and 1
                visualizer.UpdateScaleFactor(scaledValue);
                Debug.WriteLine($"Adjusted Scale Factor: {scaledValue}");
                Debug.WriteLine($"Scale factor set to {v}");  // Add this to confirm the scale factor is being updated
            });
            HandleSlider(mouse, sliderPosition2, sliderTexture2, ref sliderValue2, 0.1f, 1f, v => {
                frequencyCutoff = v;
                visualizer.UpdateFrequencyCutoff(frequencyCutoff);
            });
            HandleSlider(mouse, sliderPosition3, sliderTexture3, ref sliderValue3, 0.1f, 1f, v =>
            {
                // Map from 0.0-1.0 range to 6-10, representing 2^6=64 to 2^10=1024
                int exponent = 6 + (int)((10 - 6) * v);
                int newFFTLength = 1 << exponent;  // Convert to power of 2 based on slider position
                if (newFFTLength != audioManager.FftLength)
                {
                    audioManager.UpdateFFTLength(newFFTLength);
                    visualizer.UpdateFFTBinCount(newFFTLength / 2);
                }
            });
            HandleSlider(mouse, sliderPosition4, sliderTexture, ref svgScaleSliderValue, 0.01f, 1f, v =>
            {
                // This will update the SVG scale factor
                svgScaleSliderValue = v;
            });
            HandleSlider(mouse, sliderPosition5, sliderTexture5, ref perturbationSliderValue, 0.0f, 1.0f, v =>
            {
                perturbationSliderValue = v;
                Debug.WriteLine($"Perturbation Slider Value: {perturbationSliderValue}");
            });
            // Handle color button interactions
            HandleColorButtonInteraction(mouse, gameTime);
            for (int i = 0; i < modeButtons.Length; i++)
            {
                if (modeButtons[i].Contains(mouse.Position) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && lastButtonPressTime + TimeSpan.FromMilliseconds(250) < gameTime.TotalGameTime)
                {
                    visualizer.Mode = (AudioVisualizer.VisualizationMode)i;
                    lastButtonPressTime = gameTime.TotalGameTime;
                }
            }
        }
        if (sidebarVisible && svgButtonRect.Contains(mouse.X, mouse.Y) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && lastButtonPressTime + TimeSpan.FromMilliseconds(250) < gameTime.TotalGameTime)
        {
            lastButtonPressTime = gameTime.TotalGameTime;
            // Call the method to open the SVG file dialog
            OpenSvgFileDialog();
        }
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11))
        {
            graphics.ToggleFullScreen();
        }
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            Exit();  // This will close the application
        }
        // Check for Delete key press to drop the SVG texture
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete))
        {
            svgTexture = null; // Clear the SVG texture
        }
        if (svgTexture != null)
        {
            Debug.WriteLine("SVG Texture is ready to be drawn.");
            if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                if (!isDraggingSvg)
                {
                    // Start dragging if the mouse is over the SVG texture
                    int desiredWidth = (int)((GraphicsDevice.Viewport.Width / 3) * svgScaleSliderValue);
                    int desiredHeight = (int)((GraphicsDevice.Viewport.Height / 3) * svgScaleSliderValue);
                    float svgAspectRatio = (float)svgTexture.Width / svgTexture.Height;
                    if ((float)desiredWidth / desiredHeight > svgAspectRatio)
                    {
                        desiredWidth = (int)(desiredHeight * svgAspectRatio);
                    }
                    else
                    {
                        desiredHeight = (int)(desiredWidth / svgAspectRatio);
                    }

                    int posX = (int)(svgPosition.X - desiredWidth / 2);
                    int posY = (int)(svgPosition.Y - desiredHeight / 2);
                    Rectangle svgRectangle = new Rectangle(posX, posY, desiredWidth, desiredHeight);

                    if (svgRectangle.Contains(mouse.Position))
                    {
                        isDraggingSvg = true;
                        svgDragOffset = svgPosition - mouse.Position.ToVector2();
                    }
                }
                else
                {
                    // Update the position while dragging
                    svgPosition = mouse.Position.ToVector2() + svgDragOffset;
                }
            }
            else
            {
                // Stop dragging
                isDraggingSvg = false;
            }
            visualizer.UpdateFrequencyData(audioManager.FrequencyData);
        }
        else
        {
            Debug.WriteLine("SVG Texture not ready yet.");
        }

        base.Update(gameTime);
        
    }

    protected override void Draw(GameTime gameTime)
    {
        
        GraphicsDevice.Clear(Color.Black);

        // Update and draw the frequency data visualization
        visualizer.UpdateFrequencyData(audioManager.FrequencyData);
        visualizer.Draw(audioManager.FrequencyData);
        // Get the sum of the FFT values for perturbation effect
        float amplitudeSum = 0;
        if (audioManager.FrequencyData != null)
        {
            amplitudeSum = visualizer.GetAmplitudeSum(audioManager.FrequencyData);
        }
        // Normalize the amplitude sum and apply the perturbation slider value
        float maxPossibleSum = audioManager.FrequencyData.Length * 1.0f; // Adjust this based on your expected data range
        float normalizedAmplitudeSum = amplitudeSum / maxPossibleSum;
        float amplitudeFactor = MathHelper.Clamp(1.0f + (normalizedAmplitudeSum) * 100.0f * perturbationSliderValue, 0.0f, 1000.0f);

        // Begin drawing sprites
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
        if (svgTexture != null)
        {
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the desired size based on the scale slider value
            int baseWidth = 2 * viewportWidth / 3;
            int baseHeight = 2 * viewportHeight / 3;
            int desiredWidth = (int)(baseWidth * svgScaleSliderValue * amplitudeFactor);
            int desiredHeight = (int)(baseHeight * svgScaleSliderValue * amplitudeFactor);

            float svgAspectRatio = (float)svgTexture.Width / svgTexture.Height;
            if ((float)desiredWidth / desiredHeight > svgAspectRatio)
            {
                // Adjust width to maintain aspect ratio
                desiredWidth = (int)(desiredHeight * svgAspectRatio);
            }
            else
            {
                // Adjust height to maintain aspect ratio
                desiredHeight = (int)(desiredWidth / svgAspectRatio);
            }

            // Calculate the position of the SVG
            int posX = (int)(svgPosition.X - desiredWidth / 2);
            int posY = (int)(svgPosition.Y - desiredHeight / 2);

            spriteBatch.Draw(svgTexture, new Rectangle(posX, posY, desiredWidth, desiredHeight), Color.White);

            Debug.WriteLine($"Drawing SVG at {posX},{posY} with Amplitude Factor: {amplitudeFactor}"); // Add debug line to check the amplitude factor

        }
        if (sidebarVisible)
        {
            // Draw the sidebar background
            spriteBatch.Draw(sidebarTexture, sidebarArea, Color.White);

            // Draw the close button and svg load button
            spriteBatch.Draw(closeButtonTexture, closeButtonRect, Color.White);
            spriteBatch.Draw(closeButtonTexture, svgButtonRect, Color.White); // Reuse the close button texture or create a new one
            for (int i = 0; i < modeButtons.Length; i++)
            {
                spriteBatch.Draw(modeButtonTexture, modeButtons[i], Color.White);
                // Optionally, draw labels here if you have a sprite font loaded
            }
            // Draw the sliders
            spriteBatch.Draw(sliderTexture, new Rectangle(sliderPosition.ToPoint(), new Point((int)(sliderTexture.Width * sliderValue), sliderTexture.Height)), Color.White);
            spriteBatch.Draw(sliderTexture2, new Rectangle(sliderPosition2.ToPoint(), new Point((int)(sliderTexture2.Width * sliderValue2), sliderTexture2.Height)), Color.White);
            spriteBatch.Draw(sliderTexture3, new Rectangle(sliderPosition3.ToPoint(), new Point((int)(sliderTexture3.Width * sliderValue3), sliderTexture3.Height)), Color.White);
            spriteBatch.Draw(sliderTexture, new Rectangle(sliderPosition4.ToPoint(), new Point((int)(sliderTexture.Width * svgScaleSliderValue), sliderTexture.Height)), Color.White);
            spriteBatch.Draw(sliderTexture5, new Rectangle(sliderPosition5.ToPoint(), new Point((int)(sliderTexture5.Width * perturbationSliderValue), sliderTexture5.Height)), Color.White);
            // Draw the color toggle buttons
            for (int i = 0; i < colorButtons.Length; i++)
            {
                Color drawColor = colorToggles[i] ? colors[i] : Color.Black; // Use black when toggled off
                spriteBatch.Draw(colorButtonTexture, colorButtons[i], drawColor);
            }
        }
        
        else
        {
            Debug.WriteLine("SVG Texture is null");
        }
        // End the sprite batch operations
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void HandleColorButtonInteraction(MouseState mouse, GameTime gameTime)
    {
        if (lastButtonPressTime + TimeSpan.FromMilliseconds(250) <= gameTime.TotalGameTime)  // Debouncing to avoid multiple rapid toggles
        {
            for (int i = 0; i < colorButtons.Length; i++)
            {
                if (colorButtons[i].Contains(mouse.Position) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    colorToggles[i] = !colorToggles[i];  // Toggle the color's availability
                    visualizer.ColorToggles = colorToggles;  // Update color toggles in visualizer
                    lastButtonPressTime = gameTime.TotalGameTime;
                    break;  // Avoid multiple button interactions in the same frame
                }
            }
        }
    }
    private void HandleSlider(MouseState mouse, Vector2 sliderPos, Texture2D sliderTex, ref float sliderVal, float minVal, float maxVal, Action<float> updateAction)
    {
        Rectangle sliderRect = new Rectangle(sliderPos.ToPoint(), new Point(sliderTex.Width, sliderTex.Height));
        if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && sliderRect.Contains(mouse.Position))
        {
            sliderVal = (mouse.X - sliderPos.X) / (float)sliderTex.Width;
            sliderVal = MathHelper.Clamp(sliderVal, minVal, maxVal);
            updateAction(sliderVal);
        }
    }
}


public class AudioManager
{
    private WaveInEvent waveIn;
    private Complex[] fftBuffer;
    public float[] FrequencyData { get; private set; }
    private int fftLength = 512;  // Must be a power of two
    private int m;  // Log base 2 of fftLength

    public int FftLength => fftLength;

    public AudioManager()
    {
        m = (int)Math.Log(fftLength, 2.0);
        fftBuffer = new Complex[fftLength];
        FrequencyData = new float[fftLength / 2]; // Initialize FrequencyData to avoid null references
    }

    public void Initialize(int fftLength = 512)
    {
        this.fftLength = fftLength;
        m = (int)Math.Log(fftLength, 2.0);
        fftBuffer = new Complex[fftLength];
        FrequencyData = new float[fftLength / 2]; // Reinitialize FrequencyData with the correct size
        try
        {
            int deviceNumber = SelectAudioInputDevice();
            waveIn = new WaveInEvent
            {
                DeviceNumber = deviceNumber,
                WaveFormat = new WaveFormat(28050, 1)  // Mono channel, CD quality
            };

            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += (sender, args) => Debug.WriteLine("Recording stopped.");
            waveIn.StartRecording();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing audio device: {ex.Message}");
        }
    }

    private int SelectAudioInputDevice()
    {
        int selectedDevice = 0;  // Default to first device
        // Implement your device selection logic here
        return selectedDevice;
    }

    public void UpdateFFTLength(int newLength)
    {
        if (newLength != fftLength)
        {
            fftLength = newLength;
            m = (int)Math.Log(fftLength, 2.0);
            fftBuffer = new Complex[fftLength];
            FrequencyData = new float[fftLength / 2]; // Reinitialize FrequencyData with the correct size
        }
    }

    private void OnDataAvailable(object sender, WaveInEventArgs args)
    {
        int bufferFilled = ConvertBytesToFloats(args.Buffer, args.BytesRecorded, fftBuffer, fftLength);
        FastFourierTransform.FFT(true, m, fftBuffer);
        ProcessFFTResults();
    }

    private int ConvertBytesToFloats(byte[] buffer, int bytesRecorded, Complex[] fftBuffer, int fftLength)
    {
        int index = 0;
        for (int i = 0; i < bytesRecorded && i / 2 < fftLength; i += 2)
        {
            short sample = (short)((buffer[i + 1] << 8) | buffer[i]);
            fftBuffer[i / 2].X = sample / 32768f; // Convert to floating point
            fftBuffer[i / 2].Y = 0; // Imaginary part is zero
            index = i / 2;
        }
        return index + 1; // Return count of points filled
    }

    private void ProcessFFTResults()
    {
        FrequencyData = new float[fftLength / 2];
        for (int i = 0; i < FrequencyData.Length; i++)
        {
            FrequencyData[i] = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
        }
    }

    public void Stop()
    {
        waveIn?.StopRecording();
        waveIn?.Dispose();
        waveIn = null;
    }
}


public class AudioVisualizer
{
    private GraphicsDevice graphicsDevice;
    private Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch;
    private Texture2D pixel;
    private float[] frequencyData;
    private float frequencyCutoff = 0.5f;
    private int fftBinCount;
    public List<Color> ActiveColors { get; private set; } = new List<Color>();
    public Color[] Colors { get; set; }  // Array of colors
    public bool[] ColorToggles { get; set; }  // Array of color toggles
    private float scaleFactor = 1.0f;  // Default scale factor
    // Method to get the sum of FFT values
    public float GetAmplitudeSum(float[] frequencyData)
    {
        if (frequencyData == null || frequencyData.Length == 0)
            return 0;

        float sum = 0;
        for (int i = 0; i < frequencyData.Length; i++)
        {
            sum += frequencyData[i];
        }
        Debug.WriteLine($"Amplitude Sum: {sum}");
        return sum;
    }

    public void UpdateActiveColors(Color[] allColors, bool[] toggles)
    {
        ActiveColors.Clear();
        for (int i = 0; i < allColors.Length; i++)
        {
            if (toggles[i])
                ActiveColors.Add(allColors[i]);
        }
    }
    public enum VisualizationMode
    {
        Standard,
        MirroredMiddle,
        MirroredCorners,
        Radial
    }
    public VisualizationMode Mode { get; set; } = VisualizationMode.Radial;
    public AudioVisualizer(GraphicsDevice graphicsDevice, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
    {
        this.graphicsDevice = graphicsDevice;
        this.spriteBatch = spriteBatch;
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new Color[] { Color.White });  // White pixel for drawing
    }

    public void UpdateScaleFactor(float newScaleFactor)
    {
        scaleFactor = MathHelper.Clamp(newScaleFactor, 0.25f, 100f);
    }

    public void UpdateFrequencyData(float[] data)
    {
        frequencyData = data;
    }

    public void UpdateFrequencyCutoff(float cutoff)
    {
        frequencyCutoff = cutoff;
    }

    public void UpdateFFTBinCount(int newBinCount)
    {
        fftBinCount = newBinCount;
    }

    public void Draw(float[] frequencyData)
    {
        switch (Mode)
        {
            case VisualizationMode.Standard:
                DrawStandard(frequencyData);
                break;
            case VisualizationMode.MirroredMiddle:
                DrawMirroredMiddle(frequencyData);
                break;
            case VisualizationMode.MirroredCorners:
                DrawMirroredCorners(frequencyData);
                break;
            case VisualizationMode.Radial:
                DrawRadial(frequencyData);
                break;
        }
    }
    private void DrawStandard(float[] frequencyData)
    {
        if (frequencyData == null || frequencyData.Length == 0)
        {
            Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        Debug.WriteLine($"Drawing {frequencyData.Length} bars with cutoff at {frequencyCutoff}");


        int viewportWidth = graphicsDevice.Viewport.Width;
        int barWidth = Math.Max(10, viewportWidth / Math.Max(1, fftBinCount));
        int maxIndex = (int)(frequencyData.Length * frequencyCutoff);
        float maxBarHeight = graphicsDevice.Viewport.Height;

        spriteBatch.Begin();
        for (int i = 0; i < maxIndex && i < frequencyData.Length; i++)
        {
            float magnitude = frequencyData[i] * scaleFactor; //apply scale factor
            //Debug.WriteLine($"Magnitude[{i}]: {magnitude}");  // Output magnitude to debug
            float normalizedMagnitude = Math.Min(magnitude,1.0f); // Scale magnitude to a max of 1
            float barHeight = normalizedMagnitude * maxBarHeight;
            Color color = ActiveColors[i % ActiveColors.Count];
            spriteBatch.Draw(pixel, new Rectangle(i * barWidth, (int)(graphicsDevice.Viewport.Height - barHeight), barWidth, (int)barHeight), color);
            if (ColorToggles[i % Colors.Length])  // Check if the color at this index is enabled
            {
                spriteBatch.Draw(pixel, new Rectangle(i * barWidth, (int)(graphicsDevice.Viewport.Height - barHeight), barWidth, (int)barHeight), Colors[i % Colors.Length]);
            }
        }
        spriteBatch.End();
    }

    private void DrawMirroredMiddle(float[] frequencyData)
    {
        if (frequencyData == null || frequencyData.Length == 0)
        {
            Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        Debug.WriteLine($"Drawing {frequencyData.Length} bars with cutoff at {frequencyCutoff}");

        int viewportWidth = graphicsDevice.Viewport.Width;
        int barWidth = Math.Max(10, viewportWidth / Math.Max(1, fftBinCount));
        int maxIndex = (int)(frequencyData.Length * frequencyCutoff);
        float maxBarHeight = graphicsDevice.Viewport.Height / 2; // Half the height for mirrored effect

        spriteBatch.Begin();
        for (int i = 0; i < maxIndex && i < frequencyData.Length; i++)
        {
            float magnitude = frequencyData[i] * scaleFactor; // Apply scale factor
            float normalizedMagnitude = Math.Min(magnitude, 1.0f); // Clamp magnitude to a max of 1
            float barHeight = normalizedMagnitude * maxBarHeight;

            Color color = ActiveColors[i % ActiveColors.Count];
            int topBarY = (int)(maxBarHeight - barHeight); // Start point for top bars
            int bottomBarY = (int)(maxBarHeight + barHeight); // Start point for bottom bars

            // Draw bottom bars
            spriteBatch.Draw(pixel, new Rectangle(i * barWidth, bottomBarY, barWidth, (int)barHeight), color);

            // Draw top bars mirrored
            spriteBatch.Draw(pixel, new Rectangle(i * barWidth, topBarY, barWidth, (int)barHeight), color);

            // Optional: Draw using different color settings if toggled
            if (ColorToggles[i % Colors.Length])
            {
                spriteBatch.Draw(pixel, new Rectangle(i * barWidth, bottomBarY, barWidth, (int)barHeight), Colors[i % Colors.Length]);
                spriteBatch.Draw(pixel, new Rectangle(i * barWidth, topBarY, barWidth, (int)barHeight), Colors[i % Colors.Length]);
            }
        }
        spriteBatch.End();
    }

    private void DrawMirroredCorners(float[] frequencyData)
    {
        if (frequencyData == null || frequencyData.Length == 0)
        {
            Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        Debug.WriteLine($"Drawing {frequencyData.Length} bars with cutoff at {frequencyCutoff}");

        int viewportWidth = 1 + graphicsDevice.Viewport.Width;
        int viewportHeight = graphicsDevice.Viewport.Height;
        int totalBars = fftBinCount; // Total bars on one side
        int barWidth = Math.Max(10, viewportWidth / Math.Max(1, totalBars * 2)); // Adjust for mirroring from center
        int maxIndex = (int)(frequencyData.Length * frequencyCutoff + 1);
        float maxBarHeight = viewportHeight / 2; // Use half the viewport height for top and bottom

        spriteBatch.Begin();
        for (int i = 0; i < maxIndex; i++)
        {
            float magnitude = frequencyData[i] * scaleFactor; // Apply scale factor
            float normalizedMagnitude = Math.Min(magnitude, 1.0f); // Clamp magnitude to a max of 1
            float barHeight = normalizedMagnitude * maxBarHeight;

            Color color = ActiveColors[i % ActiveColors.Count];

            // Calculate positions so that they remain anchored at the corners
            int leftX = (viewportWidth / 2) - ((totalBars - i) * barWidth);
            int rightX = (viewportWidth / 2) + ((totalBars - i - 1) * barWidth);

            // Draw bars on the bottom from the corners towards the center
            spriteBatch.Draw(pixel, new Rectangle(leftX, viewportHeight - (int)barHeight, barWidth, (int)barHeight), color);
            spriteBatch.Draw(pixel, new Rectangle(rightX, viewportHeight - (int)barHeight, barWidth, (int)barHeight), color);

            // Draw mirrored bars on the top from the corners towards the center
            spriteBatch.Draw(pixel, new Rectangle(leftX, 0, barWidth, (int)barHeight), color);
            spriteBatch.Draw(pixel, new Rectangle(rightX, 0, barWidth, (int)barHeight), color);
        }
        spriteBatch.End();
    }

    private void DrawRadial(float[] frequencyData)
    {
        if (frequencyData == null || frequencyData.Length == 0)
        {
            Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        Debug.WriteLine($"Drawing {frequencyData.Length} bars with cutoff at {frequencyCutoff}");

        int viewportWidth = graphicsDevice.Viewport.Width;
        int viewportHeight = graphicsDevice.Viewport.Height;
        Vector2 center = new Vector2(viewportWidth / 2, viewportHeight / 2);
        float maxRadius = Math.Min(viewportWidth, viewportHeight) / 2; // Max radius of the circle
        int maxIndex = (int)(frequencyData.Length * frequencyCutoff);
        float angleStep = (float)(Math.PI * 2 / fftBinCount); // Total angle divided by the number of bins

        spriteBatch.Begin();
        for (int i = 0; i < maxIndex; i++)
        {
            float magnitude = frequencyData[i] * scaleFactor; // Apply scale factor
            float normalizedMagnitude = Math.Min(magnitude, 1.0f); // Clamp magnitude to a max of 1
            float radius = normalizedMagnitude * maxRadius;

            Color color = ActiveColors[i % ActiveColors.Count];
            float startAngle = i * angleStep;
            float endAngle = startAngle + angleStep;

            // Draw a wedge
            DrawWedge(center, radius, startAngle, endAngle, color);
        }
        spriteBatch.End();
    }

    private void DrawWedge(Vector2 center, float radius, float startAngle, float endAngle, Color color)
    {
        int numSegments = 10; // Number of triangle segments to construct the wedge
        Vector2 startVector = new Vector2((float)Math.Cos(startAngle), (float)Math.Sin(startAngle)) * radius;
        Vector2 prevVector = startVector;
        // Ensure effect is only created once or use an existing effect that is disposed elsewhere
        BasicEffect effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0, 1),
            View = Matrix.Identity,
            World = Matrix.Identity
        };
        for (int i = 1; i <= numSegments; i++)
        {
            float angle = MathHelper.Lerp(startAngle, endAngle, i / (float)numSegments);
            Vector2 newVector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

            // Draw triangle from center to prevVector to newVector
            DrawTriangle(center, center + prevVector, center + newVector, color, effect);

            // Mirror X
            DrawTriangle(center, center + new Vector2(-prevVector.X, prevVector.Y), center + new Vector2(-newVector.X, newVector.Y), color, effect);

            // Mirror Y
            DrawTriangle(center, center + new Vector2(prevVector.X, -prevVector.Y), center + new Vector2(newVector.X, -newVector.Y), color, effect);

            // Mirror both X and Y
            DrawTriangle(center, center + new Vector2(-prevVector.X, -prevVector.Y), center + new Vector2(-newVector.X, -newVector.Y), color, effect);

            prevVector = newVector;
        }
        // Dispose of the effect after use to prevent memory leak
        effect.Dispose();
    }

    private void DrawTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color, BasicEffect effect)
    {
        VertexPositionColor[] vertices = new VertexPositionColor[3];
        vertices[0] = new VertexPositionColor(new Vector3(v1, 0), color);
        vertices[1] = new VertexPositionColor(new Vector3(v2, 0), color);
        vertices[2] = new VertexPositionColor(new Vector3(v3, 0), color);

        // Set your vertex buffer here if necessary or use an existing one

        effect.VertexColorEnabled = true;
        effect.Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0, 1);
        effect.View = Matrix.Identity;
        effect.World = Matrix.Identity;

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 1);
        }
    }
}   