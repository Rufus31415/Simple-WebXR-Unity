![SimpleWebXR](/images/banner.png)

# What is SimpleWebXR ?
SimpleWebXR is a lightweight library that exposes the WebXR javascript API in your C# Unity code.
Thus, after a Unity WebGL build, your app can do augmented or virtual reality in the browser.

# Try it
## Paint
LIVE DEMO : [▶️ https://rufus31415.github.io/sandbox/simple-webxr-paint/](https://rufus31415.github.io/sandbox/webxr-hand-interaction/)

| Android | Hololens 2 | iOS | Quest | Emulator |
|:-------------------------:|:-------------------------:|:-------------------------:|:-------------------------:|:-------------------------:|
|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-android.gif"> |  <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-hololens.gif">|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-ios.gif">|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-quest.gif">  |  <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-simulator.gif">|



Sources are [here](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/Samples/SimpleWebXRDemo/Assets/Samples/Paint/PenController.cs)

Unity version : 2018.4.21f1

## CAD file viewer
[work in progress...](https://github.com/Rufus31415/react-webgl-3d-viewer-demo)

## MRTK
LIVE DEMO : [▶️ https://rufus31415.github.io/sandbox/webxr-hand-interaction/](https://rufus31415.github.io/sandbox/webxr-hand-interaction/)

## XRTK
[work in progress...](https://github.com/Rufus31415/WebXR)



# Library files
- [SimpleWebXR.cs](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/SimpleWebXR.cs): Mono Behaviour that displays the "Start AR" button and communicates with javascript. This behavior should be in your scene.
- [SimpleWebXR.jslib](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/SimpleWebXR.jslib): Javascript plugin that is included in the application and that makes the link between the Unity engine and the WebXR session. It displays the rendering and obtains the positions and characteristics of the camera.
- [SimpleWebXR.jspre](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/SimpleWebXR.jspre): Javascript plugin executed before the application that initializes a number of things.

# Tested browsers
Works on :
- [Google Chrome](https://play.google.com/store/apps/details?id=com.android.chrome) on Android
- [Firefox Reality](https://www.microsoft.com/en-gb/p/firefox-reality/9npq78m7nb0r?activetab=pivot:overviewtab) on Hololens 2
- [Mozilla WebXR Viewer](https://apps.apple.com/fr/app/webxr-viewer/id1295998056) on iOS
- [Oculus Browser](https://developer.oculus.com/webxr/?locale=fr_FR) on Oculus Quest
- [Firefox emulator](https://addons.mozilla.org/fr/firefox/addon/webxr-api-emulator/) on desktop
- [Chrome emulator](https://chrome.google.com/webstore/detail/webxr-api-emulator/mjddjgeghkdijejnciaefnkjmkafnnje) on desktop

Work in progress for :
- [Firefox Reality](https://www.oculus.com/experiences/quest/2180252408763702/?locale=fr_FR) on Oculus Quest
- don't hesitate to tell me about browsers you've tried...


# Useful WebXR content
- [https://immersive-web.github.io/](https://immersive-web.github.io/) : Many useful examples
- [https://www.w3.org/TR/webxr](https://www.w3.org/TR/webxr) : technical details of webxr
- [https://github.com/MozillaReality/unity-webxr-export](https://github.com/MozillaReality/unity-webxr-export) : a WebXR export plugin developed by Mozilla (much more complicated than this one, which is based on a js template and external js resources)
- [https://github.com/De-Panther/unity-webxr-export](https://github.com/De-Panther/unity-webxr-export) : A fork of Mozilla plugin that works in AR and VR (Btw, thanks to De-Panther for his useful help !).


# License: MIT 😘
[©Rufus31415](https://rufus31415.github.io)

See the [license file](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/LICENSE) for details.

---

Feel free to open issues :)
