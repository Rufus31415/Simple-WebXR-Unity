using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;
using System.Threading;
using System.IO;

public class LocalRestAPI : MonoBehaviour {
	public int port = 8090;
	public bool StartStopAutomatically = true;

	public uHTTP.Server Server { get; private set; }
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

	public uHTTP.Response OnRequest(uHTTP.Request request){

		var answer = new uHTTP.Response(uHTTP.StatusCode.OK);
		answer.Body = new byte[1];
		answer.Headers.Add("Content-Type", "image/png");
		return answer;
	}
}
