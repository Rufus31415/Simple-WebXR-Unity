using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;

public static partial class uHTTP 
{
    public class Server
    {
        private TcpListener listener;
        public RequestHandler requestHandler;

        public delegate Response RequestHandler(Request request);

        private object monitor = new object();
        private bool isServerRunning;
        public bool IsRunning {
            get{
                lock(monitor){
                    return isServerRunning;
                }
            }
            private set{
                lock(monitor){
                    isServerRunning = value;
                }
            }
        }
        
        public Server(int port = 80){
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start(){
            lock(monitor){
                if(IsRunning){
                    throw new Exception("Server already running!");
                }
                IsRunning = true;
            }

            listener.Start();
            new Thread(() => {
                while(true){
                    try {
                        TcpClient client = listener.AcceptTcpClient();                    
                        new Thread(() => {
                            HandleConnection(client);
                            client.Close();
                        }).Start();
                    }
                    catch(SocketException){
                        break;
                    }
                }
            }).Start();
        }

        public void Stop(){
            lock(monitor){
                if(!IsRunning){
                    throw new Exception("Server not running!");
                }
                listener.Stop();
                IsRunning = false;
            }
        }

        private void HandleConnection(TcpClient client){
            NetworkStream stream = client.GetStream();

            while(!stream.DataAvailable){
                Thread.Sleep(20);
            }

            byte[] received = new byte[client.Available];
            stream.Read(received, 0, received.Length);

            Request request = Request.TryParse(Encoding.UTF8.GetString(received));
            if(request == null){
                return; // Not HTTP
            }
            else if(requestHandler != null){
                Response response = requestHandler(request);
                if (response.Body == null) response.Body = new byte[0];
                byte[] bytes = response.ToBinary();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }
    }
}