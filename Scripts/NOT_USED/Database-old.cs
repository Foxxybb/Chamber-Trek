#if tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// this script uses Singleton

public class Database : MonoBehaviour
{
    public static Database Instance;
    
    // ACTIONS (replacing "attacks")
    [HideInInspector] public Action punch1;
    [HideInInspector] public Action jumpKick;
    [HideInInspector] public Action flip;
    [HideInInspector] public Action lowflip;
    [HideInInspector] public Action turnpunch;
    [HideInInspector] public Action stomp;

    // ENEMY ACTIONS
    [HideInInspector] public Action bashbox_lunge;

    // prefabs
    [HideInInspector] public GameObject hitbox;
    [HideInInspector] public GameObject enemyhitbox;
    [HideInInspector] public GameObject box;
    [HideInInspector] public GameObject groundSpark;
    [HideInInspector] public GameObject afterImage;
    [HideInInspector] public GameObject keySpawner;

    // effects
    [HideInInspector] public GameObject keySpark;
    [HideInInspector] public GameObject splosion;
    [HideInInspector] public GameObject sparks;
    [HideInInspector] public GameObject dust;

    // hitsparks
    [HideInInspector] public List<GameObject> hitsparks;

    // hitsounds
    [HideInInspector] public AudioClip placeholderHit;

    // misc sounds
    [HideInInspector] public AudioClip keyOpen, stageBounce;

    // player sounds
    [HideInInspector] public AudioClip jump;
    [HideInInspector] public AudioClip land;
    [HideInInspector] public AudioClip dash;
    [HideInInspector] public AudioClip skid;
    [HideInInspector] public AudioClip spawn;
    [HideInInspector] public AudioClip exit;
    [HideInInspector] public AudioClip step;
    [HideInInspector] public AudioClip step1;
    [HideInInspector] public AudioClip blip;

    //key
    [HideInInspector] public List<AudioClip> keyNotes;
    [HideInInspector] public List<AudioClip> keyHits;
    [HideInInspector] public AudioClip keyLand;
    [HideInInspector] public AudioClip keyPort;

    // door
    [HideInInspector] public AudioClip doorOpen, doorLock;

    //bashbox
    [HideInInspector] public AudioClip bashWake;

    void Awake(){
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    void Start(){
        GetPlayerActions();
        GetEnemyActions();

        GetPlayerSounds();
        GetKeySounds();
        GetBashBoxSounds();
        GetHitSounds();
        GetMiscSounds();
        GetHitSparks();

        GetPrefabs();
        GetEffects();
    }

    void GetPrefabs(){
        hitbox = Resources.Load<GameObject>("Prefabs/Hitbox");
        enemyhitbox = Resources.Load<GameObject>("Prefabs/enemyHitbox");
        box = Resources.Load<GameObject>("Prefabs/Box");
        groundSpark = Resources.Load<GameObject>("Prefabs/groundSpark");
        afterImage = Resources.Load<GameObject>("Prefabs/afterImage");
        keySpawner = Resources.Load<GameObject>("Prefabs/KeySpawner");
    }

    void GetEffects(){
        keySpark = Resources.Load<GameObject>("Prefabs/effects/KeySpark");
        splosion = Resources.Load<GameObject>("Prefabs/effects/splosion");
        sparks = Resources.Load<GameObject>("Prefabs/effects/sparks");
        dust = Resources.Load<GameObject>("Prefabs/effects/dust");
    }

    void GetPlayerActions(){
        punch1 = Resources.Load<Action>("Actions/Punch1");
        jumpKick = Resources.Load<Action>("Actions/JumpKick");
        flip = Resources.Load<Action>("Actions/Flip");
        lowflip = Resources.Load<Action>("Actions/LowFlip");
        turnpunch = Resources.Load<Action>("Actions/TurnPunch");
        stomp = Resources.Load<Action>("Actions/Stomp");

    }

    void GetEnemyActions(){
        bashbox_lunge = Resources.Load<Action>("EnemyActions/Bashbox_Lunge");
    }

    void GetPlayerSounds(){
        jump = Resources.Load<AudioClip>("Sounds/Player/jump");
        land = Resources.Load<AudioClip>("Sounds/Player/land");
        dash = Resources.Load<AudioClip>("Sounds/Player/newDash");
        skid = Resources.Load<AudioClip>("Sounds/Player/skid");
        spawn = Resources.Load<AudioClip>("Sounds/Player/spawn");
        exit = Resources.Load<AudioClip>("Sounds/Player/exit");
        step = Resources.Load<AudioClip>("Sounds/Player/step2");
        step1 = Resources.Load<AudioClip>("Sounds/Player/step1");
        blip = Resources.Load<AudioClip>("Sounds/Player/blip");

    }

    void GetHitSounds(){
        // strength 0, 1, 2, etc.
        //hit = Resources.Load<AudioClip>("Sounds/hits/hit");
        placeholderHit = Resources.Load<AudioClip>("Sounds/hits/hit1");
    }

    void GetHitSparks(){
        // strength 0, 1, 2, etc.
        hitsparks.Add(Resources.Load<GameObject>("Prefabs/hitsparks/LightSpark"));
        hitsparks.Add(Resources.Load<GameObject>("Prefabs/hitsparks/MedSpark"));
    }

    void GetMiscSounds(){
        doorOpen = Resources.Load<AudioClip>("Sounds/Door/DoorOpen");
        doorLock = Resources.Load<AudioClip>("Sounds/Door/doorLock");

        stageBounce = Resources.Load<AudioClip>("Sounds/StageBounce");
    }

    void GetKeySounds(){
        keyOpen = Resources.Load<AudioClip>("Sounds/Key/KeyOpen");

        keyNotes.Add(Resources.Load<AudioClip>("Sounds/Key/keynote1"));
        keyNotes.Add(Resources.Load<AudioClip>("Sounds/Key/keynote2"));
        keyNotes.Add(Resources.Load<AudioClip>("Sounds/Key/keynote3"));
        keyNotes.Add(Resources.Load<AudioClip>("Sounds/Key/keynote4"));
        keyNotes.Add(Resources.Load<AudioClip>("Sounds/Key/keynote5"));

        keyHits.Add(Resources.Load<AudioClip>("Sounds/Key/keyhit1"));
        keyHits.Add(Resources.Load<AudioClip>("Sounds/Key/keyhit2"));
        keyHits.Add(Resources.Load<AudioClip>("Sounds/Key/keyhit3"));
        keyHits.Add(Resources.Load<AudioClip>("Sounds/Key/keyhit4"));
        keyHits.Add(Resources.Load<AudioClip>("Sounds/Key/keyhit5"));

        keyLand = Resources.Load<AudioClip>("Sounds/Key/keyLand");
        keyPort = Resources.Load<AudioClip>("Sounds/Key/keyPort");
    }
    
    void GetBashBoxSounds(){
        bashWake = Resources.Load<AudioClip>("Sounds/Bashbox/bashWake2");
    }
}
#endif