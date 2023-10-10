using Godot;
using System;

public partial class AudioShot : AudioStreamPlayer2D
{
	private void _on_finished()
	{
		// delete this node when audio is finished
		this.QueueFree();
	}
}
