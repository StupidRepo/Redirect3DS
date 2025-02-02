using Godot;

namespace Redirect3DS;

public partial class ControllerListing : Control
{
	private int ControllerId { get; set; }
	private string ControllerName { get; set; }
	
	[Export] public Label ControllerLabel;
	[Export] public Button SwitchToButton;
	
	public void Init(string name, int id)
	{
		ControllerName = name;
		ControllerId = id;
		
		ControllerLabel.Text = ControllerName;
		SwitchToButton.Pressed += () => GlobalVars.Instance.CurrentController = ControllerId;
	}

	public override void _Process(double delta)
	{
		SwitchToButton.Disabled = GlobalVars.Instance.CurrentController == ControllerId;
	}
}