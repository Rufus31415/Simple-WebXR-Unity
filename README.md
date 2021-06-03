![SimpleWebXR](/images/banner.png)


# What is SimpleWebXR ?
SimpleWebXR is a lightweight library that exposes the [WebXR](https://developer.mozilla.org/en-US/docs/Web/API/WebXR_Device_API) javascript API in your C# [Unity](https://unity.com/) code.
Thus, after a Unity [WebGL build](https://docs.unity3d.com/Manual/webgl-gettingstarted.html), your app can do augmented or virtual reality in the browser.

<p align="center">
 <a href="https://github.com/Rufus31415/Simple-WebXR-Unity/stargazers"><b>⭐ Star if you like it !</b></a>
</p>
<p align="center">
 <a href="https://github.com/Rufus31415/Simple-WebXR-Unity/watchers"><b>👁️ Watch to be notified of latest updates !</b></a>
</p>

---

# Compatible browsers
Works on :
- [Google Chrome](https://play.google.com/store/apps/details?id=com.android.chrome) on Android (:warning: a secure https connection is required)
- [Edge](https://docs.microsoft.com/fr-fr/windows/mixed-reality/new-microsoft-edge) on Windows 10 for Mixed Reality Headsets
- [Edge](https://docs.microsoft.com/en-us/hololens/hololens-insider#introducing-the-new-microsoft-edge) on Hololens 2
- [Firefox Reality](https://www.microsoft.com/en-gb/p/firefox-reality/9npq78m7nb0r?activetab=pivot:overviewtab) on Hololens 2
- [Mozilla WebXR Viewer](https://apps.apple.com/fr/app/webxr-viewer/id1295998056) on iOS  (:warning: WebGL 2.0 should be disabled, it's a Webkit experimental feature)
- [Oculus Browser](https://developer.oculus.com/webxr/?locale=fr_FR) on Oculus Quest 1 and 2
- [Firefox emulator](https://addons.mozilla.org/fr/firefox/addon/webxr-api-emulator/) on desktop
- [Chrome emulator](https://chrome.google.com/webstore/detail/webxr-api-emulator/mjddjgeghkdijejnciaefnkjmkafnnje) on desktop

---

# Integration examples

## MRTK
[Mixed Reality Toolkit](https://github.com/microsoft/MixedRealityToolkit-Unity) is a Microsoft-driven project that provides a set of components and features, used to accelerate cross-platform MR app development in Unity. It supports Hololens, Windows Mixed Reality headset, OpenVR, Ultraleap, Mobile devices and now **WebXR** !

The files in directory [/Assets/SimpleWebXR/Scripts/MRTK-Providers](https://github.com/Rufus31415/Simple-WebXR-Unity/tree/master/Assets/SimpleWebXR/Scripts/MRTK-Providers) add WebXR capabilities to MRTK with the following functions: controller tracking, **hand** tracking, hand ray, index pointer, grip pointer and spatial pointer. Teleportation could be added.

LIVE DEMO : 
- Hand interaction : [▶️ https://rufus31415.github.io/webxr/MRTK-HandInteraction](https://rufus31415.github.io/webxr/MRTK-HandInteraction/)
- Color picker : [▶️ https://rufus31415.github.io/webxr/MRTK-ColorPicker](https://rufus31415.github.io/webxr/MRTK-ColorPicker/)
- Elastic menus : [▶️ https://rufus31415.github.io/webxr/MRTK-ElasticSystem](https://rufus31415.github.io/webxr/MRTK-ElasticSystem/)
- Hand coach : [▶️ https://rufus31415.github.io/webxr/MRTK-HandCoach](https://rufus31415.github.io/webxr/MRTK-HandCoach/)
- Hand menu : [▶️ https://rufus31415.github.io/webxr/MRTK-HandMenuLayout](https://rufus31415.github.io/webxr/MRTK-HandMenuLayout/)
- Material gallery : [▶️ https://rufus31415.github.io/webxr/MRTK-MaterialGallery](https://rufus31415.github.io/webxr/MRTK-MaterialGallery/)
- Scrolling menus : [▶️ https://rufus31415.github.io/webxr/MRTK-ScrollingObjectCollection](https://rufus31415.github.io/webxr/MRTK-ScrollingObjectCollection/)
- Solvers : [▶️ https://rufus31415.github.io/webxr/MRTK-Solver](https://rufus31415.github.io/webxr/MRTK-Solver/)

| Hololens 2 | Oculus Quest |
|:-------------------------:|:-------------------------:|
|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/mrtk-hololens.gif">|  <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/mrtk-quest.gif" height="225px">|

| iOS | Emulator |
|:-------------------------:|:-------------------------:|
|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/mrtk-ios.gif">|  <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/mrtk-simulator.gif" height="225px">|

Fun **simulation** features in browser, you can simulate your Hololens, like in Unity editor :

| Mouse interaction | Hand simulation (MAJ/SPACE and T/Y) | Scene navigation |
|:-------------------------:|:-------------------------:|:-------------------------:|
|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/mrtk-mouse.gif"> |  <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/mrtk-hand-simulation.gif">|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/mrtk-move.gif">|


MRTK version : 2.6.1

---

## Simple hand tracking
SimpleWebXR supports hand tracking. This example displays a sphere on each joint of the detected hands. The radius of this sphere is given by the device. It was tested on Hololens 2 and Oculus Quest.

You need to set flags to enable hand tracking. In firefox reality, open setting panel and set ```dom.webxr.hands.enabled``` to true. In Oculus Browser, visit chrome://flags/ and enable #webxr-hands.


LIVE DEMO : [▶️ https://rufus31415.github.io/webxr/HandDetectionExample/](https://rufus31415.github.io/webxr/HandDetectionExample/)

<p align="center"><img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/hand-tracking.gif" height="150px"/></p>


---

## Spectator view
When wearing a HoloLens, we often forget that a person who does not have it on is unable to experience the wonders that we can. Spectator View allows others to see on a 2D screen what a HoloLens user sees in their world.
[This Microsoft project](https://github.com/microsoft/MixedReality-SpectatorView) is a native spectator view app for iOS and Android. But here, the spectator view is in your browser. WebXR is optional because you can walk around the Hololens space with the keyboard and mouse.

LIVE DEMO : [▶️ https://rufus31415.github.io/sandbox/simple-webxr-spectator/](https://rufus31415.github.io/webxr/SpectatorViewWebXRClient/)

DOWNLOAD : [⏬ Hololens 2 ARM appx and dependencies](https://github.com/Rufus31415/rufus31415.github.io/tree/master/webxr/SpectatorViewHololens2Server)
 
Scene for Hololens 2 (MRTK) : [/Assets/SimpleWebXR/Scenes](https://github.com/Rufus31415/Simple-WebXR-Unity/tree/master/Assets/SimpleWebXR/Scenes)/
SpectatorViewHololens2Server.unity. To compile, this scene, do not use UWP SDK 19041, it has socket server issues.

Unity scene for Mobile (WebGL) : [/Assets/SimpleWebXR/Scenes](https://github.com/Rufus31415/Simple-WebXR-Unity/tree/master/Assets/SimpleWebXR/Scenes)/
SpectatorViewWebXRClient.unity

| Mobile | Move in Hololens space with mouse/keyboard or Follow user head |
|:-------------------------:|:-------------------------:|
|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/spectator-view-ios.gif" height="150px"/>|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/spectator-view-desktop.gif" height="150px"/>|

Comparison with Microsoft solution :

|| Simple WebXR Spectator view | Microsoft native spectator view |
|:-------------------------:|:-------------------------:|:-------------------------:|
| iOS | YES : WebXR Viewer, Safari or Chrome (move in Hololens space with touch screen) | YES (ARKit) |
| Android | YES (Chrome) | YES (ARCore) |
| Desktop | YES, you can move in hololens space with mouse and keyboard  | NO |
|Communication Mobile/Hololens| Websocket | WebRTC |
|FPS| 10 | 60 |
|Calibration| Touch the screen | Scan a QR Code |
|Experimental| YES | NO |


## Paint example
This is a very basic example on how to use Simple WebXR. It uses Unity Line Renderer to draw lines in space with your hands/controllers.

LIVE DEMO : [▶️ https://rufus31415.github.io/webxr/PaintExample/](https://rufus31415.github.io/webxr/PaintExample/)

| Android | Hololens 2 | iOS | Quest | Emulator |
|:-------------------------:|:-------------------------:|:-------------------------:|:-------------------------:|:-------------------------:|
|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-android.gif"> |  <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-hololens.gif">|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-ios.gif">|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-quest.gif">  |  <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-simulator.gif">|

<p align="center">
  <a href="https://youtu.be/cUkAdI4lJOA">
    <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/paint-youtube.png" height="150px"/>
  </a>
</p>

Sources are [here](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/Assets/SimpleWebXR/Scripts/PaintExample/PenController.cs)

---

## XR CAD file viewer
Opens 45+ 3D CAD formats in your browser (FBX, STEP, OBJ, Collada, GLTF, OnShape, ...) and now, view them in VR/AR with WebXR !

LIVE DEMO : [▶️ https://rufus31415.github.io/sandbox/3d-viewer](https://rufus31415.github.io/sandbox/3d-viewer)

<p align="center"><img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/3d-viewer.gif" height="200px"/></p>


[work in progress...](https://github.com/Rufus31415/react-webgl-3d-viewer-demo)

---


## XRTK
The [Mixed Reality Toolkit](https://github.com/XRTK/XRTK-Core)'s primary focus is to make it extremely easy to get started creating Mixed Reality applications and to accelerate deployment to multiple platforms from the same Unity project.

LIVE DEMO : [▶️ https://rufus31415.github.io/sandbox/simple-webxr-xrtk-solvers/](https://rufus31415.github.io/sandbox/simple-webxr-xrtk-solvers)
| iOS | Emulator |
|:-------------------------:|:-------------------------:|
|<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/xrtk-solvers-ios.gif" height="225px"> |  <img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/xrtk-solvers-simulator.gif" height="225px">|

[work in progress...](https://github.com/XRTK/WebXR)

---

## HPTK

Hand Physics Toolkit (HPTK) is a toolkit to implement hand-driven interactions in a modular and scalable way. Platform-independent. Input-independent. Scale-independent.

I am currently studying the implementation of WebXR in this framework.

Original repo : https://github.com/jorgejgnz/HPTK-Sample

My fork and WebXR implementation : https://github.com/Rufus31415/HPTK-Sample-WebXR

LIVE DEMO : [▶️ https://rufus31415.github.io/webxr/HPTK/](https://rufus31415.github.io/webxr/HPTK/)

(![hptk](https://user-images.githubusercontent.com/22075796/120713557-60b36080-c4c2-11eb-93b3-a09faf711758.jpg))


---

# Quick start

## Import and build a sample scene

First create a new 3D project

![tuto_NewProject](https://user-images.githubusercontent.com/22075796/117204491-a5c77280-adf0-11eb-8b86-409dfe25424e.JPG)

Download the latest release of SimpleWebXR : https://github.com/Rufus31415/Simple-WebXR-Unity/releases
- ```SimpleWebXR.unitypackage```: contains only SimpleWebXR and its demo scenes
- ```SimpleWebXR+MRTK.unitypackage``` : contains SimpleWebXR addon for MRTK. MRTK should be initialized in your project, see : https://docs.microsoft.com/fr-fr/windows/mixed-reality/mrtk-unity/


Open the unitypackage file and import all resources : in tab ```Project```, right click on ```Assets > Import Package > Custom Package```

![image](https://user-images.githubusercontent.com/22075796/117205332-c93eed00-adf1-11eb-8426-49f5773fa17f.png)

From the directory ```Assets/SimpleWebXR/Example/Scenes```, just drag/drop a scene (for example the PaintExample) in the tab "Hierarchy"
. You can play the scene, but it won't do anything (except if you are playing a MRTK sample).

Then, build the scene : ```File > Build Settings...```. Remove all scenes from the list and click ```Add open scenes``` so that you only get the scene we are going to build.

Select the WebGL plateform and click ```Switch platform```. Then click the ```Build``` button and create and select a ```Build``` directory next to Assets.

![image](https://user-images.githubusercontent.com/22075796/117208767-ca721900-adf5-11eb-831f-14c16804052c.png)

## Run your build locally in your browser

Your browser should be compatible with WebXR. For a first try, you can install the emulator :
- For Chrome : https://chrome.google.com/webstore/detail/webxr-api-emulator/mjddjgeghkdijejnciaefnkjmkafnnje
- For Firefox : https://addons.mozilla.org/en-US/firefox/addon/webxr-api-emulator

You now need a http server to serve you files. I recommend this one : https://www.npmjs.com/package/http-server
- Just download node.js : https://nodejs.org/en/
- install the server in your system with the command ```npm install --global http-server```


You can now open a command line in your directory ```Build``` and run ```http-server```. Open your browser to the url : http://120.0.0.1:8080 then open the inspector and you should have a tab "WebXR" from where you can select your simulated device. You can move the controllers and the headset from here.

Now click the button "Start VR" to enter in immersion. Congrats !

![image](https://user-images.githubusercontent.com/22075796/117209832-32752f00-adf7-11eb-8c80-47a29058c1b4.png)


## Run your build on your smartphone or headset

You can continue to host the page on your PC and serve it to other devices. The difficulty is that most browsers require a secure context for WebXR, i.e. https or localhost.

So it's a bit more complicated, but not impossible ;) ! First you need a certificate :
- Download openssl. If you are on windows, download ```binaries``` from : http://gnuwin32.sourceforge.net/packages/openssl.htm
- Extract and run the command : ```openssl req -newkey rsa:2048 -new -nodes -x509 -days 3650 -keyout key.pem -out cert.pem -config **PATH_TO_OPENSSL**\share\openssl.cnf```where **PATH_TO_OPENSSL** is the absolute path to the directory you just extracted.
- This will generate the files ```cert.pem```and ```key.pem```
- Move these files in your Build directory

To serve your files, you should now run ```http-server -S -C cert.pem``` in your Build directory. You will see in the console all the URLs where the build is accessible. In your smartphone or headset, type the one with the same subnetwork than your PC. Ensure that your firewall accepts the request. On your device, the browser will say the page is not secure, but anyway, you can continue ;)



# Installation
Just add these 3 files in your Unity Asset folder, then add SimpleWebXR MonoBehavior on a game object in your scene.
- [SimpleWebXR.cs](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/com.rufus31415.simplewebxr/Runtime/Scripts/SimpleWebXR.cs): Mono Behaviour that displays the "Start AR" button and communicates with javascript. This behavior should be in your scene.
- [SimpleWebXR.jslib](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/com.rufus31415.simplewebxr/Runtime/Plugins/WebGL/SimpleWebXR.jslib): Javascript plugin that is included in the application and that makes the link between the Unity engine and the WebXR session. It displays the rendering and obtains the positions and characteristics of the camera.
- [SimpleWebXR.jspre](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/com.rufus31415.simplewebxr/Runtime/Plugins/WebGL/SimpleWebXR.jslib): Javascript plugin executed before the application that initializes a number of things.

## Download Unity Package
Download the latest release of SimpleWebXR from : https://github.com/Rufus31415/Simple-WebXR-Unity/releases
- ```SimpleWebXR.unitypackage```: contains only SimpleWebXR and its demo scenes
- ```SimpleWebXR+MRTK.unitypackage``` : contains SimpleWebXR addon for MRTK. MRTK should be initialized in your project, see : https://docs.microsoft.com/fr-fr/windows/mixed-reality/mrtk-unity/

## Add from Package Manager

You can add the package ```com.rufus31415.simplewebxr```from the Package Manager. Go to ```Window > Package Manager``` Click the button ```+ > add package from git URL``` and enter ```https://github.com/Rufus31415/Simple-WebXR-Unity.git?path=/com.rufus31415.simplewebxr/```, after clicking on ```add``` it can take minutes even if Unity doesn't seem busy.

![image](https://user-images.githubusercontent.com/22075796/117214228-ceee0000-adfc-11eb-994a-0eab9048565d.png)


## Edit ```manifest.json``` file

For the bravest, you can edit the file ```Packages/manifest.json``` so that it contains the line :
``` json
{
  "dependencies": {
    "com.rufus31415.simplewebxr": "https://github.com/Rufus31415/Simple-WebXR-Unity.git?path=/com.rufus31415.simplewebxr/",
    ...
    ...
  }
}
```

# Samples
You can use the examples provided in this repository as a starting point.

All sample builds can be dowloaded as a zip file from here : [⏬ https://github.com/Rufus31415/rufus31415.github.io/tree/master/webxr](https://minhaskamal.github.io/DownGit/#/home?url=https://github.com/Rufus31415/rufus31415.github.io/tree/master/webxr)

# Compilation
The project must be compiled in WebGL, otherwise SimpleWebXR will have no effect. You can use the "Demo" WebGL template that is provided, but you can also use the Default one. The project has been tested with Unity 2018.4 and Unity 2020.3.

# Runtime
When compiled as a WebGL app, if the browser is WebXR compatible, it will display a "Start AR" or "Start VR" button on your canvas.
You don't need a specific WebGL Template, so you can keep using yours.

# Get started
## SimpleWebXR MonoBehavior
To begin with, **it is recommended to have the SimpleWebXR component active in the scene** at all times. I recommend you to create a root game object "WebXR" which contains only the SimpleWebXR component.

This one will manage two things:
- It will display the "Start AR" or "Start VR" button at the bottom of the canvas, and will start the session if the user presses it.
- It will call to each frame the function ```SimpleWebXR.UpdateWebXR()```.

However SimpleWebXR may not be present as a component game object in the scene, but you will have to **call the static function ```SimpleWebXR.UpdateWebXR()``` by yourself at each frame**.

You can also add this component by code by calling the static method ```SimpleWebXR.EnsureInstance()```. The SimpleWebXR component does not exist, it will create it on a root game object "WebXR".

<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/monobehavior.png"/>

## Start/Stop immersive session
If the SimpleWebXR component is active in the scene, it will automatically display a button that allows the user to start an immersive WebXR session.

You can hide this button by checking the field ```Hide Start Button```.

In addition, you can at any time in the main thread start an immersive session from your code via the static method :

``` cs
SimpleWebXR.StartSession();
```

You can also end an immersive session by calling :
``` cs
SimpleWebXR.EndSession();
```

In addition, the following static events are raised when an immersive session starts or stops :
``` cs
⚡️ UnityEvent SimpleWebXR.SessionStart
⚡️ UnityEvent SimpleWebXR.SessionEnd

SimpleWebXR.SessionStart.AddListener(OnSessionStart); // OnSessionStart() is a method in your code called when a session starts
SimpleWebXR.SessionEnd.AddListener(OnSessionEnd); // OnSessionEnd() is a method in your code called when a session starts
```

## Check AR/VR supported
To find out if the current browser supports WebXR, you can call these static methods from anywhere in your code:
``` cs
bool isARSupported = SimpleWebXR.IsARSupported(); // 'immersive-ar' feature is supported
bool isVRSupported = SimpleWebXR.IsVRSupported(); // 'immersive-vr' feature is supported
```
Warning: the result of the request is asynchronous in javascript. It may be necessary to call these functions several times or to wait to get the result. For example, the SimpleWebXR component requests the capabilities at each frame in the OnGUI() function.

## Check if a WebXR Session is running
The static property ```SimpleWebXR.InSession``` indicates whether a WebXR immersive session is in progress.
``` cs
bool isInWebXRSession = SimpleWebXR.InSession;
```

## Eyes
On a smartphone and tablets, only one camera is required. But on HMD (head mounted headset), a different rendering is made for each eye, so there are two active cameras.

At the start of the session, the left eye is equal to ```Camera.Main```. If necessary a camera is created for the left eye. The characteristics of these cameras are modified at startup (clearFlags, background, clip planes, ...).

During a session, the pose and the projection matrix are updated when ```SimpleWebXR.UpdateWebXR()``` is called. 

The second camera (right eye) is destroyed at the end of the immersive session.

``` cs
Camera leftEye = SimpleWebXR.LeftEye; // == Camera.Main
Camera rightEye = SimpleWebXR.RightEye; // == null on smartphones and tablets
```

## Input controllers
The ```SimpleWebXR.LeftInput``` and ```SimpleWebXR.RightInput``` fields represent left and right controllers. On smartphones either of these input sources can be used (it depends on the browser) and corresponds to the place where the user touched the screen.

``` cs
WebXRInputSource leftInput = SimpleWebXR.LeftInput;
WebXRInputSource rightInput = SimpleWebXR.RightInput;
```

### Members of class ```WebXRInputSource```

``` cs
class WebXRInputSource {

  // ⚡️ Event triggered when the browser triggers a XRSession.selectend event, which means the input source has fully completed its primary action.
  // On Oculus Quest : Back trigger button was pressed
  // On Hololens 2 : A air tap has been was performed
  // On smartphones : The screen was touched
  public readonly UnityEvent Select;

  // ⚡️ Event triggered when the browser triggers a XRSession.selectstart event, which means the input source begins its primary action.
  public readonly UnityEvent SelectStart;

  // ⚡️ Event triggered when the browser triggers a XRSession.selectend event, which means the input source ends its primary action.
  public readonly UnityEvent SelectEnd;

  // ⚡️ Event triggered when the browser triggers a XRSession.selectend event, which means the input source has fully completed its primary squeeze action.
  // On Oculus Quest : Side grip button was pressed
  public UnityEvent Squeeze;

  // ⚡️ Event triggered when the browser triggers a XRSession.selectstart event, which means the input source begins its primary squeeze action.
  public UnityEvent SqueezeStart;

  // ⚡️ Event triggered when the browser triggers a XRSession.selectend event, which means the input source ends its primary squeeze action.
  public UnityEvent SqueezeEnd;

  // Indicates if the input source exists
  public bool Available;

  // Handedness of the input source
  // WebXRHandedness.Left : left input source
  // WebXRHandedness.Right : right input source
  public WebXRHandedness Handedness;

  // Indicates that the input source is detected and its position is tarcked
  public bool IsPositionTracked;

  // Current position of the input source if the position is tracked
  public Vector3 Position;

  // Current rotation of the input source if the position is tracked
  public Quaternion Rotation;

  // Number of axes available for this input source
  public int AxesCount;

  // Current value of each axes
  public float[] Axes;

  // Number of button for this input source
  public int ButtonsCount = 0;

  // Current state of each buttons
  public readonly WebXRGamepadButton[] Buttons;

  // Describes the method used to produce the target ray, and indicates how the application should present the target ray to the user if desired.
  // WebXRTargetRayModes.None : No event has yet identified the target ray mode
  // WebXRTargetRayModes.TrackedPointer : The target ray originates from either a handheld device or other hand-tracking mechanism and represents that the user is using their hands or the held device for pointing. The orientation of the target ray relative to the tracked object MUST follow platform-specific ergonomics guidelines when available. In the absence of platform-specific guidance, the target ray SHOULD point in the same direction as the user’s index finger if it was outstretched.
  // WebXRTargetRayModes.Screen : The input source was an interaction with the canvas element associated with an inline session’s output context, such as a mouse click or touch event.
  // WebXRTargetRayModes.Gaze : The target ray will originate at the viewer and follow the direction it is facing. (This is commonly referred to as a "gaze input" device in the context of head-mounted displays.)
  public WebXRTargetRayModes TargetRayMode;

  //The input source primary action is active
  // On Oculus Quest : Back trigger button is pressed
  // On Hololens 2 : A air tap is performed
  // On smartphones : The screen is touched
  public bool Selected;

  // The input source primary squeeze action is active
  // On Oculus Quest : Side grip button is pressed
  public bool Squeezed;

  // Constains hand joints poses, if hand tracking is enabled
  public WebXRHand Hand; 

  // Applies haptic pulse feedback to a controller 
  // intensity : Feedback strength between 0 and 1
  // duration : Feedback duration in milliseconds
  public void HapticPulse(float intensity, float duration)
}
```

Also, the events select, squeeze, ... can be handled at the ```SimpleWebXR``` class level via the following ```WebXRInputEvent``` static events where the first argument is the input source that raised it.
``` cs
⚡️ WebXRInputEvent SimpleWebXR.InputSourceSelect
⚡️ WebXRInputEvent SimpleWebXR.InputSourceSelectStart
⚡️ WebXRInputEvent SimpleWebXR.InputSourceSelectEnd
⚡️ WebXRInputEvent SimpleWebXR.InputSourceSqueeze
⚡️ WebXRInputEvent SimpleWebXR.InputSourceSqueezeStart
⚡️ WebXRInputEvent SimpleWebXR.InputSourceSqueezeEnd

// Event triggered when a input sources has been added or removed.
⚡️ UnityEvent SimpleWebXR.InputSourcesChange
```

Similarly, haptic feedback on controllers can also be called at the static ```SimpleWebXR``` class level.

``` cs
  // Applies haptic pulse feedback to a controller 
  // hand : Left or Right
  // intensity : Feedback strength between 0 and 1
  // duration : Feedback duration in milliseconds
  public static void HapticPulse(WebXRHandedness hand, float intensity, float duration)
```


### Members of class ```WebXRGamepadButton```
```WebXRGamepadButton``` Describes a button, trigger, thumbstick, or touchpad data.

This class maps the information retrieved in javascript via the WebXR Gamepads Module API : https://www.w3.org/TR/webxr-gamepads-module-1

``` cs
class WebXRGamepadButton {

  // The amount which the button has been pressed, between 0.0 and 1.0, for buttons that have an analog sensor
  public float Value;

  // The touched state of the button
  public bool Touched;
  
  // The pressed state of the button
  public bool Pressed;   
}
```

### Members of class ```WebXRHand```
```WebXRHand``` describes the poses of hand skeleton joints

This class maps the information retrieved in javascript via the WebXR Hand Input Module API : https://www.w3.org/TR/webxr-hand-input-1

``` cs
class WebXRHand {

  // Indicates if hand tracking is available
  public bool Available;

  // Poses of hand skeleton joints
  public WebXRJoint[] Joints;
  
  // 25 joints are tracked
  public const int JOINT_COUNT = 25;
  
  // Index of each joint in Joints array :  
  public const int WRIST = 0;

  public const int THUMB_METACARPAL = 1;
  public const int THUMB_PHALANX_PROXIMAL = 2;
  public const int THUMB_PHALANX_DISTAL = 3;
  public const int THUMB_PHALANX_TIP = 4;

  public const int INDEX_METACARPAL = 5;
  public const int INDEX_PHALANX_PROXIMAL = 6;
  public const int INDEX_PHALANX_INTERMEDIATE = 7;
  public const int INDEX_PHALANX_DISTAL = 8;
  public const int INDEX_PHALANX_TIP = 9;

  public const int MIDDLE_METACARPAL = 10;
  public const int MIDDLE_PHALANX_PROXIMAL = 11;
  public const int MIDDLE_PHALANX_INTERMEDIATE = 12;
  public const int MIDDLE_PHALANX_DISTAL = 13;
  public const int MIDDLE_PHALANX_TIP = 14;

  public const int RING_METACARPAL = 15;
  public const int RING_PHALANX_PROXIMAL = 16;
  public const int RING_PHALANX_INTERMEDIATE = 17;
  public const int RING_PHALANX_DISTAL = 18;
  public const int RING_PHALANX_TIP = 19;

  public const int LITTLE_METACARPAL = 20;
  public const int LITTLE_PHALANX_PROXIMAL = 21;
  public const int LITTLE_PHALANX_INTERMEDIATE = 22;
  public const int LITTLE_PHALANX_DISTAL = 23;
  public const int LITTLE_PHALANX_TIP = 24;
}
```

<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/hand-layout.svg"/>


### Members of class ```WebXRJoint```
```WebXRJoint``` describes a joint of a hand. Each hand is made up many bones, connected by joints.

``` cs
class WebXRJoint {

  // Position of the joint
  public Vector3 Position;
  
  // Rotatiuon of the joint
  public Quaternion Rotation;

  // Optional joint radius that can be used to represent the joint has a sphere.
  // float.NaN if not supported
  public float Radius;
}
```

<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/bg.jpg"/>


# Useful WebXR content
- [https://immersive-web.github.io/](https://immersive-web.github.io/) : Many useful examples
- [https://www.w3.org/TR/webxr](https://www.w3.org/TR/webxr) : technical details of webxr
- [https://github.com/MozillaReality/unity-webxr-export](https://github.com/MozillaReality/unity-webxr-export) : a WebXR export plugin developed by Mozilla (much more complicated than this one, which is based on a js template and external js resources)
- [https://github.com/De-Panther/unity-webxr-export](https://github.com/De-Panther/unity-webxr-export) : A fork of Mozilla plugin that works in AR and VR (Btw, thanks to De-Panther for his useful help !).


# License: MIT 😘
[©Rufus31415](https://rufus31415.github.io)

See the [license file](https://github.com/Rufus31415/Simple-WebXR-Unity/blob/master/LICENSE) for details.

# Sponsor 💛
You are more than welcome to sponsor me !

| ![❤️ Sponsor](https://github.com/sponsors/Rufus31415/)  |
|---|

In order to maintain SimpleWebXR and ensure its proper functioning on all platforms, I have to acquire a lot of expensive equipment.

I currently have: Microsoft Hololens 2, Oculus Quest 1 & 2, Acer WMR Headset, iPad and iPhone.

What I would like to buy soon : Magic Leap, Android tablet and smartphone

<table>
  <tbody>
    <tr>
      <th align="center"><br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
      </th>
     <th align="center">Sponsors<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
      </th>
      <th align="center"><br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
      </th>
    </tr>
   <tr>
    <td></td>
    <td><a href="https://github.com/StephenHodgson"><img align="left" height="32" src="https://github.com/StephenHodgson.png?size=40"/> Stephen Hodgson</a></td>
    <td></td>
    </tr>
   <tr>
    <td></td>
    <td><a href="https://github.com/MauriceFrank"><img align="left" height="32" src="https://github.com/MauriceFrank.png?size=40"/> Maurice Frank</a></td>
    <td></td>
    </tr>
  </tbody>
</table>

<p align="center">
<img src="https://raw.githubusercontent.com/Rufus31415/Simple-WebXR-Unity/master/images/xr-devices.jpg" width="50%"/>
</p>


# Any questions ?
Feel free to contact me :
- Slack : https://holodevelopers.slack.com/team/U0120CVDUCV
- Discord : Rufus31415#2440
- Twitter : https://twitter.com/rufus31415
- Mail : rufus31415@gmail.com
- Clubhouse : @fgi
- Open issue : https://github.com/Rufus31415/Simple-WebXR-Unity/issues


<details>
<summary></summary>
<img src="https://ga-beacon.appspot.com/UA-163892314-2/Simple-WebXR-Unity/">
</details>
