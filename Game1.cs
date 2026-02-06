 //Welcome to Visualizationizer! I did my best to comment the code. There is always more to do and more that could be done, ya know?
//Debug lines have been commented out for resources. uncomment them as needed
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NAudio.Wave;
using System;
using System.Linq;
using NAudio.Dsp;  // Include for FFT
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using Svg;
using System.IO;

public class Visualizationizer : Game
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
    private float minScaleFactor = 0.05f;
    private float maxScaleFactor = 2f;
    private float svgScaleSliderValue = 0.6f; // Start at 60%
    private float perturbationSliderValue = 0.2f; // Start at 20% for small perturbation movement
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
    private Rectangle[] modeButtons = new Rectangle[6];
    private Texture2D modeButtonTexture;
    private string[] modeLabels = { "Standard", "MirroredMiddle", "MirroredCorners", "Radial", "CenterColumn", "Puddle" };
    private Vector2 svgPosition;
    private Vector2 svgDragOffset;
    private bool isDraggingSvg = false;
    private KeyboardState previousKeyboardState;
    private string loadedMediaPath = string.Empty;
    private AppState appState = new AppState();
    private ProfileManager profileManager = new ProfileManager();
    public Visualizationizer()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.SynchronizeWithVerticalRetrace = true;  // Sync with monitor refresh rate
        IsFixedTimeStep = true;  // Enable fixed time steps
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 60.0);  // 60 FPS
        Content.RootDirectory = "SVGs";
        IsMouseVisible = true;
        sidebarVisible = false;
        Window.AllowUserResizing = true;
        graphics.HardwareModeSwitch = false; // Keep the window borderless on full screen toggle
        graphics.PreferredBackBufferWidth = 1800; // initial width
        graphics.PreferredBackBufferHeight = 1200; // initial height
        Window.ClientSizeChanged += OnResize;
    }
    private Texture2D CreateColorTexture(int width, int height, Color color)
    {
        //Maps rendered colors from available colors
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
        sliderPosition = new Vector2(20, 100);
        sliderPosition2 = new Vector2(20, 150);
        sliderPosition3 = new Vector2(20, 200);
        sliderPosition4 = new Vector2(20, 250);
        sliderPosition5 = new Vector2(20, 300); 
        svgPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        sidebarArea = new Rectangle(0, 0, sidebarWidth, GraphicsDevice.Viewport.Height);
        closeButtonRect = new Rectangle(20, 20, 100, 35);
        svgButtonRect = new Rectangle(135, 20, 100, 35);
        int buttonSize = 45;  // Size of each color toggle button
        int padding = 16;     // Padding between buttons
        int startX = 45;      // Starting X offset
        int startY = 350;     // Starting Y offset from the top of the sidebar
        int rows = 11;
        int columns = 3;
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
        int modeButtonWidth = 90;
        int modeButtonHeight = 35;
        int modeStartY = 1040; // Y adjustment for mode buttons
        int modePadding = 15;
        modeButtonTexture = CreateColorTexture(modeButtonWidth, modeButtonHeight, Color.White); // This space has been left intentionally white for now
        for (int i = 0; i < modeButtons.Length; i++)
        {
            int row = i / 2; // Arrange in three rows
            int col = i % 2; // Two columns
            modeButtons[i] = new Rectangle(30 + col * (modeButtonWidth + modePadding), modeStartY + row * (modeButtonHeight + modePadding), modeButtonWidth, modeButtonHeight);
        }
        LoadDefaultSvg();
        svgPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
    }
    private void OpenSvgFileDialog()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        bool shiftPressed = keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);

        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "SVG Files (*.svg)|*.svg";
            openFileDialog.Title = "Open SVG File";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                svgTexture = LoadSvgTexture(openFileDialog.FileName);
                loadedMediaPath = openFileDialog.FileName;

                if (shiftPressed)
                {
                    System.Windows.Forms.MessageBox.Show("Now writing this SVG file to the SVGs folder as 'startup.svg' for use on startup.", "Setting Default SVG", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetAsDefaultSvg(openFileDialog.FileName);
                }
            }
        }
    }
    private void SetAsDefaultSvg(string filePath)
{
        string svgFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SVGs");
        // Check if the SVGs folder exists, if not, create it
        if (!Directory.Exists(svgFolderPath))
        {
            Directory.CreateDirectory(svgFolderPath);
        }
        string startupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SVGs", "startup.svg");
    try
    {
        File.Copy(filePath, startupPath, true); // Copies and overwrites the existing startup.svg
        //Debug.WriteLine("Default startup SVG set successfully.");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Failed to set default SVG: {ex.Message}");
        System.Windows.Forms.MessageBox.Show($"Failed to set the SVG as default to {AppDomain.CurrentDomain.BaseDirectory}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
    private void LoadDefaultSvg()
    {
        string startupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SVGs", "startup.svg");
        if (File.Exists(startupPath))
        {
            svgTexture = LoadSvgTexture(startupPath);
            loadedMediaPath = startupPath;
            //Debug.WriteLine("Startup SVG loaded successfully.");
        }
        else
        {
            //Debug.WriteLine("Startup SVG not found.");
        }
    }
    private void ApplyInitialSettings()
    {
        // Apply the initial settings to the visualizer and audio manager
        ActivateRandomColors(3);//Turn on 3 Random Colors
        visualizer.UpdateFrequencyData(audioManager.FrequencyData);// Ensure the initial frequency data is processed
        visualizer.UpdateScaleFactor(50);
        visualizer.UpdateFFTBinCount(32);
        //visualizer.UpdateFFTBinCount(0);
    }
    private void ActivateRandomColors(int count)
    {
        Random rand = new Random();
        HashSet<int> selectedIndices = new HashSet<int>();

        while (selectedIndices.Count < count)
        {
            int index = rand.Next(colors.Length);
            selectedIndices.Add(index);
        }

        // Reset all toggles to false initially if needed
        for (int i = 0; i < colorToggles.Length; i++)
        {
            colorToggles[i] = false;
        }

        // Activate randomly selected colors
        foreach (var index in selectedIndices)
        {
            colorToggles[index] = true;
        }

        // Update the visualizer with the new active colors
        visualizer.UpdateActiveColors(colors, colorToggles);
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
        // Bitmap upscaling for better visibility of imported file
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
        // Swap the red and blue channels because differing formats
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
        spriteBatch = new SpriteBatch(GraphicsDevice);
        audioManager = new AudioManager();
        audioManager.Initialize();
        visualizer = new AudioVisualizer(GraphicsDevice, spriteBatch)
        {
            Colors = colors,
            ColorToggles = colorToggles
        };
        ApplyInitialSettings();
        SyncAppStateFromRuntime();
        sidebarTexture = CreateColorTexture(sidebarWidth, GraphicsDevice.Viewport.Height, new Color(100, 100, 244));
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
        // Update any dependent on viewport dimensions
        int viewportWidth = GraphicsDevice.Viewport.Width;
        int viewportHeight = GraphicsDevice.Viewport.Height;
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
            // Slider 1 - Aplitude Scale Factor
            HandleSlider(mouse, sliderPosition, sliderTexture, ref sliderValue, 0.05f, 10f, v =>
            {
                float scaledValue = 1.0f + (100f - 1.0f) * v;  // scaling factor
                visualizer.UpdateScaleFactor(scaledValue);
                //Debug.WriteLine($"Adjusted Scale Factor: {scaledValue}");
                //Debug.WriteLine($"Scale factor set to {v}");  // Add this to confirm the scale factor is being updated
            });
            // Slider 2 - Frequency Cutoff affecting length of fft output
            HandleSlider(mouse, sliderPosition2, sliderTexture2, ref sliderValue2, 0.1f, 1f, v => {
                frequencyCutoff = v;
                visualizer.UpdateFrequencyCutoff(frequencyCutoff);
            });
            // Slider 3 - FFT Bin Selection affecting size of rendered bins. Smaller values are less bins, larger are more bins
            HandleSlider(mouse, sliderPosition3, sliderTexture3, ref sliderValue3, 0.1f, 1f, v =>
            {
                // Map from 0.0-1.0 range to 5-10, representing 2^5=32 to 2^10=1024
                int exponent = 5 + (int)((10 - 5) * v);
                int newFFTLength = 1 << exponent;  // Convert to power of 2 based on slider position
                if (newFFTLength != audioManager.FftLength)
                {
                    audioManager.UpdateFFTLength(newFFTLength);
                    visualizer.UpdateFFTBinCount(newFFTLength / 2);
                }
            });
            // Slider 4 - Size of the imported SVG file
            HandleSlider(mouse, sliderPosition4, sliderTexture, ref svgScaleSliderValue, 0.01f, 1.0f, v =>
            {
                svgScaleSliderValue = v;
            });
            // Slider 5 - Perturbation factor for imported SVG file. Zero value stops the shaking, big value shakes a lot
            HandleSlider(mouse, sliderPosition5, sliderTexture5, ref perturbationSliderValue, 0.0f, 1.0f, v =>
            {
                perturbationSliderValue = v;
                //Debug.WriteLine($"Perturbation Slider Value: {perturbationSliderValue}");
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
        //Load SVG when you press the load SVG button - default setting behavior handled in OpenSvgFileDialog
        if (sidebarVisible && svgButtonRect.Contains(mouse.X, mouse.Y) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && lastButtonPressTime + TimeSpan.FromMilliseconds(250) < gameTime.TotalGameTime)
        {
            lastButtonPressTime = gameTime.TotalGameTime;
            OpenSvgFileDialog();
        }
        //Get Keyboard Key Presses
        KeyboardState keyboardState = Keyboard.GetState();
        HandleProfileHotkeys(keyboardState);
        //Press ESC to Exit Program
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            Exit();  // This will close the application
        }
        //Press DEL to drp the imported SVG texture
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete))
        {
            svgTexture = null;
        }
        //Full Screen Toggle - its a little buggy but not really sure why. probably something to do with the update loop but works well enough if you hit it a few times. Need to add logic for better windowed mode handling
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11))
        {
            graphics.ToggleFullScreen();
        }
        //Imported SVG dragging logic
        if (svgTexture != null)
        {
            //Debug.WriteLine("SVG Texture is ready to be drawn.");
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
            //Debug.WriteLine("SVG Texture not ready yet.");
        }
        SyncAppStateFromRuntime();
        previousKeyboardState = keyboardState;
        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        //Black Background Color
        GraphicsDevice.Clear(Color.Black);
        //Update and draw the frequency data visualization
        visualizer.UpdateFrequencyData(audioManager.FrequencyData);
        visualizer.Draw(audioManager.FrequencyData);
        //Get the sum of the FFT values for perturbation effect
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
            // Calculate the desired SVG size based on the scale slider value
            int baseWidth = 2 * viewportWidth / 3;
            int baseHeight = 2 * viewportHeight / 3;
            int desiredWidth = (int)(2 * baseWidth * svgScaleSliderValue * amplitudeFactor);
            int desiredHeight = (int)(2 * baseHeight * svgScaleSliderValue * amplitudeFactor);
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
           //Debug.WriteLine($"Drawing SVG at {posX},{posY} with Amplitude Factor: {amplitudeFactor}"); //check the amplitude factor
        }
        if (sidebarVisible)
        {
            // Draw the sidebar background
            spriteBatch.Draw(sidebarTexture, sidebarArea, Color.White);
            // Draw the close button and svg load button
            spriteBatch.Draw(closeButtonTexture, closeButtonRect, Color.White);
            spriteBatch.Draw(closeButtonTexture, svgButtonRect, Color.White); // Reusing the close button texture for now
            // Draw the mode buttons
            for (int i = 0; i < modeButtons.Length; i++)
            {
                spriteBatch.Draw(modeButtonTexture, modeButtons[i], Color.White);// Use the mode button texture all white for now
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
        //If you press the slider and drag it you change the slider value
        if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && sliderRect.Contains(mouse.Position))
        {
            sliderVal = (mouse.X - sliderPos.X) / (float)sliderTex.Width;
            sliderVal = MathHelper.Clamp(sliderVal, minVal, maxVal);
            updateAction(sliderVal);
        }
    }

    private void HandleProfileHotkeys(KeyboardState keyboardState)
    {
        bool controlDown = keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl)
            || keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl);

        var quickSlots = new (Microsoft.Xna.Framework.Input.Keys key, int slot)[]
        {
            (Microsoft.Xna.Framework.Input.Keys.D1, 1),
            (Microsoft.Xna.Framework.Input.Keys.D2, 2),
            (Microsoft.Xna.Framework.Input.Keys.D3, 3),
            (Microsoft.Xna.Framework.Input.Keys.D4, 4),
            (Microsoft.Xna.Framework.Input.Keys.D5, 5),
            (Microsoft.Xna.Framework.Input.Keys.D6, 6),
            (Microsoft.Xna.Framework.Input.Keys.D7, 7),
            (Microsoft.Xna.Framework.Input.Keys.D8, 8),
            (Microsoft.Xna.Framework.Input.Keys.D9, 9),
            (Microsoft.Xna.Framework.Input.Keys.D0, 10)
        };

        foreach (var (key, slot) in quickSlots)
        {
            if (!IsNewKeyPress(keyboardState, key))
            {
                continue;
            }

            if (controlDown)
            {
                SaveProfileToSlot(slot);
            }
            else
            {
                LoadProfileFromSlot(slot);
            }
        }
    }

    private bool IsNewKeyPress(KeyboardState keyboardState, Microsoft.Xna.Framework.Input.Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }

    private void SaveProfileToSlot(int slot)
    {
        SyncAppStateFromRuntime();
        profileManager.SaveSlot(slot, appState);
    }

    private void LoadProfileFromSlot(int slot)
    {
        if (!profileManager.TryLoadSlot(slot, out AppState loadedState))
        {
            return;
        }

        ApplyAppState(loadedState);
        SyncAppStateFromRuntime();
    }

    private void SyncAppStateFromRuntime()
    {
        appState.AmplitudeSlider = sliderValue;
        appState.CutoffSlider = sliderValue2;
        appState.FftSlider = sliderValue3;
        appState.SvgScaleSlider = svgScaleSliderValue;
        appState.SvgPerturbationSlider = perturbationSliderValue;
        appState.ModeIndex = (int)visualizer.Mode;
        appState.ColorToggles = (bool[])colorToggles.Clone();
        appState.LoadedMediaPath = loadedMediaPath;
        appState.SetSvgPosition(svgPosition);
        appState.FftLength = audioManager.FftLength;
    }

    private void ApplyAppState(AppState state)
    {
        appState = state;

        sliderValue = MathHelper.Clamp(state.AmplitudeSlider, 0.05f, 1.0f);
        sliderValue2 = MathHelper.Clamp(state.CutoffSlider, 0.1f, 1.0f);
        sliderValue3 = MathHelper.Clamp(state.FftSlider, 0.1f, 1.0f);
        svgScaleSliderValue = MathHelper.Clamp(state.SvgScaleSlider, 0.01f, 1.0f);
        perturbationSliderValue = MathHelper.Clamp(state.SvgPerturbationSlider, 0.0f, 1.0f);
        visualizer.Mode = (AudioVisualizer.VisualizationMode)Math.Clamp(state.ModeIndex, 0, modeButtons.Length - 1);

        if (state.ColorToggles != null && state.ColorToggles.Length == colorToggles.Length)
        {
            colorToggles = (bool[])state.ColorToggles.Clone();
            visualizer.ColorToggles = colorToggles;
            visualizer.UpdateActiveColors(colors, colorToggles);
        }

        if (state.FftLength > 0 && state.FftLength != audioManager.FftLength)
        {
            audioManager.UpdateFFTLength(state.FftLength);
            visualizer.UpdateFFTBinCount(state.FftLength / 2);
        }

        visualizer.UpdateFrequencyCutoff(sliderValue2);
        float scaledValue = 1.0f + (100f - 1.0f) * sliderValue;
        visualizer.UpdateScaleFactor(scaledValue);

        svgPosition = state.GetSvgPosition();
        loadedMediaPath = state.LoadedMediaPath ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(loadedMediaPath) && File.Exists(loadedMediaPath))
        {
            try
            {
                svgTexture = LoadSvgTexture(loadedMediaPath);
            }
            catch
            {
                svgTexture = null;
            }
        }
    }
}
public class AudioManager
{
    private WaveInEvent waveIn;
    private Complex[] fftBuffer;
    public float[] FrequencyData { get; private set; }
    private int fftLength = 512;  // Must be a power of 2
    private int m;  // Log base 2 of fftLength
    public int FftLength => fftLength;
    public AudioManager()
    {
        m = (int)Math.Log(fftLength, 2.0);
        fftBuffer = new Complex[fftLength];
        FrequencyData = new float[fftLength / 2]; // Initialize FrequencyData to avoid null references
    }
    public void Initialize(int fftLength = 64)
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
                WaveFormat = new WaveFormat(22050, 1)  // Mono channel, Custom cutoff half of "CD Quality" to crop out non-musical frequencies that dont do a lot for the visual feedback. 
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
        // Need to build out more complex audio input selection. Current logic will use default input audio device in Windows
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
        //When there is stuff to do, do it
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
            fftBuffer[i / 2].Y = 0; // Imaginary part is zero. We obey only real numbers here
            index = i / 2;
        }
        return index + 1; // Return count of points filled
    }
    private void ProcessFFTResults()
    {
        //make the numbers so we can build the colored shapes
        FrequencyData = new float[fftLength / 2];
        for (int i = 0; i < FrequencyData.Length; i++)
        {
            FrequencyData[i] = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
        }
    }
    public void Stop()
    {
        //very important or else program no worky right
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
    //Get the colors!
    public List<Color> ActiveColors { get; private set; } = new List<Color>();
    public Color[] Colors { get; set; }  // make array of the colors
    public bool[] ColorToggles { get; set; }  // Array of color toggles
    private float scaleFactor = 1.0f;  // Default scale factor
    public float GetAmplitudeSum(float[] frequencyData)
    {
        // Sum up the FFT values to make a virtual amplitude value for the full sound
        if (frequencyData == null || frequencyData.Length == 0)
            return 0;

        float sum = 0;
        for (int i = 0; i < frequencyData.Length; i++)
        {
            sum += frequencyData[i];
        }
        //Debug.WriteLine($"Amplitude Sum: {sum}");
        return sum;
    }
    public void UpdateActiveColors(Color[] allColors, bool[] toggles)
    {
        //make sure you got the right colors
        ActiveColors.Clear();
        for (int i = 0; i < allColors.Length; i++)
        {
            if (toggles[i])
                ActiveColors.Add(allColors[i]);
        }
    }
    public enum VisualizationMode
    {
        //list of available modes
        Standard,
        MirroredMiddle,
        MirroredCorners,
        Radial,
        CenterColumn,
        Puddle
    }
    public VisualizationMode Mode { get; set; } = VisualizationMode.Radial;//set the default mode to Radial mode on startup
    public AudioVisualizer(GraphicsDevice graphicsDevice, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
    {
        //Draw the shapes and color them
        this.graphicsDevice = graphicsDevice;
        this.spriteBatch = spriteBatch;
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new Color[] { Color.White });  // White pixel for drawing
    }
    public void UpdateScaleFactor(float newScaleFactor)
    {
        //Get the slider value
        scaleFactor = MathHelper.Clamp(newScaleFactor, 0.25f, 100f);
    }
    public void UpdateFrequencyData(float[] data)
    {
        //Get the slider value
        frequencyData = data;
    }
    public void UpdateFrequencyCutoff(float cutoff)
    {
        frequencyCutoff = cutoff;
    }
    public void UpdateFFTBinCount(int newBinCount)
    {
        //Get the slider value
        fftBinCount = newBinCount;
    }
    public void Draw(float[] frequencyData)
    {
        //switch between the modes when triggered
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
            case VisualizationMode.CenterColumn:
                DrawCenterColumn(frequencyData);
                break;
            case VisualizationMode.Puddle:
                DrawPuddle(frequencyData);
                break;
        }
    }
    private void DrawStandard(float[] frequencyData)
    {
        //Based on the classic Graphic Equalizer. This was the proof of concept mode and ended up being functional enough to include in the final product
        if (frequencyData == null || frequencyData.Length == 0)
        {
            //Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        //Debug.WriteLine($"Drawing {frequencyData.Length} bars with cutoff at {frequencyCutoff}");
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
        //This mode was an accident when I was trying to program MirroredCorners mode. It turend out to be really cool so I made it its own mode
        if (frequencyData == null || frequencyData.Length == 0)
        {
            //Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0) return; // Skip if no colors are enabled
        // Fetch fresh viewport dimensions each frame to handle dynamic resizing
        int viewportWidth = graphicsDevice.Viewport.Width;
        int viewportHeight = graphicsDevice.Viewport.Height;
        // Recalculate the bar width based on current fftBinCount and viewport width
        int barWidth = Math.Max(10, viewportWidth / Math.Max(1, fftBinCount));
        int maxIndex = (int)(frequencyData.Length * frequencyCutoff);
        float maxBarHeight = viewportHeight / 2; // Half the height for mirrored effect
        spriteBatch.Begin();
        //Debug.WriteLine($"Drawing {maxIndex} bars with cutoff at {frequencyCutoff}, Bar Width: {barWidth}");
        for (int i = 0; i < maxIndex; i++)
        {
            float magnitude = frequencyData[i] * scaleFactor; // Apply scale factor
            float normalizedMagnitude = Math.Min(magnitude, 1.0f); // Clamp magnitude to a max of 1
            float barHeight = normalizedMagnitude * maxBarHeight;
            Color color = ActiveColors[i % ActiveColors.Count];
            int topBarY = (int)(maxBarHeight - barHeight);
            int bottomBarY = (int)(maxBarHeight + barHeight);
            // Draw only if within the viewport width to avoid drawing off-screen
            if (i * barWidth < viewportWidth)
            {
                spriteBatch.Draw(pixel, new Rectangle(i * barWidth, bottomBarY, barWidth, (int)barHeight), color);
                spriteBatch.Draw(pixel, new Rectangle(i * barWidth, topBarY, barWidth, (int)barHeight), color);
            }
        }
        spriteBatch.End();
    }
    private void DrawCenterColumn(float[] frequencyData)
    {
        //This mode was also an accident when I was trying to program MirroredCorners mode but it was super neat so it ended up as its own mode
        if (frequencyData == null || frequencyData.Length == 0)
        {
            //Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        //Debug.WriteLine($"Drawing {frequencyData.Length} bars with cutoff at {frequencyCutoff}");
        int viewportWidth = graphicsDevice.Viewport.Width;
        int viewportHeight = graphicsDevice.Viewport.Height;
        int maxIndex = (int)(frequencyData.Length * frequencyCutoff + 1);
        int totalBars = Math.Min(fftBinCount, maxIndex); // Limit total bars to available data
        int barWidth = Math.Max(1, viewportWidth / Math.Max(1, fftBinCount)); // Adjust for mirroring from center
        float maxBarHeight = viewportHeight / 2; // Use half the viewport height for top and bottom
        spriteBatch.Begin();
        for (int i = 0; i < maxIndex; i++)
        {
            float magnitude = frequencyData[i] * scaleFactor; // Apply scale factor
            float normalizedMagnitude = Math.Min(magnitude, 1.0f); // Clamp magnitude to a max of 1
            float barHeight = normalizedMagnitude * maxBarHeight;
            Color color = ActiveColors[i % ActiveColors.Count];
            // Correct starting points for each corner
            int leftX = (viewportWidth / 2) - (i * barWidth);
            int rightX = (viewportWidth / 2) + i * barWidth - barWidth;
            if (leftX >= 0 && rightX + barWidth <= viewportWidth)
            {
                // Draw bars on the bottom from the corners towards the center
                spriteBatch.Draw(pixel, new Rectangle(leftX, viewportHeight - (int)barHeight, barWidth, (int)barHeight), color);
                spriteBatch.Draw(pixel, new Rectangle(rightX, viewportHeight - (int)barHeight, barWidth, (int)barHeight), color);
                // Draw mirrored bars on the top from the corners towards the center
                spriteBatch.Draw(pixel, new Rectangle(leftX, 0, barWidth, (int)barHeight), color);
                spriteBatch.Draw(pixel, new Rectangle(rightX, 0, barWidth, (int)barHeight), color);
            }
        }
        spriteBatch.End();
    }
    private void DrawMirroredCorners(float[] frequencyData)
    {
        //This one took a while to get right for some reason. Had to develop a new scaling logic to make sure the corners stayed in the right place.
        //This leads to different edge conditions when the sliders are at limits than the other modes.
        //potential bug? maybe a feature. it allows for a single color to take up the whole screen which is neat but goes blank if first three sliders are at minimum
        if (frequencyData == null || frequencyData.Length == 0)
        {
            //Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        //Debug.WriteLine($"Drawing {frequencyData.Length} bars with cutoff at {frequencyCutoff}");
        int viewportWidth = 1 + graphicsDevice.Viewport.Width;
        int viewportHeight = graphicsDevice.Viewport.Height;
        int maxIndex = (int)(frequencyData.Length * frequencyCutoff) / 2;
        int totalBars = Math.Min(fftBinCount, maxIndex); // Limit total bars to available data
        int barWidth = Math.Max(10, viewportWidth / Math.Max(1, totalBars * 2)); // Adjust for mirroring from center
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
        //This mode was interesting to implement. Had to create new logic to draw the wedges but this turned out to be one of the defining features of the program.
        //There is a lot more that could be done with the tools developed for this mode in the future but for now it remains the only mode to use DrawWedge and DrawTriangle
        if (frequencyData == null || frequencyData.Length == 0)
        {
            //Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        //Debug.WriteLine($"Drawing {frequencyData.Length} bars with cutoff at {frequencyCutoff}");
        int viewportWidth = graphicsDevice.Viewport.Width;
        int viewportHeight = graphicsDevice.Viewport.Height;
        Vector2 center = new Vector2(viewportWidth / 2, viewportHeight / 2);
        float maxRadius = Math.Min(viewportWidth, viewportHeight) / 2; // Max radius of the circle
        int maxIndex = (int)(frequencyData.Length * frequencyCutoff);
        float angleStep = (float)(Math.PI * 4 / fftBinCount); // Total angle divided by the number of bins
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
    private void DrawPuddle(float[] frequencyData)
    {
        //This was the final mode added to the program but was one of the initial ideas for why I made the program.
        //The rings do not move position with the music but instead change their opacity.
        //The sliders control the size, number and dynamic range of the circles
        if (frequencyData == null || frequencyData.Length == 0)
        {
            //Debug.WriteLine("No frequency data to draw.");
            return;
        }
        int enabledColorCount = ColorToggles.Count(t => t);
        if (enabledColorCount == 0)
            return; // Skip drawing if no colors are enabled
        Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
        int numberOfRings = (int)(frequencyData.Length * frequencyCutoff); // Number of rings based on cutoff
        float maxRadius = Math.Min(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height) / 2; // Maximum radius of the outermost ring
        float ringWidth = maxRadius / numberOfRings; // Width of each ring
        // Create a BasicEffect for drawing
        BasicEffect effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0, 1),
            View = Matrix.Identity,
            World = Matrix.Identity
        };
        // Begin the effect and drawing
        effect.CurrentTechnique.Passes[0].Apply();
        for (int i = 0; i < numberOfRings; i++)
        {
            float magnitude = frequencyData[i] * scaleFactor; // Apply scale factor
            float opacity = Math.Min(magnitude, 1.0f); // Use magnitude to determine opacity
            Color color = ActiveColors[i % ActiveColors.Count] * opacity; // Apply opacity to color
            DrawRing(center, ringWidth * i, ringWidth * (i + 1), color, effect);
        }
        effect.Dispose(); // Dispose the effect after use
    }
    private void DrawRing(Vector2 center, float innerRadius, float outerRadius, Color color, BasicEffect effect)
    {
        //Used in DrawPuddle to draw each circle
        int segments = 100; // More segments mean a smoother circle
        for (int i = 0; i < segments; i++)
        {
            float angle1 = MathHelper.TwoPi * i / segments;
            float angle2 = MathHelper.TwoPi * (i + 1) / segments;
            Vector2 outer1 = center + new Vector2((float)Math.Cos(angle1) * outerRadius, (float)Math.Sin(angle1) * outerRadius);
            Vector2 outer2 = center + new Vector2((float)Math.Cos(angle2) * outerRadius, (float)Math.Sin(angle2) * outerRadius);
            Vector2 inner1 = center + new Vector2((float)Math.Cos(angle1) * innerRadius, (float)Math.Sin(angle1) * innerRadius);
            Vector2 inner2 = center + new Vector2((float)Math.Cos(angle2) * innerRadius, (float)Math.Sin(angle2) * innerRadius);
            // Draw triangles to form a ring segment
            DrawTriangle(inner1, outer1, outer2, color, effect);
            DrawTriangle(inner1, outer2, inner2, color, effect);
        }
    }
    private void DrawWedge(Vector2 center, float radius, float startAngle, float endAngle, Color color)
    {
        //Used in DrawRadial to draw each wedge in the pattern
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
        effect.Dispose();//This line is crucial to prevent the wedge calculation from filling up the RAM until the computer croaks. ask me how I know
    }
    private void DrawTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color, BasicEffect effect)
    {
        //Used in DrawWedge which is used in DrawRadial. This makes triangles
        VertexPositionColor[] vertices = new VertexPositionColor[3];
        vertices[0] = new VertexPositionColor(new Vector3(v1, 0), color);
        vertices[1] = new VertexPositionColor(new Vector3(v2, 0), color);
        vertices[2] = new VertexPositionColor(new Vector3(v3, 0), color);
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
//If you are reading this, thank you for looking through the code for Visualizationizer! I hope my notes were helpful. Good luck out there. 
