using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
/*
 * Usare los .dll oficiales de Primesense por que los de thinkerer los modifico
 */ 
//OpenNI
using OpenNI;
//NITE
using NITE;

public class PlayerController : MonoBehaviour {
	public float MoveSpeed = 10;
	public float RotateSpeed = 40;
	
	
	//Declaraciones-END
	//Para inicar
	void Start () {
		Debug.Log("Start");
	}	       
	
	
	void Update () {
		Debug.Log("Update");
		float MoveForward = Input.GetAxis("Vertical")  * MoveSpeed * Time.deltaTime;
    	float MoveRotate = Input.GetAxis("Horizontal") * RotateSpeed * Time.deltaTime;
		
		// Move the player
		//Avanzar
    	transform.Translate(Vector3.forward * MoveForward);
    	//Rotar
		transform.Rotate(Vector3.up * MoveRotate);
		Debug.Log(transform.rotation.x+","+transform.rotation.y+","+transform.rotation.z+","+transform.rotation.w);
		
	}
}
