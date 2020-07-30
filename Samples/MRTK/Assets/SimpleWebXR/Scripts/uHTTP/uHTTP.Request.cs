using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Sockets;

public static partial class uHTTP
{
    public class Request
    {
        public string Method { get; private set; }
        public string Url { get; private set; }
        public Dictionary<string, string> Headers {
            get; private set;
        }
        public string Body { get; private set; }

        private Request(){
            Headers = new Dictionary<string, string>();
            Body = string.Empty;
        }

        public static Request TryParse(string str){
            string[] lines = str.Split(new string[]{ EOL }, StringSplitOptions.None);

            if(!Regex.IsMatch(lines[0], "[GET|HEAD|POST|PUT|DELETE] /.* HTTP/1.1")){
                return null;
            }

            Request request = new Request();

            request.Method = lines[0].Split(' ')[0];
            request.Url = lines[0].Split(' ')[1];

            bool isData = false;
            for(int i = 1; i < lines.Length; i++){
                if(lines[i].Equals(string.Empty)){
                    isData = true;
                    continue;
                }

                if(isData){
                    request.Body += (request.Body.Equals(string.Empty) ? string.Empty : EOL) + lines[i];
                }
                else{
                    string[] keyValuePair = lines[i].Split(new string[]{ ": " }, StringSplitOptions.None);                    
                    request.Headers.Add(keyValuePair[0], keyValuePair[1]);
                }
            }

            return request;
        }
    }
}