5-22-24 20 hours later

Found memory leak after doing some extended session testing in the radial mode. Added MIT license file to repository. Reorganized prouduction notes and README. 

Program seems to be working quite nicely though. very happy with the reponsiveness and color outputs. still need to work on:

-get the SVG file to move with the input music. 
-Also now found the need to move the picture by clicking and dragging it with the mouse. 
-may be nice to resize it as well but unsure how to implemnt the resize. 
  -May need to put that on another silder
-need to adjust the colors and add one more row of buttons that have neon versions of each color and a solid black and white
-need to resolve memory leak in radial mode

  
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

