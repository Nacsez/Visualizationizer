Still in progress -----------


# Vizualizationizer 1.0

Welcome to Vizualizationizer 1.0, a light-weight audio-visualizing application that transforms audio input into dynamically reactive visual outputs! This tool is designed for both hobbyists and professionals looking to enhance their audio presentations with visual flair with non-distractinng interface which allows for continued emersion even when adjusting settings.
It is coded in C# using the free and open source visual library Monogame as well as some standard Windows libraries. I plan to maintain my own version going forward. It is availabe for open sourece use under MIT license. 

## Features

- **Dynamic Visualization**: Generates real-time visuals based on audio input. Sliders and Color Toggles provide direct control of output dynamically.
- **Customizable Visual Modes**: Choose from several visual graphgic equalizer modes such as Standard, MirroredMiddle, MirroredCorners, and Radial.
- **Textless UI Design**: Deliberately avoids bringing in text or other labels to the program to keep a clean and emersive design asthetic.
- **Load in SVG Files**: Able to load in SVG files to be sound reactive and placed on screen. SVG formatting needs to be specific and template included in the program folder.
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
- **H Key**: Toggle Help Overlay while pressed
- **Move the Mouse to Left Edge of the Screen**: Open control sidebar menu
- **Sidebar buttons function as follows:**
  -**Top Left button** closes sidebar menu.
  -**Top Right button** loads SVG files into the viewspace
  -**First Slider** is Cutoff Frequency. This effectively shortens the total length of the equalizer output.
  -**Second Slider** is FFT Bin Number. This is an exponent of 2^(n) between 8 and 1024. This is the number of renctangles which are used to represent the whole spectrum of input sound. If you increase this slider, it will increase the amount of math your computer has to do.
  -**Third Slider** is input sensitivity. Lower values will result in smaller rectangles or wedges and larger values will increase the size until it reaches the limit for that mode.
  -**Fourth Slider** Controls the Size of the imported SVG texture big to small
  -**Fith Slider** Sound Reactivity for the imported SVG. All the way to the left will stop the image from moving and all the way to the right wil be very sensitive to autio amplitude.
  -**33 Color Toggle Buttons** control the available colors to assign to the vizualization panel. Only colors which are activated will appear in the rendered panel. This is a good way to control the vibe or match with an imported file.
  -**4 Visualizer Mode Buttons** below the color grid toggle between the different visual modes of the equalizer display. Available Visual Modes:
      ~Standard~  This mode is similar to the classic linear graphic equalizer while still giving you control over the slider inputs and colors
      ~MirroredCenter~  This mode mirrors the equalizer bars above and below the centerline bnut adds a gap on the bottom rectangels to create a shadow or reflection effect
      ~MirroredCorners~  This mode mirros the equalizer to start from each corner of the view screen and render inward toward the center. This has the effect of surroudnding the outer perimeter of the screen with color reaction whiich tends to fill in more on the edges.
      ~Radial!~  This mode take the linear equalizer and instead projects it radially around the center. It is mirrored by 180 degrees and will overlap upon itself with the cutoff slider pulled past half way by design. Some neat effects are possible by adjusting the color and slider values in this mode. It is also probably the most resourece heavy mode in the program.
 
## Contributing
NEED TO UPDATE------------

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".

Don't forget to give the project a star! Thanks again!
Here a suggested path should you want to add code to the project:
1. Fork the Project and do it yourself :D (go for it!)
  2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
  3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
  4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request for main branch
   No promises on what will and will not be added to the main branch, but if its a neat feature and doesn't detract from the tone/intention of the program, it will likely be welcome. And even if it isn't, this is an open source software which you can use the code from with attribution. You can do what you like on your own with it as long as you say where it came from. 



## Support the Project

This project is developed and maintained with ❤️. If you find it useful, please consider supporting it by using buymeacoffee.com/spry for a donation. Your support helps keep me motivated to continue contributing and is greatly appreciated!

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact
       UPDATEE-------------------------

Project Email for all Visualizationizer related business or inquiries:

visualizationizer@gmail.com 

Project Link: [https://github.com/yourusername/projectname](https://github.com/yourusername/projectname)
