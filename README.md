Still in progress -----------

# Vizualizationizer 1.0

Welcome to Vizualizationizer 1.0, a dynamic audio-visualizing application that transforms audio input into dynamically reactive visual outputs. This tool is designed for both hobbyists and professionals looking to enhance their audio presentations with visual flair with a lightweight and non-distractinng interface.f
It is coded in C# using the free and open source library monogame. I plan to maintain my own version for a while at least. It is availabe for open sourece use under MIT license. 

## Features

- **Dynamic Visualization**: Generates real-time visuals based on audio input. Sliders and Color Toggles provide direct control of output dynamically.
- **Customizable Visual Modes**: Choose from several visual graphgic equalizer modes such as Standard, MirroredMiddle, MirroredCorners, and Radial.
- **Textless UI Design**: Deliberately aboids bringing in text or other labels to the program to keep a clean and emersive design asthetic.
- **Load in SVG Files**: Able to load in SVG files to be sound reactive and placed on screen. SVG formatting needs to be specific and template included in git project (working on it)
- **Windows Application**: Runs on Windows and potentially other operating systems in future updates.
  
## Installation

To get started with Vizualizationizer 1.0, follow these steps:

UPDATE THIS LINE----------1. **Download the Installer**: Navigate to the [Releases](link-to-releases-page) page and download the latest version.
2. **Run the Installer**: Execute the downloaded file and follow the on-screen instructions to install.
3. **Launch the Application**: Open the application from your desktop or start menu shortcut.

## How To Use The Program

After launching Visualizationizer 1.0, it should automatically pick up the active default audio input device on your Wubdiws session. 
If you do not have one set in Windows, this application will not have any available data to display.

You can use the following controls to interact with the application:
- **Esc**: Quit the application.
- **F11**: Toggle fullscreen vs windowed mode.
- **Move the Mouse to Left Edge of the Screen**: Open control sidebar menu
- **Sidebar buttons function as follows:**
  -Top Left button closes sidebar menu.
  -Top Right button loads SVG files into the viewspace
  -First Slider is Cutoff Frequency. This effectively shortens the total length of the equalizer output.
  -Second Slider is FFT Bin Number. This is an exponent of 2^(n) between 8 and 1024. This is the number of renctangles which are used to represent the whole spectrum of input sound. If you increase this slider, it will increase the amount of math your computer has to do.
  -Third Slider is input sensitivity. Lower values will result in smaller rectangles or wedges and larger values will increase the size until it reaches the limit for that mode.
  -Fourth Slider planned for SVG size (not yet implemented)
  -Fith Slider planned for SVG sound reactivity level (not yet implemented)
  -Color Toggle Buttons control the available colors to assign to the vizualization panel. Only colors which are activated will appear in the rendered panel. This is a good way to control the vibe or match with an imported file.
  -Buttons below the color grid toggle between the different visual modes of the equalizer display. Available Visual Modes:
      ~Standard~  This mode is similar to the classic linear graphic equalizer while still giving you control over the slider inputs and colors
      ~MirroredCenter~  This mode mirrors the equalizer bars above and below the centerline bnut adds a gap on the bottom rectangels to create a shadow or reflection effect
      ~MirroredCorners~  This mode mirros the equalizer to start from each corner of the view screen and render inward toward the center. This has the effect of surroudnding the outer perimeter of the screen with color reaction whiich tends to fill in more on the edges.
      ~Radial!~  This mode take the linear equalizer and instead projects it radially around the center. It is mirrored by 180 degrees and will overlap upon itself with the cutoff slider pulled past half way by design. Some neat effects are possible by adjusting the color and slider values in this mode. It is also probably the most resourece heavy mode in the program.
 
## Contributing
NEED TO UPDATE------------
Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".

Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Support the Project

This project is developed and maintained with ❤️. If you find it useful, please consider supporting it by buymeacoffee.com/spry Your support helps keep me motivated and is greatly appreciated!

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact
       UPDATEE-------------------------
Your Name - your-email@example.com 

Project Link: [https://github.com/yourusername/projectname](https://github.com/yourusername/projectname)
