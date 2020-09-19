/**
 * This script will be included in the generated WebAssembly.
 * It is used to manage the WebXR session by displaying the rendering and transferring the camera's characteristics and positions to Unity.
 */

mergeInto(LibraryManager.library, {
  /****************************************************************************/
  // Makes the background of the canvas transparent (see : https://support.unity3d.com/hc/en-us/articles/208892946-How-can-I-make-the-canvas-transparent-on-WebGL-)
  glClear: function (mask) {
    if (GLctx.ARSessionStarted && mask == 0x00004000) {
      var v = GLctx.getParameter(GLctx.COLOR_WRITEMASK);
      if (!v[0] && !v[1] && !v[2] && v[3])
        return;
    }
    GLctx.clear(mask);
  },

  /****************************************************************************/
  // Initialize WebXR features and check if browser is compatible
  InitWebXR: function (dataArray, dataArrayLength, byteArray, byteArrayLength, handDataArray, handDataArrayLength) {
    _isVrSupported = false;
    _isArSupported = false;

    if (!navigator.xr) return;

    // Check if WebXR immersive VR is supported (check immersive-vr before immersive-ar to make it work on Oculus Quest Browser)
    navigator.xr.isSessionSupported('immersive-vr').then(function (supported) {
      _isVrSupported = supported;
    });

    // Check if WebXR immersive AR is supported
    navigator.xr.isSessionSupported('immersive-ar').then(function (supported) {
      _isArSupported = supported;
    });

    // Initialize pointers to shared arrays that contains data (projection matrix, position, orientation, input sources)
    _dataArray = new Float32Array(buffer, dataArray, dataArrayLength);
    _byteArray = new Uint8Array(buffer, byteArray, byteArrayLength);
    _handArray = new Float32Array(buffer, handDataArray, handDataArrayLength);

    _useLocalSpaceForInput = true;

    _rAFCB = null;

    // copy camera data in shared buffer
    _dataArraySetView = function (view, id) {
      var floatStartId = id * 27;

      // Share projection matrix
      for (var i = 0; i < 16; i++) _dataArray[floatStartId + i] = view.projectionMatrix[i];

      // Share position
      var position = view.transform.position;

      _dataArray[floatStartId + 16] = position.x;
      _dataArray[floatStartId + 17] = position.y;
      _dataArray[floatStartId + 18] = position.z;

      // Share orientation
      var orientation = view.transform.orientation;
      _dataArray[floatStartId + 19] = orientation.x;
      _dataArray[floatStartId + 20] = orientation.y;
      _dataArray[floatStartId + 21] = orientation.z;
      _dataArray[floatStartId + 22] = orientation.w;

      // Share viewport
      var viewport = _arSession.renderState.baseLayer.getViewport(view);

      if (viewport) {
        var width = GLctx.canvas.width;
        var height = GLctx.canvas.height;
        _dataArray[floatStartId + 23] = viewport.x / width;
        _dataArray[floatStartId + 24] = viewport.y / height;
        _dataArray[floatStartId + 25] = viewport.width / width;
        _dataArray[floatStartId + 26] = viewport.height / height;
      }
    }

    // copy input sources data in shared buffer
    _dataArraySetInputSource = function (inputSource, id, frame) {
      var floatStartId = id * 23 + 54;
      var byteStartId = id * 20 + 4;

      // Get input source pose
      if (_arSession.localSpace) {

        var targetRayPose = frame.getPose(inputSource.targetRaySpace, _useLocalSpaceForInput ? _arSession.localSpace : _arSession.localFloorSpace);

        // On Firefox Reality on Hololens 2 targetRayPose is undefined so we use local-floor and substract UserHeight to position.y.
        // We toggle _useLocalSpaceForInput so we only get one call to getPose() next time.
        if (!targetRayPose) {
          _useLocalSpaceForInput = false;
          targetRayPose = frame.getPose(inputSource.targetRaySpace, _arSession.localFloorSpace);
        }

        if (targetRayPose) {
          _byteArray[byteStartId] = 1;
          _dataArray[floatStartId] = targetRayPose.transform.position.x;
          _dataArray[floatStartId + 1] = targetRayPose.transform.position.y - (_useLocalSpaceForInput ? 0 : _dataArray[100]);
          _dataArray[floatStartId + 2] = targetRayPose.transform.position.z;
          _dataArray[floatStartId + 3] = targetRayPose.transform.orientation.x;
          _dataArray[floatStartId + 4] = targetRayPose.transform.orientation.y;
          _dataArray[floatStartId + 5] = targetRayPose.transform.orientation.z;
          _dataArray[floatStartId + 6] = targetRayPose.transform.orientation.w;
        }
        else {
          _byteArray[byteStartId] = 0;
        }
      }
      else {
        _byteArray[byteStartId] = 0;
      }


      if (inputSource.gamepad) {
        // Get input source axes
        if (inputSource.gamepad.axes) {
          _byteArray[byteStartId + 2] = inputSource.gamepad.axes.length;

          for (var i = 0; i < inputSource.gamepad.axes.length; i++) {
            if (i >= 8) break;
            _dataArray[floatStartId + 7 + i] = inputSource.gamepad.axes[i];
          }
        }
        else {
          _byteArray[byteStartId + 2] = 0;
        }

        // Get input source buttons
        if (inputSource.gamepad.buttons) {
          _byteArray[byteStartId + 3] = inputSource.gamepad.buttons.length;

          for (var i = 0; i < inputSource.gamepad.buttons.length; i++) {
            if (i >= 8) break;

            var button = inputSource.gamepad.buttons[i];
            _byteArray[byteStartId + 4 + i] = +button.touched;
            _byteArray[byteStartId + 12 + i] = +button.pressed;

            _dataArray[floatStartId + 15 + i] = button.value;
          }
        }
        else {
          _byteArray[byteStartId + 3] = 0
        }
      }
      else {
        _byteArray[byteStartId + 2] = 0;
        _byteArray[byteStartId + 3] = 0;
      }

      // get input source target ray mode
      if (inputSource.targetRayMode) {
        switch (inputSource.targetRayMode) {
          case "tracked-pointer":
            _byteArray[byteStartId + 1] = 1;
            break;
          case "screen":
            _byteArray[byteStartId + 1] = 2;
            break;
          case "gaze":
            _byteArray[byteStartId + 1] = 3;
            break;
          default:
            _byteArray[byteStartId + 1] = 0;
        }
      }
      else {
        _byteArray[byteStartId + 1] = 0;
      }

      // Hand detection
      // https://immersive-web.github.io/webxr-hand-input/#skeleton-joints-section
      if (inputSource.hand && inputSource.hand.length == 25) {
        _byteArray[46 + id] = 1; // hand supported
        var delta = (_useLocalSpaceForInput ? 0 : _dataArray[100])
        for (var j = 0; j < 25; j++) {
          if (inputSource.hand[j] !== null) {
            var joint = frame.getJointPose(inputSource.hand[j], _useLocalSpaceForInput ? _arSession.localSpace : _arSession.localFloorSpace);
            if (joint !== null) {
              var i = id * 200 + j * 8;
              _handArray[i] = joint.transform.position.x;
              _handArray[i + 1] = joint.transform.position.y - delta;
              _handArray[i + 2] = joint.transform.position.z;
              _handArray[i + 3] = joint.transform.orientation.x;
              _handArray[i + 4] = joint.transform.orientation.y;
              _handArray[i + 5] = joint.transform.orientation.z;
              _handArray[i + 6] = joint.transform.orientation.w;
              if (joint.radius !== null) {
                _handArray[i + 7] = joint.radius;
              }
              else {
                _handArray[i + 7] = NaN;
              }
            }
            else {
              _byteArray[46 + id] = 0; // hand not fully supported
            }
          }
        }
      }
      else {
        _byteArray[46 + id] = 0; // hand not supported
      }
    }

    // Declare a pointer to the session (set in StartSession())
    _arSession = null;

    // bit mask that contains controller events to propagate
    _controllerEvents = [0, 0];

    // flag that indicates input source change
    _inputSourcesChangeEvent = 0;


    // Overloaded function that draws the canvas and retrieves the position and projection matrix of the camera.
    _requestAnimationFrame = function (frame) {
      if (!_isArSupported && !_isVrSupported) return;

      var glLayer = _arSession.renderState.baseLayer;

      // If the size of the canvas has changed (change of orientation of the smartphone for example)
      if (GLctx.canvas.width != glLayer.framebufferWidth || GLctx.canvas.height != glLayer.framebufferHeight) {
        GLctx.canvas.width = glLayer.framebufferWidth;
        GLctx.canvas.height = glLayer.framebufferHeight;
      }

      // Render !
      GLctx.bindFramebuffer(GLctx.FRAMEBUFFER, glLayer.framebuffer);

      if (_arSession.localSpace) {
        // Get position of camera
        var pose = frame.getViewerPose(_arSession.localSpace);
        if (pose) {
          // Bit mask that indicates which view to render
          var viewEye = 0;

          // Transmit data poses to Unity
          for (var i = 0; i < pose.views.length; i++) {
            if (pose.views[i].eye === 'right') {
              _dataArraySetView(pose.views[i], 1);
              viewEye = viewEye | 2;
            } else {
              _dataArraySetView(pose.views[i], 0);
              viewEye = viewEye | 1;
            }
          }

          // Indicate which eyes should be considered
          _byteArray[0] = viewEye;
        }

        // Estimate user height at first frame
        if (_dataArray[100] == 0 && _arSession.localFloorSpace) {
          var poseFloor = frame.getViewerPose(_arSession.localFloorSpace);
          if (poseFloor) _dataArray[100] = poseFloor.views[0].transform.position.y - pose.views[0].transform.position.y;
        }
      }

      // Propagate controller events and acknowledge event
      _byteArray[1] |= _controllerEvents[0];
      _byteArray[2] |= _controllerEvents[1];
      _byteArray[3] = _inputSourcesChangeEvent;
      _controllerEvents[0] = 0;
      _controllerEvents[1] = 0;
      _inputSourcesChangeEvent = 0;

      var hasLeftInput = 0;
      var hasRightInput = 0;

      // copy input sources data
      for (var i = 0; i < _arSession.inputSources.length; i++) {
        var inputSource = _arSession.inputSources[i];

        if (inputSource.handedness === "right") {
          _dataArraySetInputSource(inputSource, 1, frame);
          hasRightInput = 1;
        }
        else {
          _dataArraySetInputSource(inputSource, 0, frame);
          hasLeftInput = 1;
        }
      }

      // Set input validity
      _byteArray[44] = hasLeftInput;
      _byteArray[45] = hasRightInput;
    }

    // input source event handler
    _onSessionEvent = function (event) {
      if (event.type && event.inputSource && event.inputSource.handedness) {

        var id = ((event.inputSource.handedness == "right") ? 1 : 0);

        var eventTypeMask = _controllerEvents[id];

        switch (event.type) {
          case "squeezestart":
            eventTypeMask |= 1;
            break;
          case "squeeze":
            eventTypeMask |= 2;
            break;
          case "squeezeend":
            eventTypeMask |= 4;
            break;
          case "selectstart":
            eventTypeMask |= 8;
            break;
          case "select":
            eventTypeMask |= 16;
            break;
          case "selectend":
            eventTypeMask |= 32;
            break;
        }

        _controllerEvents[id] = eventTypeMask;
      }
    };

    // Access Unity internal Browser and override its requestAnimationFrame
    // It's a good idea found by Mozilla : https://github.com/MozillaReality/unity-webxr-export/blob/c8a6a4ee71a3d890b513fc4cd950ccd238973844/Assets/WebGLTemplates/WebXR/webxr.js#L144
    Browser.requestAnimationFrame = function (func) {
      if (!_rAFCB) _rAFCB = func;

      if (_arSession && _arSession.isInSession) {
        return _arSession.requestAnimationFrame(function (time, xrFrame) {
          _requestAnimationFrame(xrFrame);
          func(time);
        });
      } else {
        window.requestAnimationFrame(func);
      }
    };

    // bindFramebuffer frameBufferObject null in XRSession should use XRWebGLLayer FBO instead
    GLctx.bindFramebuffer = function (oldBindFramebuffer) {
      return function (target, fbo) {
        if (!fbo) {
          if (_arSession && _arSession.isInSession) {
            if (_arSession.renderState.baseLayer) {
              fbo = _arSession.renderState.baseLayer.framebuffer;
            }
          }
        }
        return oldBindFramebuffer.call(this, target, fbo);
      }
    }(GLctx.bindFramebuffer);
  },

  /****************************************************************************/
  // Return true if immersive AR is supported. InitWebXR must have been called first.
  IsArSupported: function () {
    return _isArSupported;
  },

  /****************************************************************************/
  // Return true if immersive VR is supported. InitWebXR must have been called first.
  IsVrSupported: function () {
    return _isVrSupported;
  },

  /****************************************************************************/
  // Starts the WebXR session.
  InternalStartSession: function () {
    if (!_isVrSupported && !_isArSupported) return;
    console.log("Start WebXR session...");


    // Request the WebXR session and create the WebGL layer
    // In first approach, AR is prioritary over VR
    navigator.xr.requestSession(_isArSupported ? 'immersive-ar' : 'immersive-vr', { optionalFeatures: ['local-floor', 'hand-tracking'] }).then(function (session) {
      _arSession = session;
      _canvasWidth = GLctx.canvas.width;
      _canvasHeight = GLctx.canvas.height;

      GLctx.ARSessionStarted = _isArSupported;
      session.isInSession = true; // add field in session to indicate that a session in running

      var glLayer = new XRWebGLLayer(session, GLctx);

      session.updateRenderState({ baseLayer: glLayer });

      // reinitialize canvas when session end
      session.addEventListener('end', function () {
        _byteArray[0] = 0;
        _arSession.isInSession = false;
        _arSession = null;
        GLctx.canvas.width = _canvasWidth;
        GLctx.canvas.height = _canvasHeight;
        GLctx.ARSessionStarted = false;
      });

      // handle input sources events
      session.addEventListener('select', _onSessionEvent);
      session.addEventListener('selectstart', _onSessionEvent);
      session.addEventListener('selectend', _onSessionEvent);
      session.addEventListener('squeeze', _onSessionEvent);
      session.addEventListener('squeezestart', _onSessionEvent);
      session.addEventListener('squeezeend', _onSessionEvent);

      // Raise flag when input sources change
      session.addEventListener('inputsourceschange', function () {
        _inputSourcesChangeEvent = 1;
      });

      GLctx.canvas.width = glLayer.framebufferWidth;
      GLctx.canvas.height = glLayer.framebufferHeight;

      // get working reference space
      session.requestReferenceSpace('local-floor').then(function (space) {
        _arSession.localFloorSpace = space;
      });
      session.requestReferenceSpace('local').then(function (space) {
        _arSession.localSpace = space;

        // requestAnimationFrame should be call to make in work on Quest
        Browser.requestAnimationFrame(_rAFCB);
      });
    });
  },

  /****************************************************************************/
  // Ends the session
  InternalEndSession: function () {
    if (_arSession) _arSession.end();
  },

  /****************************************************************************/
  // Poll device orientation
  InternalGetDeviceOrientation: function (orientationArray, orientationInfo) {
    _orientationArray = new Float32Array(buffer, orientationArray, 3);
    _orientationInfo = new Uint8Array(buffer, orientationInfo, 1);

    _orientationInfo[0] = 0;

    _onDeviceOrientation = function (event) {
      _orientationInfo[0] = 1;
      _orientationArray[0] = event.alpha;
      _orientationArray[1] = event.beta;
      _orientationArray[2] = event.gamma;
    }

    if (CustomEvent && DeviceMotionEvent && typeof DeviceMotionEvent.requestPermission === "function") {
      document.dispatchEvent(new CustomEvent("SimpleWebXRNeedMotionPermission"));
    }

    window.addEventListener("deviceorientation", _onDeviceOrientation);
  }
});