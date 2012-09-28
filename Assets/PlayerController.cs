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
/*
 * Iniciar OpenNI
 */
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private UserGenerator userGenerator;
	private SkeletonCapability skeletonCapability;
	private PoseDetectionCapability poseDetectionCapability;
	private string calibPose;
	Quaternion rotationInitial;
		
	private Dictionary <int, Dictionary<SkeletonJoint,SkeletonJointPosition>> joints;
	private bool shouldRun;
	
	
	//aaron
	public float valorPositivoRotacion=1;
	public float valorNegativoRotacion=1;
	public float zInicial=1320;
	public float xInicial=100;
	public float yInicial=-30;
	//Para inicar
	
	//----
	public float xAnterior=0;
	public float yAnterior=0;
	public float zAnterior=0;
	//----
	
	void Start () {
		Debug.Log("Start");
		this.context=Context.CreateFromXmlFile(XML_CONFIG,out scriptNode);
		//Al Ejecutar esta linea al salir de probar el nodo sigue trabajando para eso ocupamos el onApplicationQuit
		this.depth=context.FindExistingNode(NodeType.Depth) as DepthGenerator;
		if(depth==null){
			throw new Exception("Nodo de Profundidad no encontrado");
		}
		
		this.userGenerator=new UserGenerator(this.context);
		this.skeletonCapability=this.userGenerator.SkeletonCapability;
		this.poseDetectionCapability=this.userGenerator.PoseDetectionCapability;
		this.calibPose=this.skeletonCapability.CalibrationPose;
		//Agregas los handlers
		this.userGenerator.NewUser+=userGenerator_NewUser;
		this.userGenerator.LostUser+=userGenerator_LostUser;
		this.poseDetectionCapability.PoseDetected+=poseDetectionCapability_PoseDetected;
		this.skeletonCapability.CalibrationComplete+=skeletonCapability_CalibrationComplete;
		//Activar los joints depende del profile
		//http://openni.org/docs2/Reference/_xn_types_8h_a294999eabe6eeab319a61d3d0093b174.html#a294999eabe6eeab319a61d3d0093b174
		this.skeletonCapability.SetSkeletonProfile(SkeletonProfile.All);
		this.joints=new Dictionary<int,Dictionary<SkeletonJoint,SkeletonJointPosition>>();
		
		
		//Generar
		this.userGenerator.StartGenerating();
		this.shouldRun=true;
	}
	
	
	void userGenerator_NewUser(object sender, NewUserEventArgs e){
          if (this.skeletonCapability.DoesNeedPoseForCalibration){
            	this.poseDetectionCapability.StartPoseDetection(this.calibPose, e.ID);
           }else{
            	this.skeletonCapability.RequestCalibration(e.ID, true);
            }
    }
	
	void poseDetectionCapability_PoseDetected(object sender, PoseDetectedEventArgs e){
            this.poseDetectionCapability.StopPoseDetection(e.ID);
            this.skeletonCapability.RequestCalibration(e.ID, true);
    }
	
	void skeletonCapability_CalibrationComplete(object sender, CalibrationProgressEventArgs e){
            if (e.Status == CalibrationStatus.OK){
                this.skeletonCapability.StartTracking(e.ID);
                this.joints.Add(e.ID, new Dictionary<SkeletonJoint, SkeletonJointPosition>());
            }else if (e.Status != CalibrationStatus.ManualAbort){
                if (this.skeletonCapability.DoesNeedPoseForCalibration){
                    this.poseDetectionCapability.StartPoseDetection(calibPose, e.ID);
                }else{
                    this.skeletonCapability.RequestCalibration(e.ID, true);
                }
            }
    }
	
	void userGenerator_LostUser(object sender, UserLostEventArgs e){
		this.joints.Remove(e.ID);
	}
	
	void Update () {
		//Debug.Log("Update");
		if(this.shouldRun){
			try{
				this.context.WaitOneUpdateAll(this.depth);
			}catch(Exception){
				Debug.Log("No paso");
			}
			//Debug.Log("H "+Input.GetAxis("Horizontal"));
			//Debug.Log("V "+Input.GetAxis("Vertical"));
			
			int[] users=this.userGenerator.GetUsers();
			foreach(int user in users){
				if(this.skeletonCapability.IsTracking(user)){
					
					//Debug.Log ("Esta trackeando Usuario # "+user);
					//obtener la orientacion del joint
					SkeletonJointOrientation ori=this.skeletonCapability.GetSkeletonJointOrientation(user,SkeletonJoint.Torso);
					
					Quaternion q=SkeletonJointOrientationToQuaternion(ori);
					//Debug.Log(q.y);
					if(q.y>.30){
						transform.Rotate(new Vector3(0f,valorPositivoRotacion,0f));
					}else if(q.y<-.30){
						transform.Rotate(new Vector3(0f,-valorNegativoRotacion,0f));
					}
					
					if(q.x>.20){
							if(GameObject.Find("CuboCamara").transform.rotation.x<.25){
								GameObject.Find("CuboCamara").transform.Rotate(new Vector3(valorPositivoRotacion,0f,0f));
							}
					}else if(q.x<-.20){
							if(GameObject.Find("CuboCamara").transform.rotation.x>-.25){
								GameObject.Find("CuboCamara").transform.Rotate(new Vector3(-valorNegativoRotacion,0f,0f));
							}
					}else{
						Quaternion fromX =new Quaternion(GameObject.Find("CuboCamara").transform.rotation.x,0f,0f,GameObject.Find("CuboCamara").transform.rotation.w);
						Quaternion toX =new Quaternion(0f,GameObject.Find("CuboCamara").transform.rotation.y,0f,transform.rotation.w);
						
						
						//GameObject.Find("CuboCamara").transform.Rotate(Quaternion.Lerp(fromX, toX, Time.time * .001f));
						//GameObject.Find("CuboCamara").transform.Rotate(Vector3((GameObject.Find("CuboCamara").rotation.x *-1)*2,0f,0f));				
					}
					
					//transform.rotation=q;
					
					//Traslacion
					SkeletonJointPosition posicion=this.skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.Torso);
					if(posicion.Position.Z<1170F){
						//transform.Translate(Vector3.forward*10f*Time.deltaTime);
						transform.Translate(Vector3.forward * 0.107832f*5 *Time.deltaTime);
					}else if(posicion.Position.Z>1470F){
						//transform.Translate(Vector3.back*10f*Time.deltaTime);
						transform.Translate(Vector3.back * 0.107832f*5 *Time.deltaTime);
					}
					
					
					
					
				}
			}
		}
	}
	
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}

	
	
	public static Quaternion SkeletonJointOrientationToQuaternion(SkeletonJointOrientation m) {float tr = m.X1 + m.Y2 + m.Z3;

        float S = 0f;
		float qw = 0f;
		float qx = 0f;
		float qy = 0f;
		float qz = 0f;

        if(tr > 0) {
			S = Mathf.Sqrt(tr + 1.0f) * 2f;
			qw = 0.25f * S;
			qx = (m.Y3 - m.Z2) / S;
			qy = (m.Z1 - m.X3) / S;
			qz = (m.X2 - m.Y1) / S;

        } else if((m.X1 > m.Y2) && (m.X1 > m.Z3)) {
			S = Mathf.Sqrt(1.0f + m.X1 - m.Y2 - m.Z3) * 2f;
			qw = (m.Y3 - m.Z2) / S;
			qx = 0.25f * S;
			qy = (m.Y1 + m.X2) / S;
			qz = (m.Z1 + m.X3) / S;

        } else if(m.Y2 > m.Z3) {
			S = Mathf.Sqrt(1.0f + m.Y2 - m.X1 - m.Z3) * 2f;
			qw = (m.Z1 - m.X3) / S;
			qx = (m.Y1 + m.X2) / S;
			qy = 0.25f * S;
			qz = (m.Z2 + m.Y3) / S;

        } else {
			S = Mathf.Sqrt(1.0f + m.Z3 - m.X1 - m.Y2) * 2f;
			qw = (m.X2 - m.Y1) / S;
			qx = (m.Z1 + m.X3) / S;
			qy = (m.Z2 + m.Y3) / S;
			qz = 0.25f * S;
		}
		return new Quaternion(qx, qy, qz, qw);

    }
	
}
