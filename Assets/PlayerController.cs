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
	//Declaraciones-START
	private readonly string XML_FILE = @".//OpenNI.xml";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private UserGenerator userGenerator;
	private SkeletonCapability skeletonCapability;
	private PoseDetectionCapability poseDetectionCapability;
	private string calibPose;
	private Dictionary <int, Dictionary<SkeletonJoint,SkeletonJointPosition>> joints;
	private bool shouldRun;
		
	public Vector3 bias;
	public float scale;
	
	//Declaraciones-END
	//Para inicar
	void Start () {
		Debug.Log("Comenzando el programa");
		this.context=Context.CreateFromXmlFile(XML_FILE,out scriptNode);
		this.depth=context.FindExistingNode(NodeType.Depth) as DepthGenerator;
		if(depth==null){
			throw new Exception("El visualizador debe tener un Nodo de Profuncidad");
		}
		this.userGenerator=new UserGenerator(this.context);
		this.skeletonCapability=this.userGenerator.SkeletonCapability;
		this.poseDetectionCapability=this.userGenerator.PoseDetectionCapability;
		this.calibPose=this.skeletonCapability.CalibrationPose;
		this.userGenerator.NewUser+=userGenerator_NewUser;
		this.userGenerator.LostUser+=userGenerator_LostUser;
		this.poseDetectionCapability.PoseDetected+=poseDetectionCapability_PoseDetected;
		this.skeletonCapability.CalibrationComplete+=skeletonCapability_CalibrationComplete;
		this.skeletonCapability.SetSkeletonProfile(SkeletonProfile.All);
		this.joints=new Dictionary<int, Dictionary<SkeletonJoint, SkeletonJointPosition>>();
		this.userGenerator.StartGenerating();
				
		MapOutputMode mapMode=this.depth.MapOutputMode;
		Debug.Log("Finalizando la inicializacion");
		this.shouldRun=true;
	}	       
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("Corriendo el update");
		//Codigo heco para varios usuarios
		if(this.shouldRun){
			try{
				this.context.WaitOneUpdateAll(this.depth);
			}catch(Exception){
				Debug.Log("No paso");
			}
			
			int[] users=this.userGenerator.GetUsers();
			foreach(int user in users){
				if(this.skeletonCapability.IsTracking(user)){
					Debug.Log ("Esta trackeando");
					//TODO
					//Obtener el angulo
					
					SkeletonJointOrientation ori=new SkeletonJointOrientation();
					ori=this.skeletonCapability.GetSkeletonJointOrientation(user,SkeletonJoint.Torso);
					transform.rotation=SkeletonJointOrientationToQuaternion(ori);
					//Quaternion quaternion=SkeletonJointOrientationToQuaternion(ori);
					//transform.Rotate(quaternion.eulerAngles*Time.deltaTime);
					
					
					//SkeletonJointPosition pos=new SkeletonJointPosition();
					//pos=this.skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.Torso);
					//Vector3 v3=new Vector3(pos.Position.X/0.1f,pos.Position.Y/0.1f,pos.Position.Z/0.1f);
					//transform.position=(v3/100f)+bias;
					//transform.Translate(realWorldToUnityDimension(pos.Position));								
					
				}
			}
			
		}
		
		
		
		
		float MoveForward = Input.GetAxis("Vertical")  * MoveSpeed * Time.deltaTime;
    	float MoveRotate = Input.GetAxis("Horizontal") * RotateSpeed * Time.deltaTime;
		
		// Move the player
		//Avanzar
    	transform.Translate(Vector3.forward * MoveForward);
    	//Rotar
		//transform.Rotate(Vector3.up * MoveRotate);
		
	}
	
	void userGenerator_NewUser(object sender,NewUserEventArgs e){
		if(this.skeletonCapability.DoesNeedPoseForCalibration){
			this.poseDetectionCapability.StartPoseDetection(this.calibPose,e.ID);
		}else{
			this.skeletonCapability.RequestCalibration(e.ID,true);
		}		
	}
	
	void userGenerator_LostUser(object sender,UserLostEventArgs e){
		this.joints.Remove(e.ID);
	}
	
	void poseDetectionCapability_PoseDetected(object sender,PoseDetectedEventArgs e){
		this.poseDetectionCapability.StopPoseDetection(e.ID);
		this.skeletonCapability.RequestCalibration(e.ID,true);
	}
	
	void skeletonCapability_CalibrationComplete(object sender,CalibrationProgressEventArgs e){
		if(e.Status==CalibrationStatus.OK){
			this.skeletonCapability.StartTracking(e.ID);
			this.joints.Add(e.ID,new Dictionary<SkeletonJoint,SkeletonJointPosition>());
		}
		else if(e.Status!=CalibrationStatus.ManualAbort)
		{
			if(this.skeletonCapability.DoesNeedPoseForCalibration){
				this.poseDetectionCapability.StartPoseDetection(calibPose,e.ID);
			}else{
				this.skeletonCapability.RequestCalibration(e.ID,true);	
			}
		}
	}
	
	void OnApplicationQuit (){
		Debug.Log("Saliendo de la Aplicacion");
		context.Release();
	}
	

	public static Quaternion SkeletonJointOrientationToQuaternion(SkeletonJointOrientation m) {

        float tr = m.X1 + m.Y2 + m.Z3;
        float S = 0f;
        float qw = 0f;
        float qx = 0f;
        float qy = 0f;
        float qz = 0f;

 

        if(tr > 0) {
			Debug.Log("1");
            S = Mathf.Sqrt(tr + 1.0f) * 2f;
            qw = 0.25f * S;
            qx = (m.Y3 - m.Z2) / S;
            qy = (m.Z1 - m.X3) / S;
            qz = (m.X2 - m.Y1) / S;
        } else if((m.X1 > m.Y2) && (m.X1 > m.Z3)) {
			Debug.Log("2");
            S = Mathf.Sqrt(1.0f + m.X1 - m.Y2 - m.Z3) * 2f;
            qw = (m.Y3 - m.Z2) / S;
            qx = 0.25f * S;
            qy = (m.Y1 + m.X2) / S;
            qz = (m.Z1 + m.X3) / S;
        } else if(m.Y2 > m.Z3) {
			Debug.Log("3");
            S = Mathf.Sqrt(1.0f + m.Y2 - m.X1 - m.Z3) * 2f;
            qw = (m.Z1 - m.X3) / S;
            qx = (m.Y1 + m.X2) / S;
            qy = 0.25f * S;
            qz = (m.Z2 + m.Y3) / S;
        } else {
			Debug.Log("4");
            S = Mathf.Sqrt(1.0f + m.Z3 - m.X1 - m.Y2) * 2f;
            qw = (m.X2 - m.Y1) / S;
            qx = (m.Z1 + m.X3) / S;
            qy = (m.Z2 + m.Y3) / S;
            qz = 0.25f * S;
        }
		Debug.Log("Transformado en: "+qx+","+qy+","+qz+","+qw);
		
		return new Quaternion(qx, qy, qz, qw);
		//return new Quaternion(qx, qy, 0f, qw);
	}
	
	public static Vector3 Point3DToVector3(Point3D point) {
        return new Vector3(point.X, point.Y, point.Z);
    }
	
	public static Vector3 realWorldToUnityDimension(Point3D point) {
        return new Vector3(point.X / 0.1f, point.Y / 0.1f, point.Z / 0.1f);
    }
	
}
