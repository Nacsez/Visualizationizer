## Visualizationizer 1.0 ## Released June 2024 ##

Welcome to Visualizationizer 1.0, a light weight audio-visualizing application that transforms audio input into dynamically reactive visual outputs! This tool is designed for both hobbyists and professionals looking to enhance their audio presentations with visual flair and a simple non-distracting interface.

It is coded in C# using the free and open source library Monogame and Windows .NET system libraries. I plan to maintain this version going forward. It is available for no cost and for open source use under MIT license. If you feel inclined, you can support the project at (www.buymeacoffee.com/spry).

## Features ##

- **Dynamic Visualization**: Generates real-time visuals based on audio input. Sliders and Color Toggles provide direct control of output in real time.
- **Customizable Visual Modes**: Choose from several visual graphic equalizer modes! Available modes include Standard, MirroredMiddle, MirroredCorners, Radial, CenterColumn, and Puddle modes.
- **Textless UI Design**: Deliberately avoids bringing in text or other labels to the program to keep a clean and immersive design aesthetic. This allows for minimal distraction if adjustments are needed on the fly.
- **Load in SVG Files**: Able to load in SVG files to be sound reactive and placed on screen. SVG formatting needs to be specific and template included in the installation subfolder "SVGs". 1000x1000 pixels with same defined viewframe.
- **Windows Application**: Tested on some Windows 10 and 11 systems to be functional. I wouldn't call the testing comprehensive but seems stable from what I can tell.
 
## Installation ##

To get started with Visualizationizer 1.0, follow these steps:

1. **Download the Installer**: Navigate to the github page for Visualizationizer. [https://github.com/Nacsez/Visualizationizer] Find the Releases section and download the "Visualizationizer1.0Installer.exe"
2. **Run the Installer**: Execute the downloaded file and follow the on-screen instructions to install. Required libraries should be included in the installer so no internet connection should be required.
3. **Set up Default Windows Audio Input Device**: This application relies on the default audio input from your Windows session for visualization input. Please set this up before opening the application.
4. **Launch the Application**: Open the application from your desktop or start menu shortcut. If you are opening the program for the first time, you will be greeted with the tutorial.svg file.

## How To Use The Program

After launching Visualizationizer 1.0, it should automatically pick up the active audio input on your Windows session. There are no settings for audio input selection and it should automatically select the Windows Default.
If you do not have a default audio input device set in Windows, this application will not have any available data to display.

You will find most of this information in the tutorial.svg file. The controls are as follows:

- **Move the Mouse to Left Edge of the Screen**: Open control sidebar menu
- **Esc**: Quit the application.
- **F11**: Toggle full screen vs windowed mode.
- **Del**: Removes the loaded image from the view screen
- Sidebar buttons:
- **Top Left Button** closes the sidebar menu.
- **Top Right Button** loads SVG files into the view space
       -Holding SHIFT while pressing this button will allow you to change the file which loads up at startup
- **First Slider** is Amplitude Sensitivity. Lower values will result in smaller rectangles or wedges and larger values will increase the size until it reaches the limit for that mode.
- **Second Slider** is Cutoff Frequency. This effectively sets the total length of the equalizer output. Behaves differently in different modes.
- **Third Slider** is FFT Bin Number. This is an exponent of 2^(n) (16, 32, 64, 128, or 256). This is the number of sections which are used to represent the whole spectrum of input sound. If you increase this slider, it will increase the amount of math your computer has to do.
- **Fourth Slider** adjusts the size of the loaded SVG file in the view space.
- **Fifth Slider** adjusts the Perturbation Factor for the loaded SVG file. This can be set at Zero to stop the image from moving. Larger values will have the image get bigger as it reacts to the sound.
- **Color Toggle Buttons** control the available colors to assign to the visualization panel. Only colors which are activated will appear in the rendered panel. This is a good way to control the vibe or match with an imported file.
- **Mode Selection Buttons** below the color grid toggle between the different visual modes of the equalizer display. Available Visual Modes:
  -Standard-  This mode is similar to the classic linear graphic equalizer while still giving you control over the slider inputs and colors
  -MirroredCenter-  This mode mirrors the equalizer bars above and below the centerline but adds a gap on the bottom rectangles to create a shadow or reflection effect
  -MirroredCorners-  This mode mirrors the equalizer to start from each corner of the view screen and render inward toward the center. This has the effect of surrounding the outer perimeter of the screen with a color reaction that tends to fill in more on the edges. Scaling works somewhat differently in this mode
  -Radial-  This mode takes the linear equalizer and instead projects it radially around the center. It is mirrored by 180 degrees and will overlap upon itself with the cutoff slider pulled past half way by design. Some neat effects are possible by adjusting the color and slider values in this mode. It is also probably the most resource heavy mode in the program.
  -CenterColumn-  This mode is similar to mirrored corners except it reverses the bottom of the spectrum to be in the center of the screen. This has the effect of connecting the center in a column.
  -Puddle-  This mode is different from the rest in that it does not change shape with audio input but rather changes opacity. The concentric circles will render according to the slider settings but remain still once defined. The audio then maps to the opacity of the color in each circle.

## Contributing

Don't forget to give the project a star!

I am happy to hear your suggestions. If you come across bugs or have ideas for features, please open an issue or add comment to the github page. I created this as an open source project so that others may take what I did here and build on it for themselves. 
Here a suggested path should you want to add code to the project:
1. Fork the Project and do it yourself :D (go for it!)
  2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
  3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
  4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request for main branch
   
No promises on what will and will not be added to the main branch, but if its a neat feature and doesn't detract from the tone/intention of the program, it will likely be welcome. And even if it isn't, you can do what you like on your own with it. If you choose to take the software and do your own thing with it, best of luck! Please include attribution per the MIT License. 

I have some features in mind for myself to add in future versions and I can only imagine what others might want to do with this. I plan to keep this project open source and free for general use.

## Support the Project

This project is developed and maintained with ❤️. If you find it exciting, fun, amusing, fruitful or useful, please consider sending a donation to (www.buymeacoffee.com/spry) Your support helps keep me motivated and is greatly appreciated!

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact

Robert - Product Owner and Developer - visualizationizer@gmail.com

Project Link: [https://github.com/Nacsez/Visualizationizer]
