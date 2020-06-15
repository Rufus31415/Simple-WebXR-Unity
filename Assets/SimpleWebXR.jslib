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
  InitWebXR: function (dataArray, dataArrayLength) {
    console.log("WebXR init...");

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

    // Set view count in shared buffer
    _dataArraySetViewCount = function (count) { _dataArray[0] = count; }

    // Copy poses data in shared buffer
    _dataArraySetView = function (view, viewId) {
      var startIndex = viewId * 27 + 1;

      // Share projection matrix
      for (var i = 0; i < 16; i++) _dataArray[startIndex + i] = view.projectionMatrix[i];

      // Share position
      var position = view.transform.position;
      _dataArray[startIndex + 16] = position.x;
      _dataArray[startIndex + 17] = position.y;
      _dataArray[startIndex + 18] = position.z;

      // Share orientation
      var orientation = view.transform.orientation;
      _dataArray[startIndex + 19] = orientation.x;
      _dataArray[startIndex + 20] = orientation.y;
      _dataArray[startIndex + 21] = orientation.z;
      _dataArray[startIndex + 22] = orientation.w;

      // Share viewport
      var viewport = _arSession.renderState.baseLayer.getViewport(view);

      if (viewport) {
        var width = GLctx.canvas.width;
        var height = GLctx.canvas.height;
        _dataArray[startIndex + 23] = viewport.x / width;
        _dataArray[startIndex + 24] = viewport.y / height;
        _dataArray[startIndex + 25] = viewport.width / width;
        _dataArray[startIndex + 26] = viewport.height / height;
      }
    }

    // Declare a pointer to the session (set in StartSession())
    _arSession = null;

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

      // Get position of camera
      var pose = frame.getViewerPose(_arSession.refSpace);
      if (!pose) return;


      // Flag for available data
      _dataArraySetViewCount(pose.views.length);

      // Transmit data poses to Unity
      for (var i = 0; i < pose.views.length; i++) {
        if (pose.views[i].eye === 'right') {
          _dataArraySetView(pose.views[i], 1);
        } else {
          _dataArraySetView(pose.views[i], 0);
        }
      }
    }

    // get the current module. Works only if there is 1 Unity WebGL app in this page. I need to think of a better way to do this...
    var module = window.UnityLoader.Blobs[Object.keys(window.UnityLoader.Blobs)[0]].Module;

    // Access Unity internal Browser and override its requestAnimationFrame
    // It's a good idea found by Mozilla : https://github.com/MozillaReality/unity-webxr-export/blob/c8a6a4ee71a3d890b513fc4cd950ccd238973844/Assets/WebGLTemplates/WebXR/webxr.js#L144
    module.InternalBrowser.requestAnimationFrame = function (func) {
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
    navigator.xr.requestSession(_isVrSupported ? 'immersive-vr' : 'immersive-ar').then(function (session) {
      _arSession = session;
      _canvasWidth = GLctx.canvas.width;
      _canvasHeight = GLctx.canvas.height;

      session.isInSession = true;

      var glLayer = new XRWebGLLayer(session, GLctx);

      session.updateRenderState({ baseLayer: glLayer });

      session.addEventListener('end', function(){
        _dataArray[0] = 0;
        _arSession = null;
        GLctx.canvas.width = _canvasWidth;
        GLctx.canvas.height = _canvasHeight;
      });

      GLctx.canvas.width = glLayer.framebufferWidth;
      GLctx.canvas.height = glLayer.framebufferHeight;

      session.requestReferenceSpace('local').then(function (space) {
        session.refSpace = space;
      });
    });
  },

  // Ends the session
  InternalEndSession: function () {
    if(_arSession) _arSession.end();
  }
});