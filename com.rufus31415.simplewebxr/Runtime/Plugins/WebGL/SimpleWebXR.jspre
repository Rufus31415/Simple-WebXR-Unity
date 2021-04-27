//                                                                                           .*#%@@@@@@&.                 
//           ..                           ,                                        .,(%&@@@@@@@@@@@@@@@@#                 
//           //////,                ./(((((/                                    %@@@@@@@@@@@@@@@@@@@@@@@@*                
//           *//////////,      .*((((((((((,                                  *@@@@@@@@&#/,.  #@@@@@@@@@@&.               
//           /////////////*,*/(((((((((((/                          .#@@@@@@@#/,.          ,&@@@@@*(@@@@@#               
//            ////////*,,,,,,,,,,,*/((((((,                        /@@@@@@@&.              (@@@@@%   %@@@@@*              
//            //*,,,,,,,,,,,,,,,,,,,,,,,//                      .%@@@@@@@(               ,&@@@@@*    ,@@@@@&.             
//          ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,                 (@@@@@@@%.                (@@@@@%.      /@@@@@%             
//       .,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,            ,&@@@@@@@/                 .&@@@@@/         #@@@@&.            
//       ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.         #@@@@@@@@#((((((((((((((((((%@@@@@%.          .&@@(              
//        ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,       ,&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@/             *&.               
//         ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,          ,%@@@@@@@@&%%%%%%%%%%%%%%%%%%&@@@@@#            &@@*              
//          ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,              /@@@@@@@&,                 *@@@@@@,         (@@@@%.            
//           ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.                 .%@@@@@@@#                 %@@@@@#       *@@@@@%             
//            ,,,,,,,,,,,,,,,,,,,,,,,,,,,,.                     *&@@@@@@&,               *@@@@@@,    .&@@@@@,             
//             ,,,,,,,,,,,,,,,,,,,,,,,,,,.                         #@@@@@@@#               %@@@@@#   %@@@@@/              
//              ,,,,,,,,,,.  .,,,,,,,,,,.                            ,&@@@@@@&/,.           *@@@@@@,/@@@@@%               
//               ,.,,,,,        ,,,,,,,.                                      /@@@@@@@%#/,.   %@@@@@@@@@@@.               
//                ....            .,,,.                                        .&@@@@@@@@@@@@@@@@@@@@@@@@*                
//                                                                                .*(%@@@@@@@@@@@@@@@@@@#                 
//                                                                                          ,/#&@@@@@@@&.                 
//                                                                                                   .*,                  
//
//                ███████╗██╗███╗   ███╗██████╗ ██╗     ███████╗██╗    ██╗███████╗██████╗ ██╗  ██╗██████╗ 
//                ██╔════╝██║████╗ ████║██╔══██╗██║     ██╔════╝██║    ██║██╔════╝██╔══██╗╚██╗██╔╝██╔══██╗
//                ███████╗██║██╔████╔██║██████╔╝██║     █████╗  ██║ █╗ ██║█████╗  ██████╔╝ ╚███╔╝ ██████╔╝
//                ╚════██║██║██║╚██╔╝██║██╔═══╝ ██║     ██╔══╝  ██║███╗██║██╔══╝  ██╔══██╗ ██╔██╗ ██╔══██╗
//                ███████║██║██║ ╚═╝ ██║██║     ███████╗███████╗╚███╔███╔╝███████╗██████╔╝██╔╝ ██╗██║  ██║
//                ╚══════╝╚═╝╚═╝     ╚═╝╚═╝     ╚══════╝╚══════╝ ╚══╝╚══╝ ╚══════╝╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝
//
// 
// -----------------------------------------------------------------------------
//
// SimpleWebXR - Unity
//
// https://github.com/Rufus31415/Simple-WebXR-Unity
//
// -----------------------------------------------------------------------------
//
// MIT License
//
// Copyright(c) 2020 Florent GIRAUD (Rufus31415)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----------------------------------------------------------------------------

/**
 * This plugin contains javascript and will be executed before the WebGL application starts.
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
