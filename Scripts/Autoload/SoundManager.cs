using Godot;
using System;
using System.Collections.Generic;

public partial class SoundManager : Node
{
	public static SoundManager Instance;
	Random rand = new Random();

	AudioStreamPlayer musicPlayer;
	AudioStreamPlayer soundPlayer;

	// music
	public AudioStreamWav warmup;

	// player sounds
	public AudioStreamWav player_jump;
	public AudioStreamWav player_land;
	public AudioStreamWav player_dash;
	public AudioStreamWav player_turn;
	public AudioStreamWav player_step1;
	public AudioStreamWav player_step2;

	// bashbox sounds
	public AudioStreamWav bashbox_wake;

	// key sounds
	public List<AudioStreamWav> key_note_pool = new List<AudioStreamWav>();
	public AudioStreamWav key_note1;
	public AudioStreamWav key_note2;
	public AudioStreamWav key_note3;
	public AudioStreamWav key_note4;
	public AudioStreamWav key_note5;
	public AudioStreamWav key_open;
	public AudioStreamWav key_land;

	// other sounds
	public AudioStreamWav door_lock;
	public AudioStreamWav door_open;
	public AudioStreamWav wallbounce;

	

	public override void _Ready()
	{
		Instance = this;
		musicPlayer = (AudioStreamPlayer)this.GetChild(0);
		soundPlayer = (AudioStreamPlayer)this.GetChild(1);

		//Events.Instance.SoundEffect += PlaySoundAtNode;

		LoadMusic();
		LoadPlayerSounds();

		LoadBashboxSounds();

		LoadOtherSounds();
		LoadKeySounds();
	}

	void LoadMusic(){
		warmup = (AudioStreamWav)GD.Load("res://Audio/Music/Warmup.wav");
	}

	void LoadPlayerSounds(){
		player_jump = (AudioStreamWav)GD.Load("res://Audio/Sound/Player/jump.wav");
		player_land = (AudioStreamWav)GD.Load("res://Audio/Sound/Player/land2.wav");
		player_dash = (AudioStreamWav)GD.Load("res://Audio/Sound/Player/newDash.wav");
		player_turn = (AudioStreamWav)GD.Load("res://Audio/Sound/Player/skid.wav");
		player_step1 = (AudioStreamWav)GD.Load("res://Audio/Sound/Player/step1.wav");
		player_step2 = (AudioStreamWav)GD.Load("res://Audio/Sound/Player/step2.wav");
	}

	void LoadBashboxSounds(){
		bashbox_wake = (AudioStreamWav)GD.Load("res://Audio/Sound/Bashbox/bashWake2.wav");
	}

	void LoadKeySounds(){
		key_note_pool.Add((AudioStreamWav)GD.Load("res://Audio/Sound/Key/keynote1.wav"));
		key_note_pool.Add((AudioStreamWav)GD.Load("res://Audio/Sound/Key/keynote2.wav"));
		key_note_pool.Add((AudioStreamWav)GD.Load("res://Audio/Sound/Key/keynote3.wav"));
		key_note_pool.Add((AudioStreamWav)GD.Load("res://Audio/Sound/Key/keynote4.wav"));
		key_note_pool.Add((AudioStreamWav)GD.Load("res://Audio/Sound/Key/keynote5.wav"));
		key_open = (AudioStreamWav)GD.Load("res://Audio/Sound/Key/KeyOpen.wav");
		key_land = (AudioStreamWav)GD.Load("res://Audio/Sound/Key/keyLand.wav");
	}

	void LoadOtherSounds(){
		door_lock = (AudioStreamWav)GD.Load("res://Audio/Sound/doorLock.wav");
		door_open = (AudioStreamWav)GD.Load("res://Audio/Sound/DoorOpen.wav");
		wallbounce = (AudioStreamWav)GD.Load("res://Audio/Sound/StageBounce.wav");
	}

	public void PlayMusic(AudioStreamWav track){
		musicPlayer.Stop();
		musicPlayer.Stream = track;
		musicPlayer.Play();
	}

	// used for menu sounds
	public void PlaySound(AudioStreamWav sound){
		soundPlayer.Stream = sound;
		soundPlayer.Play();
	}

	public AudioStreamWav GetKeyNote(){
		return key_note_pool[rand.Next(0,5)];
	}

	// optional volume adjustment with (db) parameter
	public void PlaySoundAtNode(AudioStreamWav sound, Node2D node, float db){
		// add new custom AudioStreamPlayer2D to node,
		AudioShot newAudioShot = (AudioShot)Database.Instance.audioShot.Instantiate();
		newAudioShot.AddToGroup("AudioShots");
		node.AddChild(newAudioShot);
		// then play sound from that node,
		newAudioShot.VolumeDb = db;
		newAudioShot.Stream = sound;
		newAudioShot.Play();
		// the Audioshot script should then queueFree the node when audio is finished playing

	}

	public void CutSoundsAtNode(Node2D node){
		// get all sound source children of player and remove them
        var currentSounds = GetTree().GetNodesInGroup("AudioShots");
        foreach (var sound in currentSounds){
            if (sound.GetParent() == node){
                sound.QueueFree();
            }
        }
	}
}
