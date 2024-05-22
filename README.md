5-22-24

Added new visualization modes MirroredCenter, MirroredCorners, and Radial. Also adjusted sensitivity to be reactive and dialed in default values. Added buttons to control new modes and seems to be working smootly. Added ESC key to exit program.

5-21-24

This is a passion project intended to help teach me C# coding while giving me a light weight sound reactive viszualization program to run though a projector or on a screen. 

It requires an audio input on the device it is run on in order to generate FFT data which is then passed to the visualizers. 

Color toggles control the available colors.

Sliders control the cutoff frequency, # of bins and input sensitivity

First box closes the sidebar, second box opens the "Load SVG" dialog which will parse an SVG and display it in the center (still buggy)

F11 to enter or exit full screen mode (also buggy)

Planned features:

Toggles to add symetric visualization modes

SVG parsing logic improvements to center and scale input images

Sound reactive "jiggling" of the loaded SVG (gets bigger when louder)

