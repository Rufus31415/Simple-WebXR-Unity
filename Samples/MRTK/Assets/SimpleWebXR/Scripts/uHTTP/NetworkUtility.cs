using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Net;
using System.Net.Sockets;
using System.Linq;

public class NetworkUtility : MonoBehaviour {

	public bool autoUpdateFirewallRule = true;

	public string Host {
		get {
			return Dns.GetHostEntry("").HostName;
		}
	}

	public string Ip {
		get {
			string ip = "unknown";
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach(IPAddress iPAddress in host.AddressList){
				if(iPAddress.AddressFamily == AddressFamily	.InterNetwork){
					ip = iPAddress.ToString();
					break;
				}
			}
			return ip;
		}
	}

	public bool IsHttpSupported {
		get {
			return HttpListener.IsSupported;
		}
	}

	public bool IsLAN {
		get {
			return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
		}
	}

#if UNITY_STANDALONE_WIN
	void Awake() {
		if(Firewall.DoesAppRestrictionExist() && autoUpdateFirewallRule){
			Firewall.UpdateAppRule();
		}
	}
#endif
}
