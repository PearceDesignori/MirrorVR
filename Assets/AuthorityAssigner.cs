using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AuthorityAssigner : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Assign(NetworkConnection Connection, GameObject Initiator)
    {
        NetworkIdentity ni = gameObject.GetComponent<NetworkIdentity>();
        ni.AssignClientAuthority(Connection);
        if(Initiator.GetComponent<VRMain>().LocalFlag == true)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        
    }
}
