using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Mirror;


public class VRMain : NetworkBehaviour
{
    private List<UnityEngine.XR.InputDevice> Devices = new List<UnityEngine.XR.InputDevice>();
    private List<UnityEngine.XR.XRNodeState> Nodes = new List<UnityEngine.XR.XRNodeState>();

    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject PositionRef;
    private GameObject CamBase;
    public GameObject Camera;
    public GameObject SeatTarget;
    [HideInInspector]
    public float PlayerHeight = 0;
    public float SetPlayerHeight = 1.828f; //The height we offer the player might not be used if we don't detect a rift.
   //public GameObject CameraParent;
    private XRNodeState LeftHandState;
    private XRNodeState RightHandState;
    private InputDevice RightHandIn;
    private InputDevice LeftHandIn;

    public bool LocalFlag;
    

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            LocalFlag = true;
            Camera.GetComponent<Camera>().enabled = true;
            PlayerHeight = 0;
            CamBase = gameObject;
            UnityEngine.XR.InputDevices.GetDevices(Devices);
            foreach (var device in Devices)
            {
                if (device.role == InputDeviceRole.RightHanded)
                {
                    RightHandIn = device;
                }
                if (device.role == InputDeviceRole.LeftHanded)
                {
                    LeftHandIn = device;
                }
                if (device.name == "Oculus Rift")
                {
                    PlayerHeight = SetPlayerHeight;
                }

            }
        }
        else
        {
            Camera.GetComponent<Camera>().enabled = false;
            LocalFlag = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        if (isLocalPlayer)
        {
            UnityEngine.XR.InputTracking.GetNodeStates(Nodes);
            foreach (var node in Nodes)
            {
                //print(node.nodeType);
                if (node.nodeType == XRNode.RightHand)
                {
                    RightHandState = node;
                }
                if (node.nodeType == XRNode.LeftHand)
                {
                    LeftHandState = node;
                }
            }

            //Right Hand Tracking
            Vector3 RightHandPos;
            Quaternion RightHandRot;
            if (RightHandState.TryGetPosition(out RightHandPos))
            {
                RightHand.transform.position = CamBase.transform.TransformPoint(RightHandPos);
            }
            if (RightHandState.TryGetRotation(out RightHandRot))
            {
                RightHand.transform.rotation = CamBase.transform.rotation * RightHandRot;
            }

            //Left Hand Tracking
            Vector3 LeftHandPos;
            Quaternion LeftHandRot;
            if (LeftHandState.TryGetPosition(out LeftHandPos))
            {
                LeftHand.transform.position = CamBase.transform.TransformPoint(LeftHandPos);
            }
            if (LeftHandState.TryGetRotation(out LeftHandRot))
            {
                LeftHand.transform.rotation = CamBase.transform.rotation * LeftHandRot;
            }

            //Right Hand Button Mapping.
            bool rightprimarybutton;
            RightHandIn.TryGetFeatureValue(CommonUsages.primaryButton, out rightprimarybutton);
            bool rightgrip;
            RightHandIn.TryGetFeatureValue(CommonUsages.gripButton, out rightgrip);
            Vector2 TrackPosR;
            RightHandIn.TryGetFeatureValue(CommonUsages.primary2DAxis, out TrackPosR);
            bool right2dclick;
            RightHandIn.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out right2dclick);


            //Left Hand Button Mapping.
            bool leftprimarybutton;
            LeftHandIn.TryGetFeatureValue(CommonUsages.primaryButton, out leftprimarybutton);
            bool leftgrip;
            LeftHandIn.TryGetFeatureValue(CommonUsages.gripButton, out leftgrip);
            Vector2 TrackPosL;
            LeftHandIn.TryGetFeatureValue(CommonUsages.primary2DAxis, out TrackPosL);
            bool left2dclick;
            LeftHandIn.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out left2dclick);


            //Left Hand Haptics
            if (LeftHand.GetComponent<Hand>().shake != 0)
            {
                LeftHandIn.SendHapticImpulse(0, LeftHand.GetComponent<Hand>().shake);
            }

            //Right Hand Haptics
            if (RightHand.GetComponent<Hand>().shake != 0)
            {
                RightHandIn.SendHapticImpulse(0, RightHand.GetComponent<Hand>().shake);
            }


            //Forward Grab Instructions to hands
            if (rightgrip)
            {
                RightHand.GetComponent<Hand>().grab = true;
            }
            else
            {
                RightHand.GetComponent<Hand>().grab = false;
            }

            if (leftgrip)
            {
                LeftHand.GetComponent<Hand>().grab = true;
            }
            else
            {
                LeftHand.GetComponent<Hand>().grab = false;
            }

            //Forward Primary Button Instructions to hands
            if (rightprimarybutton)
            {
                RightHand.GetComponent<Hand>().interact = true;
            }
            else
            {
                RightHand.GetComponent<Hand>().interact = false;
            }
            if (leftprimarybutton)
            {
                LeftHand.GetComponent<Hand>().interact = true;
            }
            else
            {
                LeftHand.GetComponent<Hand>().interact = false;
            }

            //Forward 2d Axis click to hands
            if (right2dclick)
            {
                RightHand.GetComponent<Hand>().click2d = true;
            }
            else
            {
                RightHand.GetComponent<Hand>().click2d = false;
            }
            if (left2dclick)
            {
                LeftHand.GetComponent<Hand>().click2d = true;
            }
            else
            {
                LeftHand.GetComponent<Hand>().click2d = false;
            }

            //Forward trackpad position to hands
            RightHand.GetComponent<Hand>().trackpadpos = TrackPosR;
            LeftHand.GetComponent<Hand>().trackpadpos = TrackPosL;

            //Forward device velocity info to hands
            Vector3 RightDV;
            RightHandIn.TryGetFeatureValue(CommonUsages.deviceVelocity, out RightDV);
            RightHand.GetComponent<Hand>().DeviceVelocity = RightDV;
            Vector3 LeftDV;
            LeftHandIn.TryGetFeatureValue(CommonUsages.deviceVelocity, out LeftDV);
            LeftHand.GetComponent<Hand>().DeviceVelocity = LeftDV;

            Vector3 RightDAV;
            RightHandIn.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out RightDAV);
            RightHand.GetComponent<Hand>().DeviceAngVel = RightDAV;
            Vector3 LeftDAV;
            LeftHandIn.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out LeftDAV);
            LeftHand.GetComponent<Hand>().DeviceAngVel = LeftDAV;
        }
        
    }

    



    public void Recenter()
    {
        
        CamBase.transform.position = CamBase.transform.position + (SeatTarget.transform.position - Camera.transform.position);
        var ang = (SeatTarget.transform.eulerAngles - Camera.transform.eulerAngles).y;
        CamBase.transform.RotateAround(Camera.transform.position, CamBase.transform.up, ang);

    }




}
