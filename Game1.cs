 //Welcome to Visualizationizer! I did my best to comment the code. There is always more to do and more that could be done, ya know?
//Debug lines have been commented out for resources. uncomment them as needed
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Linq;
using NAudio.Dsp;  // Include for FFT
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

public class Visualizationizer : Game
{
    private enum InputMode
    {
        MouseKeyboard,
        Controller
    }

    private enum FocusDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    private enum FocusTargetType
    {
        LeftClose,
        LoadMedia,
        Slider,
        Color,
        Mode,
        RightClose,
        RightMicMode,
        RightSystemMode,
        RightPrevInput,
        RightNextInput
    }

    private struct FocusTarget
    {
        public FocusTargetType Type;
        public int Index;
        public Rectangle Rect;
    }

    private GraphicsDeviceManager graphics;
    private Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch;
    private Rectangle sidebarArea;
    private Rectangle closeButtonRect;    
    private Rectangle rightSidebarArea;
    private Rectangle rightCloseButtonRect;
    private Rectangle rightMicModeButtonRect;
    private Rectangle rightSystemModeButtonRect;
    private Rectangle rightPrevDeviceButtonRect;
    private Rectangle rightNextDeviceButtonRect;
    private AudioVisualizer visualizer;
    private AudioManager audioManager;
    private bool sidebarVisible;
    private bool rightSidebarVisible;
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
    private Point lastMousePosition;
    private int lastMouseWheelValue;
    private bool hasMouseSample = false;
    private TimeSpan lastMouseActivityTime = TimeSpan.Zero;
    private static readonly TimeSpan MouseHideDelay = TimeSpan.FromSeconds(2);
    private bool showHelpOverlay = false;
    private Dictionary<string, Texture2D> helpLabelTextures = new Dictionary<string, Texture2D>();
    private Texture2D focusHighlightTexture;
    private const float StandardImportScale = 0.35f;
    private InputMode currentInputMode = InputMode.MouseKeyboard;
    private GamePadState previousGamePadState;
    private bool controllerActiveOnRightPanel = false;
    private List<FocusTarget> controllerFocusTargets = new List<FocusTarget>();
    private int focusedControllerTargetIndex = -1;
    private bool controllerFocusVisible = false;
    private bool controllerHelpOverlayHeld = false;
    public Visualizationizer()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.SynchronizeWithVerticalRetrace = true;  // Sync with monitor refresh rate
        IsFixedTimeStep = true;  // Enable fixed time steps
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 60.0);  // 60 FPS
        Content.RootDirectory = "SVGs";
        IsMouseVisible = true;
        sidebarVisible = false;
        rightSidebarVisible = false;
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
        RecalculateUILayout();
        LoadDefaultMedia();
        svgPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
    }
    private void OpenMediaFileDialog()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        bool shiftPressed = keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);

        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Supported Media (*.svg;*.png;*.jpg;*.jpeg;*.gif)|*.svg;*.png;*.jpg;*.jpeg;*.gif|SVG Files (*.svg)|*.svg|Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif";
            openFileDialog.Title = "Open Media File";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadMediaFromPath(openFileDialog.FileName, true);

                if (shiftPressed)
                {
                    System.Windows.Forms.MessageBox.Show("Now writing this media file to the SVGs folder as startup media for use on startup.", "Setting Default Media", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetAsDefaultMedia(openFileDialog.FileName);
                }
            }
        }
    }
    private void SetAsDefaultMedia(string filePath)
    {
        if (!MediaLoader.IsSupportedFile(filePath))
        {
            return;
        }

        string mediaFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SVGs");
        if (!Directory.Exists(mediaFolderPath))
        {
            Directory.CreateDirectory(mediaFolderPath);
        }

        foreach (string extension in MediaLoader.GetSupportedExtensions())
        {
            string oldStartupPath = Path.Combine(mediaFolderPath, $"startup{extension}");
            if (File.Exists(oldStartupPath))
            {
                File.Delete(oldStartupPath);
            }
        }

        string startupPath = Path.Combine(mediaFolderPath, $"startup{Path.GetExtension(filePath).ToLowerInvariant()}");
        try
        {
            File.Copy(filePath, startupPath, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set default media: {ex.Message}");
            System.Windows.Forms.MessageBox.Show($"Failed to set default media to {AppDomain.CurrentDomain.BaseDirectory}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void LoadDefaultMedia()
    {
        string mediaFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SVGs");
        foreach (string extension in MediaLoader.GetSupportedExtensions())
        {
            string startupPath = Path.Combine(mediaFolderPath, $"startup{extension}");
            if (File.Exists(startupPath))
            {
                LoadMediaFromPath(startupPath, false);
                break;
            }
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
    private void LoadMediaFromPath(string filePath, bool applyStandardScale)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || !MediaLoader.IsSupportedFile(filePath))
        {
            return;
        }

        try
        {
            Texture2D loadedTexture = MediaLoader.LoadTexture(graphics.GraphicsDevice, filePath);
            svgTexture?.Dispose();
            svgTexture = loadedTexture;
            loadedMediaPath = filePath;

            if (applyStandardScale)
            {
                svgScaleSliderValue = StandardImportScale;
                svgPosition = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load media file '{filePath}': {ex.Message}");
            System.Windows.Forms.MessageBox.Show($"Failed to load media file: {Path.GetFileName(filePath)}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        RecalculateUILayout();
        focusHighlightTexture?.Dispose();
        focusHighlightTexture = CreateColorTexture(1, 1, Color.White);
    }
    protected override void UnloadContent()
    {
        foreach (var texture in helpLabelTextures.Values)
        {
            texture?.Dispose();
        }
        helpLabelTextures.Clear();
        focusHighlightTexture?.Dispose();
        focusHighlightTexture = null;
        audioManager?.Stop();
        base.UnloadContent();
    }
    private void OnResize(object sender, EventArgs e)
    {
        graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        graphics.ApplyChanges();
        RecalculateUILayout();
    }
    protected override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        UpdateMouseVisibility(mouse, gameTime);
        int viewportWidth = GraphicsDevice.Viewport.Width;

        if (!sidebarVisible && mouse.X <= 5)
        {
            sidebarVisible = true;
        }
        else if (sidebarVisible && closeButtonRect.Contains(mouse.X, mouse.Y) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        {
            sidebarVisible = false;
        }

        if (!rightSidebarVisible && mouse.X >= viewportWidth - 5)
        {
            rightSidebarVisible = true;
        }
        else if (rightSidebarVisible && rightCloseButtonRect.Contains(mouse.X, mouse.Y) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        {
            rightSidebarVisible = false;
        }

        if (rightSidebarVisible)
        {
            HandleRightPanelInteraction(mouse, gameTime);
        }

        UpdateControllerInput(gameTime);

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
            // Slider 4 - Size of the imported media file
            HandleSlider(mouse, sliderPosition4, sliderTexture, ref svgScaleSliderValue, 0.01f, 1.0f, v =>
            {
                svgScaleSliderValue = v;
            });
            // Slider 5 - Perturbation factor for imported media file. Zero value stops the shaking, big value shakes a lot
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
        //Load media when you press the load button - default setting behavior handled in OpenMediaFileDialog
        if (sidebarVisible && svgButtonRect.Contains(mouse.X, mouse.Y) && mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && lastButtonPressTime + TimeSpan.FromMilliseconds(250) < gameTime.TotalGameTime)
        {
            lastButtonPressTime = gameTime.TotalGameTime;
            OpenMediaFileDialog();
        }
        //Get Keyboard Key Presses
        KeyboardState keyboardState = Keyboard.GetState();
        UpdateKeyboardInputMode(keyboardState);
        showHelpOverlay = keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) || controllerHelpOverlayHeld;
        HandleProfileHotkeys(keyboardState);
        //Press ESC to Exit Program
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            Exit();  // This will close the application
        }
        //Press DEL to drop the imported media texture
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete))
        {
            svgTexture?.Dispose();
            svgTexture = null;
            loadedMediaPath = string.Empty;
        }
        //Full Screen Toggle - works with current resize recalculation path.
        if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F11))
        {
            graphics.ToggleFullScreen();
            RecalculateUILayout();
        }
        //Imported media dragging logic
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
        if (rightSidebarVisible)
        {
            spriteBatch.Draw(sidebarTexture, rightSidebarArea, Color.White);
            spriteBatch.Draw(closeButtonTexture, rightCloseButtonRect, Color.White);
            Color micModeColor = audioManager.CaptureSource == AudioCaptureSource.Microphone ? new Color(205, 255, 205) : Color.White;
            Color systemModeColor = audioManager.CaptureSource == AudioCaptureSource.SystemLoopback ? new Color(205, 255, 205) : Color.White;
            spriteBatch.Draw(closeButtonTexture, rightMicModeButtonRect, micModeColor);
            spriteBatch.Draw(closeButtonTexture, rightSystemModeButtonRect, systemModeColor);

            Color deviceButtonColor = audioManager.CaptureSource == AudioCaptureSource.Microphone ? Color.White : new Color(170, 170, 170);
            spriteBatch.Draw(closeButtonTexture, rightPrevDeviceButtonRect, deviceButtonColor);
            spriteBatch.Draw(closeButtonTexture, rightNextDeviceButtonRect, deviceButtonColor);
        }
        DrawControllerFocusHighlight();
        if (showHelpOverlay)
        {
            DrawHelpOverlay();
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

    private void HandleRightPanelInteraction(MouseState mouse, GameTime gameTime)
    {
        if (mouse.LeftButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        {
            return;
        }

        if (lastButtonPressTime + TimeSpan.FromMilliseconds(250) > gameTime.TotalGameTime)
        {
            return;
        }

        if (rightMicModeButtonRect.Contains(mouse.Position))
        {
            audioManager.SetCaptureSource(AudioCaptureSource.Microphone);
            lastButtonPressTime = gameTime.TotalGameTime;
            return;
        }

        if (rightSystemModeButtonRect.Contains(mouse.Position))
        {
            audioManager.SetCaptureSource(AudioCaptureSource.SystemLoopback);
            lastButtonPressTime = gameTime.TotalGameTime;
            return;
        }

        if (audioManager.CaptureSource == AudioCaptureSource.Microphone)
        {
            if (rightPrevDeviceButtonRect.Contains(mouse.Position))
            {
                audioManager.SelectPreviousInputDevice();
                lastButtonPressTime = gameTime.TotalGameTime;
                return;
            }

            if (rightNextDeviceButtonRect.Contains(mouse.Position))
            {
                audioManager.SelectNextInputDevice();
                lastButtonPressTime = gameTime.TotalGameTime;
                return;
            }
        }
    }

    private void UpdateKeyboardInputMode(KeyboardState keyboardState)
    {
        foreach (var key in keyboardState.GetPressedKeys())
        {
            if (previousKeyboardState.IsKeyUp(key))
            {
                if (currentInputMode != InputMode.MouseKeyboard)
                {
                    currentInputMode = InputMode.MouseKeyboard;
                    controllerFocusVisible = false;
                }
                return;
            }
        }
    }

    private void UpdateControllerInput(GameTime gameTime)
    {
        GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
        if (!gamePadState.IsConnected)
        {
            controllerHelpOverlayHeld = false;
            controllerFocusTargets.Clear();
            focusedControllerTargetIndex = -1;
            controllerFocusVisible = false;
            previousGamePadState = gamePadState;
            return;
        }

        controllerHelpOverlayHeld = gamePadState.IsButtonDown(Buttons.Start);

        bool hasControllerActivity = HasControllerActivity(gamePadState);
        if (hasControllerActivity)
        {
            currentInputMode = InputMode.Controller;
            controllerFocusVisible = true;
        }

        if (IsNewGamePadButtonPress(gamePadState, Buttons.LeftShoulder))
        {
            sidebarVisible = !sidebarVisible;
            controllerActiveOnRightPanel = false;
            currentInputMode = InputMode.Controller;
            controllerFocusVisible = true;
        }
        if (IsNewGamePadButtonPress(gamePadState, Buttons.RightShoulder))
        {
            rightSidebarVisible = !rightSidebarVisible;
            controllerActiveOnRightPanel = true;
            currentInputMode = InputMode.Controller;
            controllerFocusVisible = true;
        }

        if (currentInputMode == InputMode.Controller)
        {
            RebuildControllerFocusTargets();

            if (controllerFocusTargets.Count > 0)
            {
                bool navLeft = IsNewGamePadButtonPress(gamePadState, Buttons.DPadLeft)
                    || IsNewStickDirection(gamePadState.ThumbSticks.Left.X, previousGamePadState.ThumbSticks.Left.X, -1);
                bool navRight = IsNewGamePadButtonPress(gamePadState, Buttons.DPadRight)
                    || IsNewStickDirection(gamePadState.ThumbSticks.Left.X, previousGamePadState.ThumbSticks.Left.X, 1);
                bool navUp = IsNewGamePadButtonPress(gamePadState, Buttons.DPadUp)
                    || IsNewStickDirection(gamePadState.ThumbSticks.Left.Y, previousGamePadState.ThumbSticks.Left.Y, 1);
                bool navDown = IsNewGamePadButtonPress(gamePadState, Buttons.DPadDown)
                    || IsNewStickDirection(gamePadState.ThumbSticks.Left.Y, previousGamePadState.ThumbSticks.Left.Y, -1);

                FocusTarget focusedTarget = controllerFocusTargets[focusedControllerTargetIndex];
                bool focusedSlider = focusedTarget.Type == FocusTargetType.Slider;

                if (focusedSlider && navLeft)
                {
                    AdjustFocusedSlider(-1);
                }
                else if (focusedSlider && navRight)
                {
                    AdjustFocusedSlider(1);
                }
                else
                {
                    if (navLeft) MoveControllerFocus(FocusDirection.Left);
                    if (navRight) MoveControllerFocus(FocusDirection.Right);
                }

                if (navUp) MoveControllerFocus(FocusDirection.Up);
                if (navDown) MoveControllerFocus(FocusDirection.Down);

                if (IsNewGamePadButtonPress(gamePadState, Buttons.A))
                {
                    ActivateFocusedControllerTarget(gameTime);
                }
            }
            else
            {
                controllerFocusVisible = false;
            }
        }

        previousGamePadState = gamePadState;
    }

    private bool HasControllerActivity(GamePadState gamePadState)
    {
        if (IsNewGamePadButtonPress(gamePadState, Buttons.A)
            || IsNewGamePadButtonPress(gamePadState, Buttons.B)
            || IsNewGamePadButtonPress(gamePadState, Buttons.X)
            || IsNewGamePadButtonPress(gamePadState, Buttons.Y)
            || IsNewGamePadButtonPress(gamePadState, Buttons.Start)
            || IsNewGamePadButtonPress(gamePadState, Buttons.Back)
            || IsNewGamePadButtonPress(gamePadState, Buttons.LeftShoulder)
            || IsNewGamePadButtonPress(gamePadState, Buttons.RightShoulder)
            || IsNewGamePadButtonPress(gamePadState, Buttons.DPadUp)
            || IsNewGamePadButtonPress(gamePadState, Buttons.DPadDown)
            || IsNewGamePadButtonPress(gamePadState, Buttons.DPadLeft)
            || IsNewGamePadButtonPress(gamePadState, Buttons.DPadRight))
        {
            return true;
        }

        const float threshold = 0.50f;
        return (Math.Abs(gamePadState.ThumbSticks.Left.X) > threshold && Math.Abs(previousGamePadState.ThumbSticks.Left.X) <= threshold)
            || (Math.Abs(gamePadState.ThumbSticks.Left.Y) > threshold && Math.Abs(previousGamePadState.ThumbSticks.Left.Y) <= threshold);
    }

    private bool IsNewGamePadButtonPress(GamePadState gamePadState, Buttons button)
    {
        return gamePadState.IsButtonDown(button) && previousGamePadState.IsButtonUp(button);
    }

    private static bool IsNewStickDirection(float currentAxis, float previousAxis, int direction)
    {
        const float threshold = 0.50f;
        if (direction < 0)
        {
            return currentAxis < -threshold && previousAxis >= -threshold;
        }
        return currentAxis > threshold && previousAxis <= threshold;
    }

    private void RebuildControllerFocusTargets()
    {
        FocusTargetType previousType = FocusTargetType.LeftClose;
        int previousIndex = 0;
        bool hasPreviousFocus = focusedControllerTargetIndex >= 0 && focusedControllerTargetIndex < controllerFocusTargets.Count;
        if (hasPreviousFocus)
        {
            previousType = controllerFocusTargets[focusedControllerTargetIndex].Type;
            previousIndex = controllerFocusTargets[focusedControllerTargetIndex].Index;
        }

        controllerFocusTargets.Clear();
        bool useRightPanel = controllerActiveOnRightPanel;
        if (useRightPanel && !rightSidebarVisible)
        {
            useRightPanel = false;
        }
        if (!useRightPanel && !sidebarVisible && rightSidebarVisible)
        {
            useRightPanel = true;
        }

        if (!sidebarVisible && !rightSidebarVisible)
        {
            focusedControllerTargetIndex = -1;
            controllerFocusVisible = false;
            return;
        }

        if (useRightPanel)
        {
            AddFocusTarget(FocusTargetType.RightClose, 0, rightCloseButtonRect);
            AddFocusTarget(FocusTargetType.RightMicMode, 0, rightMicModeButtonRect);
            AddFocusTarget(FocusTargetType.RightSystemMode, 0, rightSystemModeButtonRect);
            AddFocusTarget(FocusTargetType.RightPrevInput, 0, rightPrevDeviceButtonRect);
            AddFocusTarget(FocusTargetType.RightNextInput, 0, rightNextDeviceButtonRect);
        }
        else
        {
            AddFocusTarget(FocusTargetType.LeftClose, 0, closeButtonRect);
            AddFocusTarget(FocusTargetType.LoadMedia, 0, svgButtonRect);
            AddFocusTarget(FocusTargetType.Slider, 0, new Rectangle(sliderPosition.ToPoint(), new Point(sliderTexture.Width, sliderTexture.Height)));
            AddFocusTarget(FocusTargetType.Slider, 1, new Rectangle(sliderPosition2.ToPoint(), new Point(sliderTexture2.Width, sliderTexture2.Height)));
            AddFocusTarget(FocusTargetType.Slider, 2, new Rectangle(sliderPosition3.ToPoint(), new Point(sliderTexture3.Width, sliderTexture3.Height)));
            AddFocusTarget(FocusTargetType.Slider, 3, new Rectangle(sliderPosition4.ToPoint(), new Point(sliderTexture.Width, sliderTexture.Height)));
            AddFocusTarget(FocusTargetType.Slider, 4, new Rectangle(sliderPosition5.ToPoint(), new Point(sliderTexture5.Width, sliderTexture5.Height)));
            for (int i = 0; i < colorButtons.Length; i++)
            {
                AddFocusTarget(FocusTargetType.Color, i, colorButtons[i]);
            }
            for (int i = 0; i < modeButtons.Length; i++)
            {
                AddFocusTarget(FocusTargetType.Mode, i, modeButtons[i]);
            }
        }

        focusedControllerTargetIndex = 0;
        for (int i = 0; i < controllerFocusTargets.Count; i++)
        {
            if (controllerFocusTargets[i].Type == previousType && controllerFocusTargets[i].Index == previousIndex)
            {
                focusedControllerTargetIndex = i;
                break;
            }
        }
        controllerFocusVisible = controllerFocusTargets.Count > 0;
    }

    private void AddFocusTarget(FocusTargetType type, int index, Rectangle rect)
    {
        controllerFocusTargets.Add(new FocusTarget
        {
            Type = type,
            Index = index,
            Rect = rect
        });
    }

    private void MoveControllerFocus(FocusDirection direction)
    {
        int nextIndex = FindNextControllerFocusIndex(direction);
        if (nextIndex >= 0)
        {
            focusedControllerTargetIndex = nextIndex;
        }
    }

    private int FindNextControllerFocusIndex(FocusDirection direction)
    {
        if (focusedControllerTargetIndex < 0 || focusedControllerTargetIndex >= controllerFocusTargets.Count)
        {
            return controllerFocusTargets.Count > 0 ? 0 : -1;
        }

        var currentRect = controllerFocusTargets[focusedControllerTargetIndex].Rect;
        var currentCenter = currentRect.Center;
        int bestIndex = -1;
        float bestScore = float.MaxValue;

        for (int i = 0; i < controllerFocusTargets.Count; i++)
        {
            if (i == focusedControllerTargetIndex)
            {
                continue;
            }

            var targetCenter = controllerFocusTargets[i].Rect.Center;
            float dx = targetCenter.X - currentCenter.X;
            float dy = targetCenter.Y - currentCenter.Y;

            float primary;
            float secondary;
            switch (direction)
            {
                case FocusDirection.Left:
                    if (dx >= 0) continue;
                    primary = -dx;
                    secondary = Math.Abs(dy);
                    break;
                case FocusDirection.Right:
                    if (dx <= 0) continue;
                    primary = dx;
                    secondary = Math.Abs(dy);
                    break;
                case FocusDirection.Up:
                    if (dy >= 0) continue;
                    primary = -dy;
                    secondary = Math.Abs(dx);
                    break;
                case FocusDirection.Down:
                    if (dy <= 0) continue;
                    primary = dy;
                    secondary = Math.Abs(dx);
                    break;
                default:
                    continue;
            }

            float score = (primary * 4f) + secondary;
            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void ActivateFocusedControllerTarget(GameTime gameTime)
    {
        if (focusedControllerTargetIndex < 0 || focusedControllerTargetIndex >= controllerFocusTargets.Count)
        {
            return;
        }

        var target = controllerFocusTargets[focusedControllerTargetIndex];
        switch (target.Type)
        {
            case FocusTargetType.LeftClose:
                sidebarVisible = false;
                break;
            case FocusTargetType.LoadMedia:
                OpenMediaFileDialog();
                break;
            case FocusTargetType.Color:
                if (target.Index >= 0 && target.Index < colorToggles.Length)
                {
                    colorToggles[target.Index] = !colorToggles[target.Index];
                    visualizer.UpdateActiveColors(colors, colorToggles);
                }
                break;
            case FocusTargetType.Mode:
                if (target.Index >= 0 && target.Index < modeButtons.Length)
                {
                    visualizer.Mode = (AudioVisualizer.VisualizationMode)target.Index;
                }
                break;
            case FocusTargetType.RightClose:
                rightSidebarVisible = false;
                break;
            case FocusTargetType.RightMicMode:
                audioManager.SetCaptureSource(AudioCaptureSource.Microphone);
                break;
            case FocusTargetType.RightSystemMode:
                audioManager.SetCaptureSource(AudioCaptureSource.SystemLoopback);
                break;
            case FocusTargetType.RightPrevInput:
                if (audioManager.CaptureSource == AudioCaptureSource.Microphone)
                {
                    audioManager.SelectPreviousInputDevice();
                }
                break;
            case FocusTargetType.RightNextInput:
                if (audioManager.CaptureSource == AudioCaptureSource.Microphone)
                {
                    audioManager.SelectNextInputDevice();
                }
                break;
            case FocusTargetType.Slider:
                break;
        }

        lastButtonPressTime = gameTime.TotalGameTime;
    }

    private void AdjustFocusedSlider(int direction)
    {
        if (focusedControllerTargetIndex < 0 || focusedControllerTargetIndex >= controllerFocusTargets.Count)
        {
            return;
        }

        var target = controllerFocusTargets[focusedControllerTargetIndex];
        if (target.Type != FocusTargetType.Slider)
        {
            return;
        }

        const float step = 0.03f;
        switch (target.Index)
        {
            case 0:
                sliderValue = MathHelper.Clamp(sliderValue + (step * direction), 0.05f, 1.0f);
                visualizer.UpdateScaleFactor(1.0f + (100f - 1.0f) * sliderValue);
                break;
            case 1:
                sliderValue2 = MathHelper.Clamp(sliderValue2 + (step * direction), 0.1f, 1.0f);
                frequencyCutoff = sliderValue2;
                visualizer.UpdateFrequencyCutoff(frequencyCutoff);
                break;
            case 2:
                sliderValue3 = MathHelper.Clamp(sliderValue3 + (step * direction), 0.1f, 1.0f);
                int exponent = 5 + (int)((10 - 5) * sliderValue3);
                int newFFTLength = 1 << exponent;
                if (newFFTLength != audioManager.FftLength)
                {
                    audioManager.UpdateFFTLength(newFFTLength);
                    visualizer.UpdateFFTBinCount(newFFTLength / 2);
                }
                break;
            case 3:
                svgScaleSliderValue = MathHelper.Clamp(svgScaleSliderValue + (step * direction), 0.01f, 1.0f);
                break;
            case 4:
                perturbationSliderValue = MathHelper.Clamp(perturbationSliderValue + (step * direction), 0.0f, 1.0f);
                break;
        }
    }

    private void UpdateMouseVisibility(MouseState mouse, GameTime gameTime)
    {
        bool moved = !hasMouseSample
            || mouse.Position != lastMousePosition
            || mouse.ScrollWheelValue != lastMouseWheelValue;

        bool buttonPressed = mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed
            || mouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed
            || mouse.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed
            || mouse.XButton1 == Microsoft.Xna.Framework.Input.ButtonState.Pressed
            || mouse.XButton2 == Microsoft.Xna.Framework.Input.ButtonState.Pressed;

        if (moved || buttonPressed)
        {
            lastMouseActivityTime = gameTime.TotalGameTime;
            if (currentInputMode != InputMode.MouseKeyboard)
            {
                currentInputMode = InputMode.MouseKeyboard;
                controllerFocusVisible = false;
            }
            if (!IsMouseVisible)
            {
                IsMouseVisible = true;
            }
        }
        else if (IsMouseVisible && gameTime.TotalGameTime - lastMouseActivityTime >= MouseHideDelay)
        {
            IsMouseVisible = false;
        }

        hasMouseSample = true;
        lastMousePosition = mouse.Position;
        lastMouseWheelValue = mouse.ScrollWheelValue;
    }

    private void InitializeHelpOverlayTextures()
    {
        foreach (var texture in helpLabelTextures.Values)
        {
            texture?.Dispose();
        }
        helpLabelTextures.Clear();

        AddHelpLabelTexture("close", "Close");
        AddHelpLabelTexture("load", "Load Media");
        AddHelpLabelTexture("slider1", "Amplitude");
        AddHelpLabelTexture("slider2", "Cutoff");
        AddHelpLabelTexture("slider3", "FFT Bins");
        AddHelpLabelTexture("slider4", "Image Size");
        AddHelpLabelTexture("slider5", "Image React");
        AddHelpLabelTexture("colors", "Colors");
        AddHelpLabelTexture("right_mic", "Mic In");
        AddHelpLabelTexture("right_system", "System");
        AddHelpLabelTexture("right_prev", "Prev Input");
        AddHelpLabelTexture("right_next", "Next Input");
        for (int i = 0; i < modeLabels.Length; i++)
        {
            AddHelpLabelTexture($"mode{i}", modeLabels[i]);
        }
    }

    private void AddHelpLabelTexture(string key, string text)
    {
        int width = Math.Max(72, text.Length * 11);
        helpLabelTextures[key] = CreateTextTexture(text, width, 24);
    }

    private Texture2D CreateTextTexture(string text, int width, int height)
    {
        using (var bitmap = new System.Drawing.Bitmap(width, height))
        {
            using (var gfx = System.Drawing.Graphics.FromImage(bitmap))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gfx.Clear(System.Drawing.Color.Transparent);
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                using (var font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel))
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                using (var format = new System.Drawing.StringFormat())
                {
                    format.Alignment = System.Drawing.StringAlignment.Center;
                    format.LineAlignment = System.Drawing.StringAlignment.Center;
                    gfx.DrawString(text, font, brush, new System.Drawing.RectangleF(0, 0, width, height), format);
                }
            }
            return CreateTextureFromBitmap(graphics.GraphicsDevice, bitmap);
        }
    }

    private void DrawControllerFocusHighlight()
    {
        if (!controllerFocusVisible
            || currentInputMode != InputMode.Controller
            || focusHighlightTexture == null
            || focusedControllerTargetIndex < 0
            || focusedControllerTargetIndex >= controllerFocusTargets.Count)
        {
            return;
        }

        Rectangle rect = controllerFocusTargets[focusedControllerTargetIndex].Rect;
        Rectangle fillRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        spriteBatch.Draw(focusHighlightTexture, fillRect, new Color(0, 0, 0, 30));

        DrawFocusBorder(new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4), 3, new Color(0, 0, 0, 220));
        DrawFocusBorder(rect, 2, new Color(100, 255, 220, 255));
    }

    private void DrawFocusBorder(Rectangle rect, int thickness, Color color)
    {
        if (rect.Width <= 0 || rect.Height <= 0 || thickness <= 0)
        {
            return;
        }

        Rectangle top = new Rectangle(rect.X, rect.Y, rect.Width, thickness);
        Rectangle bottom = new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness);
        Rectangle left = new Rectangle(rect.X, rect.Y, thickness, rect.Height);
        Rectangle right = new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height);

        spriteBatch.Draw(focusHighlightTexture, top, color);
        spriteBatch.Draw(focusHighlightTexture, bottom, color);
        spriteBatch.Draw(focusHighlightTexture, left, color);
        spriteBatch.Draw(focusHighlightTexture, right, color);
    }

    private void DrawHelpOverlay()
    {
        if (sidebarVisible)
        {
            spriteBatch.Draw(closeButtonTexture, sidebarArea, new Color(0, 0, 0, 120));
            DrawHelpLabel("close", closeButtonRect);
            DrawHelpLabel("load", svgButtonRect);
            DrawHelpLabel("slider1", new Rectangle(sliderPosition.ToPoint(), new Point(sliderTexture.Width, sliderTexture.Height)));
            DrawHelpLabel("slider2", new Rectangle(sliderPosition2.ToPoint(), new Point(sliderTexture2.Width, sliderTexture2.Height)));
            DrawHelpLabel("slider3", new Rectangle(sliderPosition3.ToPoint(), new Point(sliderTexture3.Width, sliderTexture3.Height)));
            DrawHelpLabel("slider4", new Rectangle(sliderPosition4.ToPoint(), new Point(sliderTexture.Width, sliderTexture.Height)));
            DrawHelpLabel("slider5", new Rectangle(sliderPosition5.ToPoint(), new Point(sliderTexture5.Width, sliderTexture5.Height)));
            DrawHelpLabel("colors", GetBoundingRect(colorButtons));
            for (int i = 0; i < modeButtons.Length; i++)
            {
                DrawHelpLabel($"mode{i}", modeButtons[i]);
            }
        }

        if (rightSidebarVisible)
        {
            spriteBatch.Draw(closeButtonTexture, rightSidebarArea, new Color(0, 0, 0, 120));
            DrawHelpLabel("close", rightCloseButtonRect);
            DrawHelpLabel("right_mic", rightMicModeButtonRect);
            DrawHelpLabel("right_system", rightSystemModeButtonRect);
            DrawHelpLabel("right_prev", rightPrevDeviceButtonRect);
            DrawHelpLabel("right_next", rightNextDeviceButtonRect);
        }
    }

    private void DrawHelpLabel(string key, Rectangle anchorRect)
    {
        if (!helpLabelTextures.TryGetValue(key, out Texture2D labelTexture))
        {
            return;
        }
        int labelX = anchorRect.Center.X - (labelTexture.Width / 2);
        int labelY = anchorRect.Center.Y - (labelTexture.Height / 2);
        Rectangle backRect = new Rectangle(anchorRect.X, anchorRect.Y, anchorRect.Width, anchorRect.Height);
        spriteBatch.Draw(closeButtonTexture, backRect, new Color(0, 0, 0, 180));
        spriteBatch.Draw(labelTexture, new Vector2(labelX, labelY), Color.White);
    }

    private Rectangle GetBoundingRect(Rectangle[] rectangles)
    {
        int minX = rectangles.Min(r => r.Left);
        int minY = rectangles.Min(r => r.Top);
        int maxX = rectangles.Max(r => r.Right);
        int maxY = rectangles.Max(r => r.Bottom);
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    private void RecalculateUILayout()
    {
        int viewportWidth = GraphicsDevice.Viewport.Width;
        int viewportHeight = GraphicsDevice.Viewport.Height;
        float uiScale = MathHelper.Clamp(viewportHeight / 1200f, 0.60f, 1.35f);

        sidebarWidth = Math.Max(200, (int)(260 * uiScale));
        sidebarArea = new Rectangle(0, 0, sidebarWidth, viewportHeight);
        rightSidebarArea = new Rectangle(Math.Max(0, viewportWidth - sidebarWidth), 0, sidebarWidth, viewportHeight);

        int sidePadding = Math.Max(12, (int)(20 * uiScale));
        int topButtonGap = Math.Max(8, (int)(15 * uiScale));
        int topButtonHeight = Math.Max(22, (int)(35 * uiScale));
        int topButtonWidth = Math.Max(60, (sidebarWidth - (sidePadding * 2) - topButtonGap) / 2);
        closeButtonRect = new Rectangle(sidePadding, sidePadding, topButtonWidth, topButtonHeight);
        rightCloseButtonRect = new Rectangle(rightSidebarArea.X + sidePadding, sidePadding, topButtonWidth, topButtonHeight);
        svgButtonRect = new Rectangle(closeButtonRect.Right + topButtonGap, sidePadding, topButtonWidth, topButtonHeight);

        int rightContentX = rightSidebarArea.X + sidePadding;
        int rightContentWidth = Math.Max(120, sidebarWidth - (sidePadding * 2));
        int rightRowGap = Math.Max(10, (int)(18 * uiScale));
        int rightStartY = rightCloseButtonRect.Bottom + Math.Max(14, (int)(24 * uiScale));
        int rightHalfWidth = Math.Max(50, (rightContentWidth - topButtonGap) / 2);
        rightMicModeButtonRect = new Rectangle(rightContentX, rightStartY, rightHalfWidth, topButtonHeight);
        rightSystemModeButtonRect = new Rectangle(rightMicModeButtonRect.Right + topButtonGap, rightStartY, rightHalfWidth, topButtonHeight);

        int rightDeviceRowY = rightStartY + topButtonHeight + rightRowGap;
        rightPrevDeviceButtonRect = new Rectangle(rightContentX, rightDeviceRowY, rightHalfWidth, topButtonHeight);
        rightNextDeviceButtonRect = new Rectangle(rightPrevDeviceButtonRect.Right + topButtonGap, rightDeviceRowY, rightHalfWidth, topButtonHeight);

        int sliderX = sidePadding;
        int sliderWidth = Math.Max(120, sidebarWidth - (sidePadding * 2));
        int sliderHeight = Math.Max(12, (int)(20 * uiScale));
        int sliderStartY = Math.Max(sidePadding + topButtonHeight + 30, (int)(100 * uiScale));
        int sliderGap = Math.Max(18, (int)(50 * uiScale));
        sliderPosition = new Vector2(sliderX, sliderStartY);
        sliderPosition2 = new Vector2(sliderX, sliderStartY + sliderGap);
        sliderPosition3 = new Vector2(sliderX, sliderStartY + (sliderGap * 2));
        sliderPosition4 = new Vector2(sliderX, sliderStartY + (sliderGap * 3));
        sliderPosition5 = new Vector2(sliderX, sliderStartY + (sliderGap * 4));

        int modeButtonGapX = topButtonGap;
        int modeButtonGapY = Math.Max(8, (int)(15 * uiScale));
        int modeButtonHeight = Math.Max(18, (int)(35 * uiScale));
        int modeButtonWidth = Math.Max(50, (sidebarWidth - (sidePadding * 2) - modeButtonGapX) / 2);

        int modeRows = 3;
        int modeAreaHeight = (modeRows * modeButtonHeight) + ((modeRows - 1) * modeButtonGapY);
        int modeBottomPadding = sidePadding;
        int minGapFromSliders = Math.Max(8, (int)(18 * uiScale));
        int gapColorsToModes = Math.Max(8, (int)(14 * uiScale));
        int sliderBottom = (int)sliderPosition5.Y + sliderHeight;
        int colorAreaTop = sliderBottom + minGapFromSliders;
        int modeStartY = viewportHeight - modeBottomPadding - modeAreaHeight;
        int availableColorHeight = modeStartY - gapColorsToModes - colorAreaTop;
        if (availableColorHeight < 1)
        {
            availableColorHeight = 1;
        }

        int rows = 11;
        int columns = 3;
        int minColorPadding = 1;
        int baseColorSize = Math.Max(20, (int)(45 * uiScale));
        int baseColorPadding = Math.Max(6, (int)(16 * uiScale));
        int maxSizeByWidth = Math.Max(4, (sidebarWidth - (sidePadding * 2) - (2 * minColorPadding)) / columns);
        int colorButtonWidth = Math.Min(baseColorSize, maxSizeByWidth);
        int colorPaddingX = Math.Min(baseColorPadding, Math.Max(1, (int)(colorButtonWidth * 0.30f)));
        int colorPaddingY = Math.Max(1, (int)(colorPaddingX * 0.75f));
        int colorButtonHeight = colorButtonWidth;
        int colorGridHeight = (rows * colorButtonHeight) + ((rows - 1) * colorPaddingY);
        if (colorGridHeight > availableColorHeight)
        {
            colorPaddingY = minColorPadding;
            colorButtonHeight = Math.Max(4, (availableColorHeight - ((rows - 1) * colorPaddingY)) / rows);
            colorGridHeight = (rows * colorButtonHeight) + ((rows - 1) * colorPaddingY);
        }

        int colorGridWidth = (columns * colorButtonWidth) + ((columns - 1) * colorPaddingX);
        int colorStartX = Math.Max(10, (sidebarWidth - colorGridWidth) / 2);
        int colorStartY = colorAreaTop + Math.Max(0, (availableColorHeight - colorGridHeight) / 2);
        for (int i = 0; i < colorButtons.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            int x = colorStartX + col * (colorButtonWidth + colorPaddingX);
            int y = colorStartY + row * (colorButtonHeight + colorPaddingY);
            colorButtons[i] = new Rectangle(x, y, colorButtonWidth, colorButtonHeight);
        }

        int colorBottom = colorStartY + colorGridHeight;
        if (colorBottom + gapColorsToModes > modeStartY)
        {
            modeStartY = colorBottom + gapColorsToModes;
        }

        for (int i = 0; i < modeButtons.Length; i++)
        {
            int row = i / 2;
            int col = i % 2;
            int x = sidePadding + col * (modeButtonWidth + modeButtonGapX);
            int y = modeStartY + row * (modeButtonHeight + modeButtonGapY);
            modeButtons[i] = new Rectangle(x, y, modeButtonWidth, modeButtonHeight);
        }

        if (spriteBatch != null)
        {
            RebuildUiTextures(sliderWidth, sliderHeight, topButtonWidth, topButtonHeight, modeButtonWidth, modeButtonHeight, colorButtonWidth);
        }
    }

    private void RebuildUiTextures(int sliderWidth, int sliderHeight, int topButtonWidth, int topButtonHeight, int modeButtonWidth, int modeButtonHeight, int colorButtonSize)
    {
        sidebarTexture?.Dispose();
        closeButtonTexture?.Dispose();
        sliderTexture?.Dispose();
        sliderTexture2?.Dispose();
        sliderTexture3?.Dispose();
        sliderTexture4?.Dispose();
        sliderTexture5?.Dispose();
        modeButtonTexture?.Dispose();
        colorButtonTexture?.Dispose();

        sidebarTexture = CreateColorTexture(sidebarWidth, GraphicsDevice.Viewport.Height, new Color(100, 100, 244));
        closeButtonTexture = CreateColorTexture(topButtonWidth, topButtonHeight, new Color(245, 245, 245));
        sliderTexture = CreateColorTexture(sliderWidth, sliderHeight, Color.Gray);
        sliderTexture2 = CreateColorTexture(sliderWidth, sliderHeight, Color.Gray);
        sliderTexture3 = CreateColorTexture(sliderWidth, sliderHeight, Color.Gray);
        sliderTexture4 = CreateColorTexture(sliderWidth, sliderHeight, Color.Gray);
        sliderTexture5 = CreateColorTexture(sliderWidth, sliderHeight, Color.Gray);
        modeButtonTexture = CreateColorTexture(modeButtonWidth, modeButtonHeight, Color.White);
        colorButtonTexture = CreateColorTexture(colorButtonSize, colorButtonSize, Color.White);
        InitializeHelpOverlayTextures();
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
        appState.AudioDeviceNumber = audioManager.SelectedInputDevice;
        appState.AudioCaptureSource = audioManager.CaptureSource.ToString();
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

        AudioCaptureSource requestedSource = AudioCaptureSource.Microphone;
        if (!string.IsNullOrWhiteSpace(state.AudioCaptureSource) && Enum.TryParse(state.AudioCaptureSource, out AudioCaptureSource parsedSource))
        {
            requestedSource = parsedSource;
        }
        audioManager.ApplySourceAndDevice(requestedSource, state.AudioDeviceNumber);

        visualizer.UpdateFrequencyCutoff(sliderValue2);
        float scaledValue = 1.0f + (100f - 1.0f) * sliderValue;
        visualizer.UpdateScaleFactor(scaledValue);

        svgPosition = state.GetSvgPosition();
        loadedMediaPath = state.LoadedMediaPath ?? string.Empty;

        if (string.IsNullOrWhiteSpace(loadedMediaPath))
        {
            svgTexture = null;
            return;
        }

        if (File.Exists(loadedMediaPath) && MediaLoader.IsSupportedFile(loadedMediaPath))
        {
            LoadMediaFromPath(loadedMediaPath, false);
        }
        else
        {
            svgTexture = null;
        }
    }
}
public enum AudioCaptureSource
{
    Microphone,
    SystemLoopback
}

public class AudioManager
{
    private IWaveIn activeCapture;
    private WaveFormat activeWaveFormat;
    private readonly object captureLock = new object();
    private Complex[] fftBuffer;
    public float[] FrequencyData { get; private set; }
    private int fftLength = 512;  // Must be a power of 2
    private int m;  // Log base 2 of fftLength
    public int FftLength => fftLength;
    public AudioCaptureSource CaptureSource { get; private set; } = AudioCaptureSource.Microphone;
    public int SelectedInputDevice { get; private set; } = 0;
    public int InputDeviceCount => WaveIn.DeviceCount;

    public AudioManager()
    {
        m = (int)Math.Log(fftLength, 2.0);
        fftBuffer = new Complex[fftLength];
        FrequencyData = new float[fftLength / 2];
    }

    public void Initialize(int fftLength = 64)
    {
        this.fftLength = fftLength;
        m = (int)Math.Log(fftLength, 2.0);
        fftBuffer = new Complex[fftLength];
        FrequencyData = new float[fftLength / 2];
        RestartCapture();
    }

    public void ApplySourceAndDevice(AudioCaptureSource source, int deviceNumber)
    {
        CaptureSource = source;
        if (InputDeviceCount <= 0)
        {
            SelectedInputDevice = 0;
        }
        else
        {
            SelectedInputDevice = Math.Clamp(deviceNumber, 0, InputDeviceCount - 1);
        }
        RestartCapture();
    }

    public void SetCaptureSource(AudioCaptureSource source)
    {
        if (CaptureSource == source)
        {
            return;
        }
        CaptureSource = source;
        RestartCapture();
    }

    public void SelectNextInputDevice()
    {
        if (InputDeviceCount <= 0)
        {
            return;
        }
        SelectedInputDevice = (SelectedInputDevice + 1) % InputDeviceCount;
        if (CaptureSource == AudioCaptureSource.Microphone)
        {
            RestartCapture();
        }
    }

    public void SelectPreviousInputDevice()
    {
        if (InputDeviceCount <= 0)
        {
            return;
        }
        SelectedInputDevice--;
        if (SelectedInputDevice < 0)
        {
            SelectedInputDevice = InputDeviceCount - 1;
        }
        if (CaptureSource == AudioCaptureSource.Microphone)
        {
            RestartCapture();
        }
    }

    public string GetSelectedInputDeviceName()
    {
        if (InputDeviceCount <= 0 || SelectedInputDevice < 0 || SelectedInputDevice >= InputDeviceCount)
        {
            return "No Input";
        }
        return WaveIn.GetCapabilities(SelectedInputDevice).ProductName;
    }

    public void UpdateFFTLength(int newLength)
    {
        lock (captureLock)
        {
            if (newLength == fftLength)
            {
                return;
            }

            fftLength = newLength;
            m = (int)Math.Log(fftLength, 2.0);
            fftBuffer = new Complex[fftLength];
            FrequencyData = new float[fftLength / 2];
        }
    }

    private void RestartCapture()
    {
        lock (captureLock)
        {
            StopCapture_NoLock();

            try
            {
                if (CaptureSource == AudioCaptureSource.SystemLoopback)
                {
                    var loopback = new WasapiLoopbackCapture();
                    loopback.DataAvailable += OnDataAvailable;
                    loopback.RecordingStopped += (sender, args) => Debug.WriteLine("Loopback capture stopped.");
                    activeWaveFormat = loopback.WaveFormat;
                    activeCapture = loopback;
                }
                else
                {
                    if (InputDeviceCount <= 0)
                    {
                        return;
                    }

                    SelectedInputDevice = Math.Clamp(SelectedInputDevice, 0, InputDeviceCount - 1);
                    var waveIn = new WaveInEvent
                    {
                        DeviceNumber = SelectedInputDevice,
                        WaveFormat = new WaveFormat(22050, 1)
                    };
                    waveIn.DataAvailable += OnDataAvailable;
                    waveIn.RecordingStopped += (sender, args) => Debug.WriteLine("Recording stopped.");
                    activeWaveFormat = waveIn.WaveFormat;
                    activeCapture = waveIn;
                }

                activeCapture.StartRecording();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing audio capture: {ex.Message}");
                StopCapture_NoLock();
            }
        }
    }

    private void OnDataAvailable(object sender, WaveInEventArgs args)
    {
        lock (captureLock)
        {
            if (activeWaveFormat == null || fftBuffer == null || fftBuffer.Length == 0)
            {
                return;
            }

            ConvertBytesToFloats(args.Buffer, args.BytesRecorded, fftBuffer, fftLength, activeWaveFormat);
            FastFourierTransform.FFT(true, m, fftBuffer);
            ProcessFFTResults();
        }
    }

    private static void ConvertBytesToFloats(byte[] buffer, int bytesRecorded, Complex[] fftBuffer, int fftLength, WaveFormat waveFormat)
    {
        Array.Clear(fftBuffer, 0, fftBuffer.Length);

        int channels = Math.Max(1, waveFormat.Channels);
        int bitsPerSample = waveFormat.BitsPerSample;
        int bytesPerSample = Math.Max(1, bitsPerSample / 8);
        int bytesPerFrame = Math.Max(bytesPerSample * channels, waveFormat.BlockAlign);
        int framesRecorded = bytesRecorded / bytesPerFrame;
        int framesToProcess = Math.Min(framesRecorded, fftLength);

        for (int frame = 0; frame < framesToProcess; frame++)
        {
            int frameOffset = frame * bytesPerFrame;
            float sampleSum = 0f;
            for (int channel = 0; channel < channels; channel++)
            {
                int sampleOffset = frameOffset + (channel * bytesPerSample);
                sampleSum += ReadSample(buffer, sampleOffset, waveFormat);
            }
            fftBuffer[frame].X = sampleSum / channels;
            fftBuffer[frame].Y = 0;
        }
    }

    private static float ReadSample(byte[] buffer, int offset, WaveFormat waveFormat)
    {
        if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat && waveFormat.BitsPerSample == 32)
        {
            return BitConverter.ToSingle(buffer, offset);
        }

        if (waveFormat.Encoding == WaveFormatEncoding.Pcm)
        {
            return waveFormat.BitsPerSample switch
            {
                8 => (buffer[offset] - 128) / 128f,
                16 => BitConverter.ToInt16(buffer, offset) / 32768f,
                24 => ReadPcm24(buffer, offset),
                32 => BitConverter.ToInt32(buffer, offset) / 2147483648f,
                _ => 0f
            };
        }

        return 0f;
    }

    private static float ReadPcm24(byte[] buffer, int offset)
    {
        int sample = buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16);
        if ((sample & 0x00800000) != 0)
        {
            sample |= unchecked((int)0xFF000000);
        }
        return sample / 8388608f;
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
        lock (captureLock)
        {
            StopCapture_NoLock();
        }
    }

    private void StopCapture_NoLock()
    {
        if (activeCapture != null)
        {
            try
            {
                activeCapture.StopRecording();
            }
            catch
            {
                // ignored
            }
            activeCapture.Dispose();
            activeCapture = null;
        }
        activeWaveFormat = null;
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
