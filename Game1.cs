
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
//using SharpDX.Direct2D1;
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
    private Texture2D sidebarTexture, sliderTexture2, sliderTexture3;
    private Texture2D closeButtonTexture;
    private int sidebarWidth = 250;
    private Texture2D sliderTexture;
    private Vector2 sliderPosition, sliderPosition2, sliderPosition3;
    private float sliderValue, sliderValue2 = 0.5f, sliderValue3 = 0.5f;
    //private int fftBinCount = 256; // Default bin count, assuming power of 2
    private float frequencyCutoff = 0.5f; // Default cutoff at half of the frequency range
    private float minScaleFactor = 0.0001f;
    private float maxScaleFactor = 0.01f;
    private TimeSpan lastUpdate = TimeSpan.Zero;
    private TimeSpan lastButtonPressTime;
    private const double UpdateThreshold = 0.1; // seconds
    private Color[] colors = new Color[]
{
    // Reds
    Color.LightSalmon, Color.Red, Color.DarkRed,
    // Oranges
    Color.PeachPuff, Color.Orange, Color.DarkOrange,
    // Yellows
    Color.LightYellow, Color.Yellow, Color.Goldenrod,
    // Greens
    Color.LightGreen, Color.Green, Color.DarkGreen,
    // Blues
    Color.LightBlue, Color.Blue, Color.DarkBlue,
    // Indigos
    Color.MediumSlateBlue, Color.Indigo, Color.MidnightBlue,
    // Violets
    Color.Plum, Color.Violet, Color.DarkViolet,
    // Greys
    Color.LightGray, Color.Gray, Color.DarkSlateGray
};
    private bool[] colorToggles = new bool[24]; // Adjust to 24 toggles
    private Rectangle[] colorButtons = new Rectangle[24];
    private Texture2D colorButtonTexture;
    private Texture2D svgTexture;
    private Rectangle svgButtonRect;

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
        graphics.PreferredBackBufferWidth = 1200; // initial width
        graphics.PreferredBackBufferHeight = 900; // initial height
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
        sidebarArea = new Rectangle(0, 0, sidebarWidth, GraphicsDevice.Viewport.Height);
        closeButtonRect = new Rectangle(10, 10, 80, 30);
        svgButtonRect = new Rectangle(100, 10, 80, 30); // Positioned 100 pixels from the left
        int buttonSize = 40;  // Size of each color toggle button
        int padding = 15;     // Padding between buttons
        int startX = 40;      // Starting X offset
        int startY = 300;     // Starting Y offset from the top of the sidebar
        int rows = 8; // Eight rows
        int columns = 3; // Three columns
        colorButtons = new Rectangle[rows * columns]; // Initialize for 24 buttons

        for (int i = 0; i < colorButtons.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            int x = startX + col * (buttonSize + padding);
            int y = startY + row * (buttonSize + padding);
            colorButtons[i] = new Rectangle(x, y, buttonSize, buttonSize);
        }

        colorButtonTexture = CreateColorTexture(buttonSize, buttonSize, Color.White);  // White used as a mask for color

        visualizer = new AudioVisualizer(GraphicsDevice, spriteBatch)
        {
            Colors = colors,
            ColorToggles = colorToggles
        };
        
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
            HandleSlider(mouse, sliderPosition, sliderTexture, ref sliderValue, 0f, 1f, v => {
                float scale = minScaleFactor + (maxScaleFactor - minScaleFactor) * v;
                visualizer.UpdateScaleFactor(scale);
            });
            HandleSlider(mouse, sliderPosition2, sliderTexture2, ref sliderValue2, 0f, 1f, v => {
                frequencyCutoff = v;
                visualizer.UpdateFrequencyCutoff(frequencyCutoff);
            });
            HandleSlider(mouse, sliderPosition3, sliderTexture3, ref sliderValue3, 0f, 1f, v =>
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

            // Handle color button interactions
            HandleColorButtonInteraction(mouse, gameTime);
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
        if (svgTexture != null)
        {
            Debug.WriteLine("SVG Texture is ready to be drawn.");
        }
        else
        {
            Debug.WriteLine("SVG Texture not ready yet.");
        }

        //if (svgTexture != null)
        //{
        //    spriteBatch.Draw(svgTexture, new Vector2(400, 200), Color.White); // Adjust position as needed
        //}
        base.Update(gameTime);
        
    }

    protected override void Draw(GameTime gameTime)
    {
        
        GraphicsDevice.Clear(Color.Black);

        // Update and draw the frequency data visualization
        visualizer.UpdateFrequencyData(audioManager.FrequencyData);
        visualizer.Draw(audioManager.FrequencyData);

        // Begin drawing sprites
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
        if (svgTexture != null)
        {
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the desired size to take up the center of the screen
            int desiredWidth = 2*viewportWidth / 3;
            int desiredHeight = 2*viewportHeight / 3;

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

            // Calculate the position to center the SVG
            int posX = (viewportWidth - desiredWidth) / 2;
            int posY = (viewportHeight - desiredHeight) / 2;

            spriteBatch.Draw(svgTexture, new Rectangle(posX, posY, desiredWidth, desiredHeight), Color.White);

            Debug.WriteLine($"Drawing SVG at {posX},{posY}");
            //Debug.WriteLine("Drawing SVG at 100,100");

        }
        if (sidebarVisible)
        {
            // Draw the sidebar background
            spriteBatch.Draw(sidebarTexture, sidebarArea, Color.White);

            // Draw the close button and svg load button
            spriteBatch.Draw(closeButtonTexture, closeButtonRect, Color.White);
            spriteBatch.Draw(closeButtonTexture, svgButtonRect, Color.White); // Reuse the close button texture or create a new one

            // Draw the sliders
            spriteBatch.Draw(sliderTexture, new Rectangle(sliderPosition.ToPoint(), new Point((int)(sliderTexture.Width * sliderValue), sliderTexture.Height)), Color.White);
            spriteBatch.Draw(sliderTexture2, new Rectangle(sliderPosition2.ToPoint(), new Point((int)(sliderTexture2.Width * sliderValue2), sliderTexture2.Height)), Color.White);
            spriteBatch.Draw(sliderTexture3, new Rectangle(sliderPosition3.ToPoint(), new Point((int)(sliderTexture3.Width * sliderValue3), sliderTexture3.Height)), Color.White);

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
    }

    public void Initialize(int fftLength = 512)
    {
        this.fftLength = fftLength;
        m = (int)Math.Log(fftLength, 2.0);
        fftBuffer = new Complex[fftLength];

        try
        {
            int deviceNumber = SelectAudioInputDevice();
            waveIn = new WaveInEvent
            {
                DeviceNumber = deviceNumber,
                WaveFormat = new WaveFormat(44100, 1)  // Mono channel, CD quality
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
    public void UpdateActiveColors(Color[] allColors, bool[] toggles)
    {
        ActiveColors.Clear();
        for (int i = 0; i < allColors.Length; i++)
        {
            if (toggles[i])
                ActiveColors.Add(allColors[i]);
        }
    }
    public AudioVisualizer(GraphicsDevice graphicsDevice, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
    {
        this.graphicsDevice = graphicsDevice;
        this.spriteBatch = spriteBatch;
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new Color[] { Color.White });  // White pixel for drawing
    }

    public void UpdateScaleFactor(float scaleFactor)
    {
        // This would adjust how the visualization scaling should work, placeholder for now
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
            float magnitude = frequencyData[i];
            //Debug.WriteLine($"Magnitude[{i}]: {magnitude}");  // Output magnitude to debug
            float normalizedMagnitude = Math.Min(1f, magnitude * 10); // Scale magnitude to a max of 1
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

}   