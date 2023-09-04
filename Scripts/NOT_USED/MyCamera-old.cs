#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// camera script for gameplay scenes

public class MyCamera : MonoBehaviour
{
    public Transform target;
    public float zOffset = -5f; // zoom level

    // use stage gameobject as origin to set bounds for camera
    public GameObject stage;
    public Vector2 stageOrigin;
    public GameObject foreground; // move foreground.x when camera moves
    public int foregroundOffsetY;
    public float scrollRate;

    // camera bounds
    public GameObject leftBar;
    public GameObject rightBar;
    public GameObject bottomBar;
    public GameObject topBar;
    float camWidth = 18;
    float camHeight = 10;
    

    // lerp stuff
    public float lerpTime = 3f;
    public float elapsedTime;
    public bool lerping;

    // clamp values
    [Header("Camera Bounds")]
    public float xMin = -100;
    public float xMax = 100;
    public float yMin = -100;
    public float yMax = 100;

    void Start(){
        // default target is door, then shifts to player
        // wrap this in pregame script
        // target = GameObject.Find("Door").transform;
        // stage = GameObject.Find("Stage");
        // stageOrigin = stage.transform.position; // origin of stage and camera bounds are automatic

        //StartCoroutine(Pregame());

        // set bounds of camera using barriers

    }

    void Update()
    {
        if (Oracle.Instance.paused){
            //this.enabled = false;
        } else {

        // screen shake test
        // if (Input.GetKeyDown(KeyCode.Space)){
        //     StartCoroutine(Shake(0.2f,0.1f));
        // }

        if (target != null){
            if (lerping){
                LerpToTarget();
            } else {
                TrackTarget();
            }
        }
        
        // lerp test
        if (Input.GetKeyDown(KeyCode.M)){
            ChangeTarget("Door");
        }
        if (Input.GetKeyDown(KeyCode.N)){
            ChangeTarget("Player");
        }

        // maintain offset
        transform.position = new Vector3(transform.position.x,transform.position.y,zOffset);
        }
    }

    public IEnumerator Shake(float duration, float magnitude){
        Vector3 origin = transform.position;

        float elapsed = 0.0f;

        while (elapsed < duration){
            float x = (Random.Range(-1f, 1f) * magnitude)*(1-(elapsed/duration)) + origin.x;
            float y = (Random.Range(-1f, 1f) * magnitude)*(1-(elapsed/duration)) + origin.y;

            transform.position = new Vector3(x,y, origin.z);

            elapsed += Time.deltaTime;

            yield return null;
        }
    }

    void TrackTarget(){
        //float x = Mathf.Clamp(target.position.x, stageOrigin.x + xMin, stageOrigin.x + xMax);
        //float y = Mathf.Clamp(target.position.y, stageOrigin.y + yMin, stageOrigin.y + yMax);

        // Y clamp has extra 0.5 to fully show ground

        float x = Mathf.Clamp(target.position.x, leftBar.transform.position.x + (camWidth/2), rightBar.transform.position.x - (camWidth/2));
        float y = Mathf.Clamp(target.position.y, bottomBar.transform.position.y + (float)((camHeight/2)+0.5), topBar.transform.position.y - (float)((camHeight/2)+0.5));

        transform.position = new Vector3(x,y,0);
        MoveForeground();
    }

    void LerpToTarget(){
        lerping = true;
        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime/lerpTime;

        transform.position = Vector2.Lerp(this.transform.position,
            target.position,
            Mathf.SmoothStep(0,1,percentageComplete));

        // clamp camera
        //float x = Mathf.Clamp(target.position.x, stageOrigin.x + xMin, stageOrigin.x + xMax);
        //float y = Mathf.Clamp(target.position.y, stageOrigin.y + yMin, stageOrigin.y + yMax);

        // ugly clamp X
        if (transform.position.x > (rightBar.transform.position.x - (camWidth/2))){
            //transform.position = new Vector3(stageOrigin.x + xMax,transform.position.y);
            transform.position = new Vector3(rightBar.transform.position.x - (camWidth/2), transform.position.y);
        } else if (transform.position.x < (leftBar.transform.position.x + (camWidth/2))){
            transform.position = new Vector3(leftBar.transform.position.x + (camWidth/2), transform.position.y);
        }

        // ugly clamp Y
        if (transform.position.y > (topBar.transform.position.y - (float)((camHeight/2)+0.5))){
            transform.position = new Vector3(transform.position.x, topBar.transform.position.y - (float)((camHeight/2)+0.5));
        } else if (transform.position.y < (bottomBar.transform.position.y + (float)((camHeight/2)+0.5))){
            transform.position = new Vector3(transform.position.x, bottomBar.transform.position.y + (float)((camHeight/2)+0.5));
        }

        if (percentageComplete >= .5f){
            lerping = false;
        }

        MoveForeground();
    }

    public void ChangeTarget(string nameOfTarget){
        target = GameObject.Find(nameOfTarget).transform;
        elapsedTime = 0;
        LerpToTarget();
    }

    void MoveForeground(){
        // move foreground when camera moves
        if (foreground != null){
            //foreground.GetComponent<RectTransform>().sizeDelta = new Vector2(stageOrigin.x-transform.position.x,foregroundOffsetY);
            foreground.GetComponent<RectTransform>().anchoredPosition =
             new Vector2((stageOrigin.x-transform.position.x)*scrollRate,
             ((stageOrigin.y-transform.position.y)*scrollRate)+(foregroundOffsetY));
        }
    }
}
#endif