6-1-24 -- 6-2-24

-Made several minor changes and cleaned up the code over the past few days. Commented the code better as well and removed unneeded sections. Commented out most debugging sections. 
-Spent quite a while trying to get the installer right. Removed 32 bit compatibilitiy to shirnk the install file to less than 100mb. Current install file size is 82mb. 
-Adjusted folder permissions of installer and created SVGs folder on install with permissions to edit it. 
-Decided against adding the H key function because of complexity. May add it in future.
-SVG Tutorial created and automatically loaded after installation.
-Install media is uploaded to release page and release page draft made up

Still needed
-Still need to make video for release
-Need to figure out a website maybe?
-Need to figure out how to publicize this

5-29-24 -- 5-30-24

Added default SVG setting function by holding Shift + Clicking on Load SVG Button. This copies the SVG file to the SVGs content folder in the program folder. Hopefully the installer package will be able to set this up. Still need to do that part. 
Added CenterColumn and Puddle modes. Wasn't planning on adding more modes but CentralColumn was first found as a bug while trying to fix an issue with scaling in MirroredCorners mode. Fixed the scaling issue in MirroredCorners by adding a scaling function to the cutoff. It now will fill the screen and adjust the size of the rectangles to suit. 
Puddle mode has been kicking around in my head for a while and decided to implement it because 5 doesnt make the interface buttons even and 6 does. 
Got feedback on logo design and current design is likely to become the icon for the program. Need to work on making a proper logo with the radial mode implied behind the V
Still to do:
-Implement H button help feature
  -Load Font
  -Decide on Implementation Overlay vs Direct Render
-Create SVG Tutorial to load on install
-Build Install Media and test on several systems
  -need to make sure to package all .NET framework stuff as well as file folders with SVG packs
-Instructional and Promotional Material
-Website


5-27-24

Created Installation media and uninstaller with InnoSetup. Created several SVGs as well as a test logo which can be loaded in and used with the program. 

Features to add before release:
-Design and Add Logo and Icon for Program
-Create Logic to autoload SVG file on startup from "Content" folder
-Create SVG file to load on startup which servers as Tutorial
-Enable pressing the H key to bring up an overlay which has some symbols and words which describe what each button or section does
-Create Font?? Pick a font to use if I don't

Planned things to do outside of the software:
-Create Physical Media for Release Promo
-Create SVG Pack for SPRY standardize SVGs that come with program
-Create Final Installation Media 
  -TEST THIS OUT A COUPLE PLACES
-Create instructional video
-Create website
-Create demonstration video
-Create Screenshots for Publication


5-25-24

Memory leak is fixed. Added new colors up to 33 total including grey shades and changed the location of the colors to be more intuitive. Increased the side of sidebar slightly and plan to reorganize buttons just a bit before final release. Figured out that the viewspace limit for SVG input file is 1000 by 1000 pixels and it works reliably at 1:1 scaling. Might be able to get better resolution by messing with scaling of input files but it works well enough right now. I plan on making several SVG files and templates to accompany the program on installation.

Next step to get the SVG file to react with input sound and install a slider to control it size. I also want to add logic to move the SVG file when you click and drag it.

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

