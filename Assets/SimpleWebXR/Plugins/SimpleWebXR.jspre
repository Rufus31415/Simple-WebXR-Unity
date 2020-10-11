/**
 * This plugin contains javascript will be executed before the WebGL application starts.
 */

// A workaround to make it work under Firefox Reality that does not implement isContextLost()
if(!WebGLRenderingContext.prototype.isContextLost) WebGLRenderingContext.prototype.isContextLost = function() {return false;}


 // The following code was developed by Mozilla and provides a pointer to the internal Unity Browser directly from the modules.
 // It also allows to add the XRCompatible attribute to true. (we could also do this via makeXRCompatible())
 // https://github.com/MozillaReality/unity-webxr-export/blob/75d4f429f7b53fe719f3f52c82c739fcc1740d08/Assets/WebXR/Plugins/WebGL/webxr.jspre
setTimeout(function () {
  if (GL && GL.createContext) {
    GL.createContextOld = GL.createContext;
    GL.createContext = function (canvas, webGLContextAttributes) {
      var contextAttributes = {
        xrCompatible: true
      };

      if (webGLContextAttributes) {
        for (var attribute in webGLContextAttributes) {
          contextAttributes[attribute] = webGLContextAttributes[attribute];
        }
      }

      return GL.createContextOld(canvas, contextAttributes);
    }
  }
}, 0);
