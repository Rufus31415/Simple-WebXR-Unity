/**
 * This script will be included in the generated WebAssembly.
 * It is used to manage the WebXR session by displaying the rendering and transferring the camera's characteristics and positions to Unity.
 */

mergeInto(LibraryManager.library, {

  // Makes the background of the canvas transparent (see : https://support.unity3d.com/hc/en-us/articles/208892946-How-can-I-make-the-canvas-transparent-on-WebGL-)
  glClear: function (mask) {
    if (mask == 0x00004000) {
      var v = GLctx.getParameter(GLctx.COLOR_WRITEMASK);
      if (!v[0] && !v[1] && !v[2] && v[3])
        return;
    }
    GLctx.clear(mask);
  },


  // Initialize WebXR features and check if browser is compatible
  InitWebXR: function (dataArray, dataArrayLength, byteArray, byteArrayLength) {
    _isVrSupported = false;
    _isArSupported = false;

    if (!navigator.xr) return;

    // Check if WebXR immersive AR is supported
    navigator.xr.isSessionSupported('immersive-ar').then(function (supported) {
      _isArSupported = supported;
    });

    // Check if WebXR immersive VR is supported
    navigator.xr.isSessionSupported('immersive-vr').then(function (supported) {
      _isVrSupported = supported;
    });

    // Initialize a pointer to the shared array that contains camera settings (projection matrix, position, orientation)
    _dataArray = new Float32Array(buffer, dataArray, dataArrayLength);

    _byteArray = new Uint8Array(buffer, byteArray, byteArrayLength);

    // Set view ete mask in shared buffer
    _dataArraySetViewEye = function (mask) { _byteArray[0] = mask; }

    // Copy poses data in shared buffer
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

    _dataArraySetInputSource = function (inputSource, id, frame) {
      var floatStartId = id * 7 + 54;

      var byteStartId = 3 * id + 4;

      if (_arSession.localFloorSpace) {
        var targetRayPose = frame.getPose(inputSource.targetRaySpace, _arSession.localFloorSpace);

        if(!targetRayPose) frame.getPose(inputSource.targetRaySpace, _arSession.localSpace);

        if (targetRayPose) {
          _byteArray[byteStartId + 0] = 1;
          _dataArray[floatStartId + 0] = targetRayPose.transform.position.x;
          _dataArray[floatStartId + 1] = targetRayPose.transform.position.y;
          _dataArray[floatStartId + 2] = targetRayPose.transform.position.z;
          _dataArray[floatStartId + 3] = targetRayPose.transform.orientation.x;
          _dataArray[floatStartId + 4] = targetRayPose.transform.orientation.y;
          _dataArray[floatStartId + 5] = targetRayPose.transform.orientation.z;
          _dataArray[floatStartId + 6] = targetRayPose.transform.orientation.w;
         // str = str + "\r\n Position : " + JSON.stringify(targetRayPose.transform.position);
        }
        else {
          _byteArray[byteArray] = 0;
        }
      }
      else {
        _byteArray[byteArray] = 0;
      }

      // JSON.stringify(inputSource.profiles); // generic-hand, generic-hand-
      if (inputSource.gamepad) {
        _byteArray[byteStartId + 1] = inputSource.gamepad.axes.length;
        _byteArray[byteStartId + 2] = inputSource.gamepad.buttons.length;

        //str = str + "\r\n Axes : " + JSON.stringify(inputSource.gamepad.axes);
        //str = str + "\r\n Buttons : " + JSON.stringify(inputSource.gamepad.buttons);
      }
      else {
        _byteArray[byteStartId + 1] = 0;
        _byteArray[byteStartId + 2] = 0;
      }

      if (inputSource.targetRayMode) {

      }

      //stringToUTF8(str, _stringArray, lengthBytesUTF8(str) + 1);
    }

    // Declare a pointer to the session (set in StartSession())
    _arSession = null;

    var _controllerEvents = [0, 0];
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

          // Flag for available data
          _dataArraySetViewEye(viewEye);
        }
      }

      // Propagate controller events and acknowledge event
      _byteArray[1] |= _controllerEvents[0];
      _byteArray[2] |= _controllerEvents[1];
      _byteArray[3] = _inputSourcesChangeEvent;
      _controllerEvents[0] = 0;
      _controllerEvents[1] = 0;
      _inputSourcesChangeEvent = 0;


      for (var i = 0; i < _arSession.inputSources.length; i++) {
        var inputSource = _arSession.inputSources[i];

        if (inputSource.handedness === "right") {
          _dataArraySetInputSource(inputSource, 1, frame);
        }
        else {
          _dataArraySetInputSource(inputSource, 0, frame);
        }
      }
    }

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

    // get the current module. Works only if there is 1 Unity WebGL app in this page. I need to think of a better way to do this...
    //var module = window.UnityLoader.Blobs[Object.keys(window.UnityLoader.Blobs)[0]].Module;

    // Access Unity internal Browser and override its requestAnimationFrame
    // It's a good idea found by Mozilla : https://github.com/MozillaReality/unity-webxr-export/blob/c8a6a4ee71a3d890b513fc4cd950ccd238973844/Assets/WebGLTemplates/WebXR/webxr.js#L144
    Module.InternalBrowser.requestAnimationFrame = function (func) {
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

  // Return true if immersive AR is supported. InitWebXR must have been called first.
  IsArSupported: function () {
    return _isArSupported;
  },


  // Return true if immersive VR is supported. InitWebXR must have been called first.
  IsVrSupported: function () {
    return _isVrSupported;
  },


  // Starts the WebXR session.
  InternalStartSession: function () {
    if (!_isVrSupported && !_isArSupported) return;

    console.log("Start WebXR session...");

    // Request the WebXR session and create the WebGL layer
    navigator.xr.requestSession(_isVrSupported ? 'immersive-vr' : 'immersive-ar', { optionalFeatures: ['local-floor'] }).then(function (session) {
      _arSession = session;
      _canvasWidth = GLctx.canvas.width;
      _canvasHeight = GLctx.canvas.height;

      session.isInSession = true;

      var glLayer = new XRWebGLLayer(session, GLctx);

      session.updateRenderState({ baseLayer: glLayer });

      session.addEventListener('end', function () {
        _byteArray[0] = 0;
        _arSession = null;
        GLctx.canvas.width = _canvasWidth;
        GLctx.canvas.height = _canvasHeight;
      });

      session.addEventListener('select', _onSessionEvent);
      session.addEventListener('selectstart', _onSessionEvent);
      session.addEventListener('selectend', _onSessionEvent);
      session.addEventListener('squeeze', _onSessionEvent);
      session.addEventListener('squeezestart', _onSessionEvent);
      session.addEventListener('squeezeend', _onSessionEvent);

      session.addEventListener('inputsourceschange', function () {
        _inputSourcesChangeEvent = 1;
      });

      GLctx.canvas.width = glLayer.framebufferWidth;
      GLctx.canvas.height = glLayer.framebufferHeight;


      session.requestReferenceSpace('local-floor').then(function (space) {
        _arSession.localFloorSpace = space;
      });

      session.requestReferenceSpace('local').then(function (space) {
        _arSession.localSpace = space;
      });

    });
  },

  // Ends the session
  InternalEndSession: function () {
    if (_arSession) _arSession.end();
  }
});