using Godot;

namespace Redirect3DS;

public partial class GlobalVars : Node
{
	public static GlobalVars Instance { get; private set; }

	[Export] public string GamepadTitleFormat = "[b][u]Connected Gamepads[/u][/b] [i][color=#bbbbbb]({0})[/color][/i]";
	
	[Export] public int PacketsPerSecond = 45;
	public int CurrentController = -1;

	public override void _Ready()
	{
		Instance = this;
	}
}