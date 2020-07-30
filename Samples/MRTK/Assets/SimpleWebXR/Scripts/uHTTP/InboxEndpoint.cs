using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InboxEndpoint : MonoBehaviour
{
    public uHTTP.Server Server { get; private set; }

	[System.Serializable]
    public class PostRequestHandler : UnityEvent<string, string> {}
	[Tooltip("Callback (with url and request body) for any incoming post request")]
    public PostRequestHandler postRequestHandler;

	public int port = 1234;
	public bool StartStopAutomatically = true;

	void OnEnable(){
		if(Server == null){
			Server = new uHTTP.Server(port);
		}
		Server.requestHandler = (uHTTP.Request request) => {
            if(request.Method.ToUpper().Equals("POST")) {
                Dispatcher.Invoke(() => {
                    if(postRequestHandler != null) {
                        postRequestHandler.Invoke(request.Url, request.Body);
                    }
                });
                uHTTP.Response response = new uHTTP.Response(uHTTP.StatusCode.OK);
				response.Headers.Add("Access-Control-Allow-Origin", "*");
				return response;
            }
            return new uHTTP.Response(uHTTP.StatusCode.ERROR);
	    };
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
}
