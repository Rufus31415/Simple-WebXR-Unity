using System.Collections.Generic;
using System.Text;
using System;

public static partial class uHTTP {

    public class Response
    {
        public static Response Default {
            get {
                Response response = new Response(StatusCode.OK);
                response.Headers.Add("Connection", "Closed");
                response.Headers.Add("Content-Type", "text/html");
                response.Body = Encoding.UTF8.GetBytes("Hello, &#181;HTTP!");
                return response;
            }
        }

        public StatusCode StatusCode
        {
            get; private set;
        }
        public Dictionary<string, string> Headers {
            get; private set;
        }
        public byte[] Body { get; set; }

        public Response(StatusCode statusCode)
        {
            this.StatusCode = statusCode;
            Headers = new Dictionary<string, string>();
            Body = new byte[]{};
        }

        public byte[] ToBinary(){
            StringBuilder head = new StringBuilder(
                string.Format("HTTP/1.1 {0} {1}", StatusCode.statusCode, StatusCode.description)
            );
            foreach(var header in Headers){
                head.Append(EOL);
                head.Append(
                    string.Format("{0}: {1}", header.Key, header.Value)
                );
            }
            head.Append(EOL);
            head.Append(EOL);

            byte[] headBytes = Encoding.UTF8.GetBytes(head.ToString());
            byte[] bytes = new byte[headBytes.Length + Body.Length];
            Array.Copy(headBytes, bytes, headBytes.Length);
            Array.Copy(Body, 0, bytes, headBytes.Length, Body.Length);
            return bytes;
        }
    }
}