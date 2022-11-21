# Magic Mirror

Magic Mirror is a virtual fitting room application that imitates the process of trying clothes on in Augmented Reality using Microsoft Kinect v2 device. The application was created as a practical part of Bachelor thesis available here: http://hdl.handle.net/10467/99196
The quality of thesis solution was considered exceptional and graded with A.

The solution requires an installation of Kinect for Windows SDK 2.0 and Kinect Studio.  MS Kinect v2 is also required for motion tracing.

## User instructions

1. Plug in Kinect v2 to your PC.
2. Download Kinect drivers and Kinect SDK.
3. Launch Kinect Studio v2.0 and connect Kinect in it.
4. Launch the application executable KinectMirror.exe located in Build
folder.
5. Stand in front of the monitor, look at your mirrored image on the display.
You have to see your body fully from head to toes.
6. Navigate to the Outfits tab using a computer mouse. Choose a preferable
body type and adjust it to match your own body by clicking the Adjust
Body button.
7. Select one piece of clothing from each category:
(a) top - lower layer
(b) top - upper layer
(c) bottom
(d) shoes
(e) hat
(f) gloves
8. Try moving around the room and/or posing.
9. Swap selected parts of clothing with any other ones that are available in
the menu.
10. Try picking clothes with gestures. To do so, navigate to the preferred
piece of clothing with a hand cursor and make a fist to click.
11. Either with a computer mouse or with a hand cursor navigate to the
Settings tab. You may want to adjust hand cursors positions in the bottom
section of the Settings tab.
12. Either with a computer mouse or with a hand cursor navigate to the
Environment tab. Change Kinect Visualization type to Point cloud.
13. Using a PC mouse adjust points size and/or density to your liking.
14. Click on Remove Background check box and select the Futuristic 1 room.
Navigate to the Outfits tab again and pick some clothes. Try to move
around the area and/or pose.
15. Navigate back to the Environment tab and select the Futuristic 2 room.
Repeat the try-on experience from the previous step.
16. Navigate to the Settings tab and select dynamic camera. Keep in mind
that the gestures are not working in dynamic camera mode.
(a) Using Off-Axis Settings section in the Settings tab adjust position
and rotation of the screen relatively to your Kinect device. If needed,
the screen size can be modified as well.
(b) Walk around the room to see, if this effect reminds you of the way
mirror reflection works.
(c) Using a PC mouse navigate to clothes selection once more, pick some
clothes.
(d) Navigate to the Settings tab and switch camera back to static.
17. Navigate to the Environment tab and select the Playground room.
(a) Put your right palm inside the big yellow object. This should spawn
some random 3d primitives to the scene.
(b) Try to grab a 3d object with your hand.
18. You have now completed the test. To close the application, click on the
Quit button

## UI
[ Main menu buttons: ]
| Button | Action |
| ------ | ------ |
| Settings | Opens settings tab |
| Environment | Opens environment variants ta |
| Outfits | Opens outfit selecting tab |
| Quit | Shuts down the application |

[ Settings tab: ]
| Button | Action |
| ------ | ------ |
| Camera - static/dynamic | Changes camera mode |
| Off-Axis Settings | Contains adjustments of the screen relatively to Kinect device positio |
| Debugging | Toggles various debugging tool |
| Miscellaneous | Several miscellaneous tweaks |
| Hand cursors | Gesture input settings |

[ Environment tab: ]

| Button | Action |
| ------ | ------ |
| Kinect visualization | Switches between flat screen or point cloud visualisation |
| Rooms | Room Selection; better to use it with point cloud with removed background |


[ Outfits tab: ]

| Button | Action |
| ------ | ------ |
| Body type selector | Select most suitable body typ |

Body adjustment buttons:
| Button | Action |
| ------ | ------ |
|Reset body | Resets body adjustment|
| Adjust body | Adjusts body proportions to improve visualization quality |
| Category toggle | Filter outfits by enabling or disabling its categories |
| Outfit buttons | Equip or ”unequip” an outfit |


[ Debug panel: ]
- Is enabled in settings window
- Shows fps and visualizes internal data