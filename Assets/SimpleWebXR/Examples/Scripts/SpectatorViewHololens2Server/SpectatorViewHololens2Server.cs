using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace Rufus31415.WebXR.Demo
{
    public class SpectatorViewHololens2Server : MonoBehaviour
    {
        private static readonly Regex _headerSecWebSocketKey = new Regex("Sec-WebSocket-Key: (.*)", RegexOptions.Compiled);

        public int port = 8090;
        public bool StartStopAutomatically = true;

        Socket _ws;
        private readonly object _wsLock = new object();
        private bool _wsReady;

        private bool ConnectedAndReady
        {
            get
            {
                lock (_wsLock) return _ws != null && _ws.Connected && _wsReady;
            }
        }

        private RequestImageJSon _requestData;

        private void OnDestroy()
        {
            lock (_wsLock) if (_ws != null && _ws.Connected) _ws.Disconnect(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Socket listeningSocket = null;
                    try
                    {
                        listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        listeningSocket.SendBufferSize = 65536;
                        listeningSocket.Bind(new IPEndPoint(IPAddress.Any, port: 8090));
                        listeningSocket.Listen(0);

                        _ws = listeningSocket.Accept();
                        Debug.Log("A client connected.");

                        var receivedData = new byte[100000];
                        var receivedDataLength = _ws.Receive(receivedData);

                        var requestString = Encoding.UTF8.GetString(receivedData, 0, receivedDataLength);

                        if (requestString.StartsWith("GET"))
                        {
                            var receivedWebSocketKey = _headerSecWebSocketKey.Match(requestString).Groups[1].Value.Trim();
                            var keyHash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes($"{receivedWebSocketKey}258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));

                            var header = new StringBuilder();
                            header.Append("HTTP/1.1 101 Switching Protocols\r\n");
                            header.Append("Connection: Upgrade\r\n");
                            header.Append("Upgrade: websocket\r\n");
                            header.Append("Sec-WebSocket-Accept: ");
                            header.Append(Convert.ToBase64String(keyHash));
                            header.Append("\r\n\r\n");

                            var responseBytes = Encoding.UTF8.GetBytes(header.ToString());

                            lock (_wsLock) _ws.Send(responseBytes);
                        }

                        while (true)
                        {
                            _wsReady = true;

                            _ws.Receive(receivedData);

                            if ((receivedData[0] & (byte)Opcode.CloseConnection) == (byte)Opcode.CloseConnection)
                            {
                            // Close connection request.
                            Debug.Log("Client disconnected.");
                                CloseWebSocket(listeningSocket);
                                break;
                            }
                            else
                            {
                                var receivedPayload = ParsePayloadFromFrame(receivedData);
                                var receivedString = Encoding.UTF8.GetString(receivedPayload);

                                try
                                {
                                    _requestData = JsonUtility.FromJson<RequestImageJSon>(receivedString);
                                }
                                catch
                                {
                                    _requestData = null;
                                }
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        if (!(e is SocketException) || (e as SocketException).ErrorCode != 10048)
                        {
                            Debug.LogException(e);
                        }
                    }
                    finally
                    {
                        _wsReady = false;
                        CloseWebSocket(listeningSocket);
                    }

                // Wait between 2 connexions
                Thread.Sleep(1000);
                }
            });

            StartCoroutine(RenderInTexture());
        }

        private void CloseWebSocket(Socket listeningSocket)
        {
            _wsReady = false;
            lock (_wsLock)
            {
                if (listeningSocket != null && _ws != null)
                {
                    _ws = null;
                    //try { listeningSocket.Shutdown(SocketShutdown.Both); }
                    // catch { }
                    try { listeningSocket.Close(); }
                    catch { }
                    try { listeningSocket.Dispose(); }
                    catch { }
                }
            }
        }

        private void LateUpdate()
        {
            var data = _requestData; // cache _requestData

            if (data != null && _camera)
            {
                if (data.f)
                {
                    _offsetPosition = Camera.main.transform.position - data.p;
                    _offsetRotation = Camera.main.transform.eulerAngles - data.r;
                }

                _camera.transform.position = data.p + _offsetPosition;
                _camera.transform.eulerAngles = data.r + _offsetRotation;
                _camera.projectionMatrix = data.m;
            }

            if (data != null && data.t && !data.f)
            {
                var pointer = CoreServices.InputSystem.FocusProvider.GetPointers<PokePointer>().FirstOrDefault();

                if (pointer != null)
                {
                    _offsetPosition = pointer.Position - data.p;
                    _offsetRotation = new Vector3(0, Camera.main.transform.eulerAngles.y - data.r.y, 0);
                }
            }

        }


        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        private readonly WaitForSeconds _waitForSeconds = new WaitForSeconds(0.1f);


        private byte[] _png;

        private Vector3 _offsetRotation;
        private Vector3 _offsetPosition;

        private Texture2D _streamingTexture;

        private void SetCameraTexture(Camera c, int w, int h)
        {
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

            descr.height = h;
            descr.width = w;

            c.targetTexture = new RenderTexture(descr);
        }

        Camera _camera;

        public IEnumerator RenderInTexture()
        {
            _camera = gameObject.EnsureComponent<Camera>();
            _camera.backgroundColor = new Color(0, 0, 0, 0);
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.depth = Camera.main.depth - 1;
            _camera.transform.position = Camera.main.transform.position;
            _camera.transform.rotation = Camera.main.transform.rotation;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 1000;
            _camera.forceIntoRenderTexture = true;
            _camera.enabled = false;

            _streamingTexture = new Texture2D(100, 100);

            SetCameraTexture(_camera, _streamingTexture.width, _streamingTexture.height);


            while (true)
            {
                yield return _waitForSeconds;

                yield return new WaitUntil(() => ConnectedAndReady);

                yield return _waitForEndOfFrame;

                try
                {

                    var data = _requestData; // cache _requestData

                    if (data != null && _camera)
                    {
                        if (!_camera.targetTexture || !_streamingTexture || _camera.targetTexture.width != data.w || _camera.targetTexture.height != data.h)
                        {
                            if (_camera.targetTexture) Destroy(_camera.targetTexture);
                            if (_streamingTexture) Destroy(_streamingTexture);

                            SetCameraTexture(_camera, data.w, data.h);
                            _streamingTexture = new Texture2D(data.w, data.h);
                        }
                    }

                    var formerActiveTexture = RenderTexture.active;

                    var tex = _camera.targetTexture;
                    RenderTexture.active = tex;

                    _camera.Render();

                    _streamingTexture.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
                    _streamingTexture.Apply();

                    _png = _streamingTexture.EncodeToPNG();


                    lock (_wsLock)
                    {
                        if (_ws != null)
                        {
                            const int maxPacketSize = 60000;

                            var header = new byte[] { 0x12, 0x06, 0x92, (byte)(_png.Length & 0xFF), (byte)((_png.Length >> 8) & 0xFF), (byte)((_png.Length >> 16) & 0xFF), (byte)((_png.Length >> 24) & 0xFF) };
                            _ws.Send(CreateFrameFromByte(header, 0, header.Length));

                            int nbFrame = (int)Math.Ceiling((float)_png.Length / maxPacketSize);

                            for (int i = 0; i < nbFrame; i++)
                            {
                                var encoded = CreateFrameFromByte(_png, i * maxPacketSize, i == nbFrame - 1 ? _png.Length - i * maxPacketSize : maxPacketSize);
                                _ws.Send(encoded);
                            }
                        }
                    }

                    RenderTexture.active = formerActiveTexture;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

            }
        }

        public static byte[] ParsePayloadFromFrame(byte[] incomingFrameBytes)
        {
            var payloadLength = 0L;
            var totalLength = 0L;
            var keyStartIndex = 0L;

            // 125 or less.
            // When it's below 126, second byte is the payload length.
            if ((incomingFrameBytes[1] & 0x7F) < 126)
            {
                payloadLength = incomingFrameBytes[1] & 0x7F;
                keyStartIndex = 2;
                totalLength = payloadLength + 6;
            }

            // 126-65535.
            // When it's 126, the payload length is in the following two bytes
            if ((incomingFrameBytes[1] & 0x7F) == 126)
            {
                payloadLength = BitConverter.ToInt16(new[] { incomingFrameBytes[3], incomingFrameBytes[2] }, 0);
                keyStartIndex = 4;
                totalLength = payloadLength + 8;
            }

            // 65536 +
            // When it's 127, the payload length is in the following 8 bytes.
            if ((incomingFrameBytes[1] & 0x7F) == 127)
            {
                payloadLength = BitConverter.ToInt64(new[] { incomingFrameBytes[9], incomingFrameBytes[8], incomingFrameBytes[7], incomingFrameBytes[6], incomingFrameBytes[5], incomingFrameBytes[4], incomingFrameBytes[3], incomingFrameBytes[2] }, 0);
                keyStartIndex = 10;
                totalLength = payloadLength + 14;
            }

            if (totalLength > incomingFrameBytes.Length)
            {
                throw new Exception("The buffer length is smaller than the data length.");
            }

            var payloadStartIndex = keyStartIndex + 4;

            byte[] key = { incomingFrameBytes[keyStartIndex], incomingFrameBytes[keyStartIndex + 1], incomingFrameBytes[keyStartIndex + 2], incomingFrameBytes[keyStartIndex + 3] };

            var payload = new byte[payloadLength];
            Array.Copy(incomingFrameBytes, payloadStartIndex, payload, 0, payloadLength);
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)(payload[i] ^ key[i % 4]);
            }

            return payload;
        }

        public enum Opcode
        {
            Fragment = 0,
            Text = 1,
            Binary = 2,
            CloseConnection = 8,
            Ping = 9,
            Pong = 10
        }

        public static byte[] CreateFrameFromString(string message)
        {
            var payload = Encoding.UTF8.GetBytes(message);
            return CreateFrameFromByte(payload, 0, payload.Length);

        }

        public static byte[] CreateFrameFromByte(byte[] payload, int start, int length)
        {

            byte[] frame;

            if (length < 126)
            {
                frame = new byte[1 /*op code*/ + 1 /*payload length*/ + length /*payload bytes*/];
                frame[1] = (byte)length;
                Array.Copy(payload, start, frame, 2, length);
            }
            else if (length >= 126 && length <= 65535)
            {
                frame = new byte[1 /*op code*/ + 1 /*payload length option*/ + 2 /*payload length*/ + length /*payload bytes*/];
                frame[1] = 126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                Array.Copy(payload, start, frame, 4, length);
            }
            else
            {
                frame = new byte[1 /*op code*/ + 1 /*payload length option*/ + 8 /*payload length*/ + length /*payload bytes*/];
                frame[1] = 127; // <-- Indicates that payload length is in following 8 bytes.
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);
                Array.Copy(payload, start, frame, 10, length);
            }

            frame[0] = (byte)((byte)Opcode.Binary | 0x80 /*FIN bit*/);

            return frame;
        }

    }
}