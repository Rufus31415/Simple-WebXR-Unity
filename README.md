
See the simple demo : https://rufus31415.github.io/sandbox/simple-webxr-unity/

More about WebXR : https://rufus31415.github.io/tag/WebXR/


Unity version : 2018.4.21f1


This prototype shows a simple implementation of WebXR with Unity 3D.
The project is compiled in WebGL and is based on ~250 lines of code divided into 3 files :
- [SimpleWebXR.cs](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/Assets/SimpleWebXR.cs): Behaviour that displays the "Start AR" button and writes the position of the hand camera.
- [SimpleWebXR.jslib](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/Assets/SimpleWebXR.jslib): Javascript plugin that is included in the application and that makes the link between the Unity engine and the WebXR session. It displays the rendering and obtains the positions and characteristics of the camera.
- [SimpleWebXR.jspre](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/Assets/SimpleWebXR.jspre): Javascript plugin executed before the application that initializes a number of things.

This example works on devices that support WebXR ('immersive-ar' and 'immersive-vr'). Devices with 1 view (known as magic windows like smartphones) or with 2 views (stereo, one for each eyes, like Hololens, Quest, Vive...) are supported.

Tested on:
- [WebXR Viewer application on iOS](https://apps.apple.com/us/app/webxr-viewer/id1295998056).
- Firefox Reality on Hololens 2
- Firefox and Chrome WebXR emulator on Windows
- Chrome on Android

Working on controllers and hand support...
 
Here are the resources that allowed me to move forward with this project:
- [https://immersive-web.github.io/](https://immersive-web.github.io/) : Many useful examples
- [https://www.w3.org/TR/webxr](https://www.w3.org/TR/webxr) : technical details of webxr
- [https://github.com/MozillaReality/unity-webxr-export](https://github.com/MozillaReality/unity-webxr-export) : a WebXR export plugin developed by Mozilla (much more complicated than this one, which is based on a js template and external js resources)

![](https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/simple-webxr1.gif)
![](https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/simple-webxr2.gif)

Feel free to open issues :)
