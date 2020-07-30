using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;
using System.Threading;
using System.IO;

public class StaticWebServer : MonoBehaviour {
	public uHTTP.Server Server { get; private set; }

	[Tooltip("Relative to the StreamingAssets")] public string rootFolder;
	[Tooltip("Relative to the Root Folder")] public string page404;
	public int port = 8080;
	public bool StartStopAutomatically = true;

	private uHTTP.Response CreateResponse(uHTTP.StatusCode statusCode, string file){
		uHTTP.Response response = new uHTTP.Response(statusCode);
		response.Headers.Add("Connection", "Closed");
		if(!string.IsNullOrEmpty(file)){
			response.Body = File.ReadAllBytes(file);
		}
		return response;
	}

	void OnEnable(){
		if(Server == null){
			Server = new uHTTP.Server(port);
		}
		Server.requestHandler = OnRequest;
		if(StartStopAutomatically && !Server.IsRunning){
			Server.Start();
		}
	}

	void OnDisable(){
		if(StartStopAutomatically && Server.IsRunning){
			Server.Stop();
		}
	}

	void OnDestroy(){
		if(Server != null && Server.IsRunning){
			Server.Stop();
		}
	}

	void Awake(){
		streamingAssetsPath = Application.streamingAssetsPath;
	}

	private string streamingAssetsPath;

	private string root {
		get {
			string root = streamingAssetsPath;
			root += (string.IsNullOrEmpty(rootFolder) ? string.Empty : "/") + rootFolder;
			if(root.EndsWith("/")){
				root = root.Substring(0, root.Length -1);
			}
			return root;
		}
	}

	public string GetIndexFile(string folder){
		foreach(FileInfo file in new DirectoryInfo(folder).GetFiles()){
			if(file.Name.ToLower().Split('.')[0].Equals("index")){
				return file.Name;
			}
		}
		return null;
	}

	public uHTTP.Response OnRequest(uHTTP.Request request){
		if(!request.Method.ToUpper().Equals("GET")){
			return new uHTTP.Response(uHTTP.StatusCode.ERROR);
		}
		string file = root + request.Url.Split('?')[0];
		if(file.EndsWith("/")){
			file += GetIndexFile(file);
		}
		if(file == null || !File.Exists(file)){
			return CreateResponse(uHTTP.StatusCode.NOT_FOUND, root + '/' + page404);
		}
		return CreateResponse(uHTTP.StatusCode.OK, file);
	}
}
