using System.ComponentModel;
using System.Net.Sockets;
using System.Text;

public static partial class uHTTP
{
    private const string EOL = "\r\n";

    public class StatusCode {
        public static readonly StatusCode SWITCHING_PROTOCOLS = new StatusCode(101, "Switching Protocols");
        public static readonly StatusCode OK = new StatusCode(200, "OK");
        public static readonly StatusCode NOT_FOUND = new StatusCode(404, "Not Found");
        public static readonly StatusCode ERROR = new StatusCode(500, "Internal Server Error");

        public readonly int statusCode;
        public readonly string description;

        private StatusCode(int statusCode, string description) {
            this.statusCode = statusCode;
            this.description = description;
        }
    }
}