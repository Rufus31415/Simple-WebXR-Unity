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
  InitWebXR: function (dataArray) {
    console.log("WebXR init...");

    // Check if WebXR immersive AR is supported
    _isArSupported = false;
    if (navigator.xr) {
      navigator.xr.isSessionSupported('immersive-ar').then(function (supported) {
        _isArSupported = supported;
      });
    }

    // Initialize a pointer to the shared array that contains camera settings (projection matrix, position, orientation)
    _dataArray = new Float32Array(buffer, dataArray, 24);

    // Declare a pointer to the session (set in StartSession())
    _arSession = null;

    // Overloaded function that draws the canvas and retrieves the position and projection matrix of the camera.
    _requestAnimationFrame = function (frame) {
      if (!_arSession) return;

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

      // Share projection matrix
      for (var i = 0; i < 16; i++) _dataArray[i] = pose.views[0].projectionMatrix[i];

      // Share position
      var position = pose.views[0].transform.position;
      _dataArray[16] = position.x;
      _dataArray[17] = position.y;
      _dataArray[18] = position.z;

      // Share orientation
      var orientation = pose.views[0].transform.orientation;
      _dataArray[19] = orientation.x;
      _dataArray[20] = orientation.y;
      _dataArray[21] = orientation.z;
      _dataArray[22] = orientation.w;

      // Flag for available data
      _dataArray[23] = 1;
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


  // Starts the WebXR session.
  StartSession: function () {
    if (!_isArSupported) return;

    console.log("Start WebXR session...");

    // Request the WebXR session and create the WebGL layer
    navigator.xr.requestSession('immersive-ar').then(function (session) {
      _arSession = session;
      session.isInSession = true;

      var glLayer = new XRWebGLLayer(session, GLctx);

      session.updateRenderState({ baseLayer: glLayer });

      //session.addEventListener('end', onSessionEnded); // implement end of session later...

      GLctx.canvas.width = glLayer.framebufferWidth;
      GLctx.canvas.height = glLayer.framebufferHeight;

      session.requestReferenceSpace('local').then(function (space) {
        session.refSpace = space;
      });
    });
  }
});