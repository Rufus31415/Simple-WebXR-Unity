using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class Dispatcher : MonoBehaviour {
	private static object monitor = new object();
	private static bool isApplicationQuitting = false;
	private static int dispatcherPresent = 0;
	public static int DispatcherPresent {
		get{
			lock(monitor){
				return dispatcherPresent;
			}
		}
	}
	private static Queue<Action> taskQueue = new Queue<Action>();

	void Awake(){
		lock(monitor){
			dispatcherPresent++;
		}
	}

	void OnDestroy(){
		lock(monitor){
			dispatcherPresent--;
		}
	}
	
	void OnApplicationQuit(){
		lock(monitor){
			isApplicationQuitting = true;
			while(taskQueue.Count > 0){
				taskQueue.Dequeue().Invoke();
			}
		}
	}

	void Update(){
		Action task;
		lock(monitor){
			if(taskQueue.Count == 0){
				return;
			}
			task = taskQueue.Dequeue();
		}
		task.Invoke();
	}

	public static void Invoke(Action task){
		lock(monitor){
			if(isApplicationQuitting){
				throw new Exception("Dispatcher destroyed!");
			}
			if(dispatcherPresent == 0){
				Debug.LogWarning("Create at least one Dispatcher!");
			}
			taskQueue.Enqueue(task);
		}
	}
}
