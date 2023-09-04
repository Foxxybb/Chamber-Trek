#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// universal script used by all objects that can be affected by hitboxes

public class HitHandler : MonoBehaviour
{
    public int hitstop;
    public bool inHitstop;
    public Vector2 storedVel;
    public Vector2 backupVel;

    public int hitstun;
    public bool inHitstun;

    public bool facingRight;
    public Action currentAction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleHitstop();
        HandleHitstun();
    }

    public void ApplyHitstop(int newHitstop, Vector2 newVel){
        hitstop = newHitstop;
        storedVel = newVel;
    }

    public void ApplyHitstun(int newHitstun){
        hitstun = newHitstun;
    }

    void HandleHitstop(){
        
        if ((storedVel != Vector2.zero)){
            backupVel = storedVel;
        }

        if (hitstop > 0){
            inHitstop = true;
            hitstop--;
        } else {
            inHitstop = false;
        }
    }

    void HandleHitstun(){

        if (hitstun > 0){
            inHitstun = true;
            if (!inHitstop){
                hitstun--;
            }
            
        } else {
            inHitstun = false;
        }

        
    }
}
#endif