using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class SimpleSpectatorViewServer : MonoBehaviour
{
    //public Camera Camera;

    public int port = 8090;
    public bool StartStopAutomatically = true;

    public uHTTP.Server Server { get; private set; }


    void OnEnable()
    {
        if (Server == null)
        {
            Server = new uHTTP.Server(port);
        }
        Server.requestHandler = OnRequest;
        if (StartStopAutomatically && !Server.IsRunning)
        {
            Server.Start();
        }
    }

    void OnDisable()
    {
        if (StartStopAutomatically && Server.IsRunning)
        {
            Server.Stop();
        }
    }

    void OnDestroy()
    {
        if (Server != null && Server.IsRunning)
        {
            Server.Stop();
        }
    }

    private readonly ManualResetEvent _waitRender = new ManualResetEvent(true);

    private RequestImageJSon _requestData;

    public uHTTP.Response OnRequest(uHTTP.Request request)
    {
        _waitRender.Reset();

        if (!string.IsNullOrEmpty(request.Body))
        {
            try
            {
                _requestData = JsonUtility.FromJson<RequestImageJSon>(UnityEngine.Networking.UnityWebRequest.UnEscapeURL(request.Body));
            }
            catch
            {
                _requestData = null;
            }
        }
        else _requestData = null;


        _requestImage = true;
        var answer = new uHTTP.Response(uHTTP.StatusCode.OK);
        answer.Headers.Add("Content-Type", "image/png");
        answer.Headers.Add("Access-Control-Allow-Origin", "*");
        
        _waitRender.WaitOne(500);

        answer.Body = _png;
        return answer;
    }

    private void Start()
    {
        StartCoroutine(RenderInTexture());
    }

   private readonly WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();


    private bool _requestImage = false;

    private byte[]  _png;

    private Vector3 _offsetRotation;
    private Vector3 _offsetPosition;

    public IEnumerator RenderInTexture()
    {
        Camera _camera = gameObject.EnsureComponent<Camera>();
        _camera.backgroundColor = new Color(0, 0, 0, 0);
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.depth = Camera.main.depth - 1;
        _camera.transform.position = Camera.main.transform.position;
        _camera.transform.rotation = Camera.main.transform.rotation;
        _camera.nearClipPlane = 0.1f;
        _camera.farClipPlane = 1000;
        _camera.forceIntoRenderTexture = true;
        _camera.enabled = false;

        while (true)
        {
            yield return new WaitUntil(() => _requestImage);
            _requestImage = false;

            yield return frameEnd;

            var data = _requestData;

            if (data != null)
            {
                if (!_camera.targetTexture || _camera.targetTexture.width != data.w || _camera.targetTexture.height != data.h)
                {
                    if (_camera.targetTexture) Destroy(_camera.targetTexture);

                    var descr = new RenderTextureDescriptor();
                    descr.autoGenerateMips = true;
                    descr.bindMS = false;
                    descr.colorFormat = RenderTextureFormat.ARGB32;
                    descr.depthBufferBits = 0;
                    descr.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
                    descr.enableRandomWrite = false;
                    descr.memoryless = RenderTextureMemoryless.None;
                    descr.msaaSamples = 1;
                    descr.sRGB = false;
                    descr.shadowSamplingMode = UnityEngine.Rendering.ShadowSamplingMode.None;
                    descr.useMipMap = false;
                    descr.volumeDepth = 1;
                    descr.vrUsage = UnityEngine.VRTextureUsage.None;

                    descr.height = data.h;
                    descr.width = data.w;


                    _camera.targetTexture = new RenderTexture(descr);
                }

                _camera.transform.position = data.f ? Camera.main.transform.position : data.p + _offsetPosition;
                _camera.transform.eulerAngles = data.f ? Camera.main.transform.eulerAngles : data.r+ _offsetRotation;
                _camera.projectionMatrix = data.m;
            }

            var formerActiveTexture = RenderTexture.active;

            var tex = _camera.targetTexture;
            RenderTexture.active = tex;

            _camera.Render();

            //110fps
            Texture2D tempTex = new Texture2D(tex.width, tex.height);
            tempTex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            //50fps
            tempTex.Apply();

            //22fps
            _png = tempTex.EncodeToPNG();

            _waitRender.Set();

            RenderTexture.active = formerActiveTexture;

            if (data != null && data.t)
            {
                var pointer = CoreServices.InputSystem.FocusProvider.GetPointers<PokePointer>().FirstOrDefault();

                if(pointer != null)
                {
                    _offsetPosition = pointer.Position - data.p;
                    _offsetRotation = new Vector3(0, Camera.main.transform.eulerAngles.y - data.r.y, 0);
                }
            }

        }


    }

}


public class RequestImageJSon
{
    public int w, h;
    public Vector3 p, r;
    public Matrix4x4 m;
    public bool f;
    public bool t;
}
