using UnityEngine;
public class ExitGate : MonoBehaviour
{
    void Update(){ transform.Rotate(0,18f*Time.deltaTime,0); }
    void OnTriggerEnter(Collider other){ var p=other.GetComponent<Player3D>(); if(p!=null){ if(GameBootstrap.Instance.Manager.Cells>=3) GameBootstrap.Instance.Manager.Win(); else GameBootstrap.Instance.Manager.Alert("El portal está sellado. Faltan runas ancestrales."); } }
}
