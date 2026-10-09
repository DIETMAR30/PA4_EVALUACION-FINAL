using UnityEngine;
public class FollowCamera3D : MonoBehaviour
{
    public Transform target; public Vector3 offset = new Vector3(0,3.4f,-6.2f); public float smooth = 6f;
    void LateUpdate(){ if(!target)return; Vector3 desired=target.position+offset; transform.position=Vector3.Lerp(transform.position,desired,smooth*Time.deltaTime); transform.LookAt(target.position+Vector3.up*1.1f); }
}
