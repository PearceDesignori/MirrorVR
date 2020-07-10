using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.XR;

public class Hand : MonoBehaviour
{
    private GameObject self;
    private VRMain MainScript;
    public bool grab;
    public bool interact;
    public Vector2 trackpadpos;
    public float shake;

    private bool TeleportMode = false;
    private bool TeleportModeActive = false;
    private bool clicking = false;
    private float LaunchAngle;
    public float ArcReferenceVelocity;

    public int ArcResolution = 25;
    private LineRenderer LR;
    public Material ArcMaterial;
    public Color ArcColor;
    private GameObject TeleportTarget;
    private bool R;
    private Vector3 TeleportMark;

    public Vector3 DeviceVelocity;
    public Vector3 DeviceAngVel;
    private List<Vector3> SmoothVel = new List<Vector3>();
    private Vector3 SmoothedVel;

    

    public bool click2d;

    // Start is called before the first frame update
    void Start()
    {
        self = gameObject;
        MainScript = self.GetComponentInParent<VRMain>();

        LR = self.AddComponent<LineRenderer>();
        LR.startWidth = 0.05f;
        LR.endWidth = 0.05f;
        LR.enabled = false;
        LR.material = ArcMaterial;
        LR.startColor = ArcColor;
        LR.endColor = new Color(1, 0, 0, 1);
    }

    // Update is called once per frame

    void Update()
    {

        if(click2d & !clicking) //Only trigger this code when the button is pushed for the first time.
        {

            //--------Teleport Activation--------
            if(TeleportMode == false)
            {
                TeleportMode = true;
            }
            else
            {
                TeleportMode = false;
            }


            clicking = true; //Note that we are holding the button so we don't spam the button press code.
        }

        if (!click2d) //Reset the button hold check when we release the button.
        {
            clicking = false;
            TeleportMode = false;
        }

        //Trajectory Teleport Setup
        if(TeleportMode) //Only initialize the teleport mode if we aren't already in it.
        {
            if (!TeleportModeActive)
            {
                for (int i = 0; i <= ArcResolution; i++)
                {

                }
                LR.enabled = true;
                TeleportTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                TeleportTarget.GetComponent<SphereCollider>().enabled = false;
                TeleportModeActive = true; //Note that we are now in teleport mode so we don't keep initalizing it.
            }

            LR.positionCount = ArcResolution;

            Vector3 ForwardRef = gameObject.transform.TransformPoint(new Vector3(0,0,5));
            ForwardRef.y = gameObject.transform.position.y;

            //Arc Calculator
            LaunchAngle = Vector3.SignedAngle(gameObject.transform.forward, ForwardRef - gameObject.transform.position, Vector3.Cross(ForwardRef + new Vector3(0,10,0), ForwardRef - gameObject.transform.position));
            
            float ArcDistance = ((ArcReferenceVelocity * Mathf.Cos(LaunchAngle * Mathf.Deg2Rad)) / 2) * ((ArcReferenceVelocity * Mathf.Sin(LaunchAngle * Mathf.Deg2Rad)) + Mathf.Sqrt(Mathf.Pow(ArcReferenceVelocity * Mathf.Sin(LaunchAngle * Mathf.Deg2Rad), 2) + 2 * 2 * gameObject.transform.position.y));

            Vector3[] Verts = new Vector3[ArcResolution];
            for(int i = 0; i < ArcResolution; i++)
            {
                Verts[i] = gameObject.transform.position + (((Vector3.Normalize(ForwardRef - gameObject.transform.position) / (float)Verts.Length) * (float)i) * ArcDistance) + new Vector3(0, CalcArc(((float)i) / ((float)Verts.Length - 1), ArcDistance), 0);

                
            }

            for (int i = 0; i < ArcResolution - 1; i++)
            {
                RaycastHit hit;
                R = Physics.Linecast(Verts[i], Verts[i + 1], out hit);
                TeleportTarget.transform.position = hit.point;
                LR.SetPosition(i, Verts[i]);
                if (R)
                {
                    LR.positionCount = i + 1;
                    LR.SetPosition(i, hit.point);
                    TeleportMark = hit.point;
                    break;
                }
            }
        }
        else
        {
            if (TeleportModeActive)
            {
                LR.enabled = false;
                Destroy(TeleportTarget);
                if (R)
                {
                    gameObject.GetComponentInParent<VRMain>().transform.position = gameObject.GetComponentInParent<VRMain>().transform.position + (TeleportMark - new Vector3(gameObject.GetComponentInParent<VRMain>().Camera.transform.position.x, gameObject.GetComponentInParent<VRMain>().gameObject.transform.position.y, gameObject.GetComponentInParent<VRMain>().Camera.transform.position.z)) + new Vector3(0, gameObject.GetComponentInParent<VRMain>().PlayerHeight, 0);
                }
            }
            TeleportModeActive = false;
        }

        SmoothVel.Add(DeviceVelocity);
        if(SmoothVel.Count > 10)
        {
            SmoothVel.RemoveAt(0);
        }
        SmoothedVel = new Vector3(0, 0, 0);
        foreach(Vector3 v in SmoothVel)
        {
            SmoothedVel = SmoothedVel + v;
        }

        SmoothedVel = SmoothedVel / 10;
        //Debug.DrawLine(self.transform.position, self.transform.position + SmoothedVel);
    }

    float CalcArc(float t, float dist) //Note: T analogous to "At what percent of the max distance are we". Should be a fraction.
    {
        float x = t * dist;
        float y = x * Mathf.Tan(LaunchAngle * Mathf.Deg2Rad) - ((2 * x * x) / (2 * ArcReferenceVelocity * ArcReferenceVelocity * Mathf.Cos(LaunchAngle * Mathf.Deg2Rad) * Mathf.Cos(LaunchAngle * Mathf.Deg2Rad)));
        return y;
    }

    private void OnTriggerStay(Collider other)
    {

        if(grab)
        {
            if(other.GetComponent<Rigidbody>() != null)
            {
                other.transform.position = self.transform.position - (other.GetComponent<Renderer>().bounds.center - other.transform.position);
                other.tag = "GrabbedObject";
                if(other.GetComponent<AuthorityAssigner>() != null)
                {
                    other.GetComponent<AuthorityAssigner>().Assign(MainScript.transform.GetComponent<NetworkIdentity>().connectionToClient, MainScript.gameObject);
                }
            }

        }
        else
        {
            if(other.tag == "GrabbedObject")
            {
                other.GetComponent<Rigidbody>().velocity = SmoothedVel * 7;
                other.GetComponent<Rigidbody>().angularVelocity = DeviceAngVel;
                //other.GetComponent<Rigidbody>().angularVelocity = 
                other.tag = "Untagged";
            }


        }


        /* ------------------OBSOLETE CODE FROM THE SPACE SIM IMPLEMENTATION OF VR. USEFUL REFERENCE POSSIBLY.-----------
        if(grab)
        {
            
            if(other.GetComponent<Joystick>()!= null)
            {
                other.GetComponent<Joystick>().grabbed = true;
                other.GetComponent<Joystick>().HoldingObj = self;

            }
            
            else
            {
                other.transform.position = self.transform.position - (other.GetComponent<Renderer>().bounds.center - other.transform.position);
                shake = 0;
            }

        }
        else
        {
            if (other.GetComponent<Joystick>() != null)
            {
                other.GetComponent<Joystick>().grabbed = false;
            }
            
            shake = 0;
        }
        */
    }
}
