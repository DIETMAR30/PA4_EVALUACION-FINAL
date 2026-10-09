using UnityEngine;
public class CollectibleCell : MonoBehaviour
{
    bool collected;
    void Update(){ if(!collected) transform.Rotate(0,80f*Time.deltaTime,0,Space.World); }
    void OnTriggerEnter(Collider other){ if(collected)return; if(other.GetComponent<Player3D>()!=null){collected=true; GameBootstrap.Instance.Pickup(transform); gameObject.SetActive(false);} }
}
