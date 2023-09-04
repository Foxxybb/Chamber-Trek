#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS HAS BEEN REPLACED BY: NewHitbox

public class Hitbox : MonoBehaviour
{
    GameObject owner; // object that is parent of hitbox (player)
    Player player; // player script reference if hitbox is owned by player
    Action currentAction; // holds attack properties of this hitbox
    
    Vector2 ownerVel;
    Vector3 hbSize;
    Vector2 hbOffset;
    Vector2 hitSparkPos;

    //public int attackStrength; // used to determine what sound and effect to use

    // these values are reversed if player is facing left
    public Vector2 knockback; 
    public Vector2 selfKnockback;
    public Vector2 stageKnockback;

    public int lifespan; // for debugging

    // store list of game objects that have been hit by this hitbox
    // so they are not hit multiple times
    List<GameObject> victims = new List<GameObject>();

    // save bounce to bool to prevent multiple bounces
    bool bounced;

    void Start()
    {
        owner = this.transform.parent.gameObject;
        hbSize = GetComponent<BoxCollider2D>().size;
        hbOffset = GetComponent<BoxCollider2D>().offset;

        lifespan = 1;

        player = owner.GetComponent<Player>();
        currentAction = player.currentAction;


        // setup hitbox properties
        if (!player.facingRight) {
            knockback.x = (currentAction.knockback.x * (-1));
            knockback.y = currentAction.knockback.y;
            selfKnockback.x = (currentAction.selfKnockback.x * (-1));
            selfKnockback.y = currentAction.selfKnockback.y;
            stageKnockback.x = (currentAction.stageKnockback.x * (-1));
            stageKnockback.y = currentAction.stageKnockback.y;
        } else {
            knockback = currentAction.knockback;
            selfKnockback = currentAction.selfKnockback;
            stageKnockback = currentAction.stageKnockback;
        }
    }

    void Update(){
        //Physics2D.OverlapBox
        MyCollisions();
        
        lifespan++;
    }

    void MyCollisions(){
        Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)this.transform.position + hbOffset,hbSize, 0);
        foreach (Collider2D hit in hits){
            TriggerHitbox(hit);
        }
    }

    void TriggerHitbox(Collider2D other){
        // if object has not been hit yet, add it to list of victims
        if (!victims.Contains(other.gameObject) && other.tag == "Attackable") {
            victims.Add(other.gameObject);
            // need to store velocity of player, victim velocity is reset
            ApplyHitbox(other);
        }

        // if object is "Stage" apply knockback to player
        if (!bounced && (currentAction.stageKnockback != Vector2.zero) && other.gameObject.layer == LayerMask.NameToLayer("Stage")){
            // for movement consistency, current player velocity is nullified on the same frame that knockback is applied
            owner.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            owner.GetComponent<Rigidbody2D>().AddForce(stageKnockback, ForceMode2D.Impulse);

            //print("stage bounce");
            // spawn special spark and play special sound
            // GameObject.Instantiate(Database.Instance.hitsparks[0],
            // this.transform.position, // position
            // Quaternion.identity); // rotation

            SoundManager.Instance.PlayPlayerSound(Database.Instance.stageBounce);

            bounced = true;
        }
    }

    // fired when attackable object hit
    void ApplyHitbox(Collider2D other)
        {   
            // apply hitstop (to both), pass velocity as parameter to store
            player.hh.ApplyHitstop(currentAction.hitstop, owner.GetComponent<Rigidbody2D>().velocity);
            player.jumpCancel = true;

            // Object specific handles with hitboxes
            // all attackable objects
            // BOX
            if (other.gameObject.GetComponent<Box>() != null){
                
                // apply selfKnockback
                if (currentAction.selfKnockback != Vector2.zero) { player.hh.storedVel = selfKnockback; }

                // Box script short
                Box box = other.GetComponent<Box>();

                if (player.currentAction.hitSound != null){
                    SoundManager.Instance.PlayPlayerSound(player.currentAction.hitSound);
                } else {
                    SoundManager.Instance.PlayPlayerSound(Database.Instance.placeholderHit);
                }

                // check X position of victim if (reverseHitbox)
                if (currentAction.reverseHitbox){
                    if ((player.facingRight && (owner.transform.position.x > other.transform.position.x)) ||
                        (!player.facingRight && (owner.transform.position.x < other.transform.position.x))){
                            //Debug.Log("reverse");
                            //box.ApplyHit(currentAction.hitstop, new Vector2(knockback.x*(-1), knockback.y));
                        }
                    else {
                        //box.ApplyHit(currentAction.hitstop, knockback);
                    }
                } else {
                    //box.ApplyHit(currentAction.hitstop, knockback);
                }

                box.launched = currentAction.launcher;

                
            } // KEY 
            else if (other.gameObject.GetComponent<Key>() != null){
                // Key script short
                Key key = other.GetComponent<Key>();

                if (currentAction.launcher){
                    SoundManager.Instance.PlayRandomKeyNote();
                } else {
                    SoundManager.Instance.PlayRandomKeyHit();
                }

                // check X position of victim if (reverseHitbox)
                if (currentAction.reverseHitbox){
                    if ((player.facingRight && (owner.transform.position.x > other.transform.position.x)) ||
                        (!player.facingRight && (owner.transform.position.x < other.transform.position.x))){
                            //Debug.Log("reverse");
                            //key.ApplyHit(currentAction.hitstop, new Vector2(knockback.x*(-1), knockback.y));
                        }
                    else {
                        //key.ApplyHit(currentAction.hitstop, knockback);
                    }
                } else {
                    //key.ApplyHit(currentAction.hitstop, knockback);
                }

                key.launched = currentAction.launcher;

            } // BASHBOX 
            else if (other.gameObject.GetComponent<Bashbox>() != null){
                // BASHBOX script short
                Bashbox bashbox = other.GetComponent<Bashbox>();

                if (player.currentAction.hitSound != null){
                    SoundManager.Instance.PlayPlayerSound(player.currentAction.hitSound);
                } else {
                    SoundManager.Instance.PlayPlayerSound(Database.Instance.placeholderHit);
                }

                // check X position of victim if (reverseHitbox)
                if (currentAction.reverseHitbox){
                    if ((player.facingRight && (owner.transform.position.x > other.transform.position.x)) ||
                        (!player.facingRight && (owner.transform.position.x < other.transform.position.x))){
                            //Debug.Log("reverse");
                            //bashbox.ApplyHit(currentAction.hitstop, new Vector2(knockback.x*(-1), knockback.y));
                        }
                    else {
                        //bashbox.ApplyHit(currentAction.hitstop, knockback);
                    }
                } else {
                    //bashbox.ApplyHit(currentAction.hitstop, knockback);
                }

                bashbox.ApplyHitstun(currentAction.hitstun);
                bashbox.ApplyDamage(currentAction.damage);

                // make enemy face direction being attacked from
                if ((owner.transform.position.x > other.transform.position.x)){
                    bashbox.facingRight = true;
                } else {
                    bashbox.facingRight = false;
                }
            }

            // get midpoint between hitbox and target to spawn hitspark
            hitSparkPos.x = (this.transform.position.x + other.gameObject.transform.position.x)/2;
            hitSparkPos.y = this.transform.position.y;
            //hitSparkPos = (other.gameObject.transform.position - this.gameObject.transform.position);

            // spawn hitspark
            GameObject.Instantiate(Database.Instance.sparks,
            hitSparkPos, // position
            Quaternion.identity); // rotation

            // shake camera
            if (currentAction.shakeOnHit){
                Oracle.Instance.ShakeCamera(currentAction.shakeDuration, currentAction.shakeMagnitude);
            }
        }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube((Vector2)this.transform.position + hbOffset, hbSize);
    }

    
}
#endif