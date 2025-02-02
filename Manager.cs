using System;
using System.Linq;
using System.Net.Sockets;
using Godot;
using Redirect3DS;

internal partial class Manager : Control
{
	[Export] public TextEdit IPAddress;
	[Export] public Button SendPackets;

	[Export] public RichTextLabel GamepadTitleLabel;
	[Export] public VBoxContainer GamepadList;
	[Export] public PackedScene GamepadLabelScene;
	
	private UdpClient udpClient;
	private readonly GamepadData gamepadData = new();
	
	private Timer packetTimer = new();
	
	private bool sending;
	
	public override void _Ready()
	{
		udpClient = new UdpClient();
		
		AddChild(packetTimer);
		
		packetTimer.WaitTime = 1.0 / GlobalVars.Instance.PacketsPerSecond;
		packetTimer.Timeout += OnTimeout;
		packetTimer.Start();

		SendPackets.Pressed += () =>
		{
			sending = !sending;
			GetViewport().GuiReleaseFocus();
			
			IPAddress.Editable = !sending;
		};
		
		Input.JoyConnectionChanged += (_, _) => RefreshGamepadList();
		RefreshGamepadList();
	}

	public override void _Process(double delta)
	{
		gamepadData.UpdateGamepadState();
		SendPackets.Text = (sending ? "Stop" : "Start") + " sending data";
	}

	private void OnTimeout()
	{
		if (!sending) return;
		
		var currentFrameData = gamepadData.PrepareFrameData();
		SendFrame(currentFrameData);
	}

	private void SendFrame(byte[] frameData)
	{
		udpClient.Send(frameData, frameData.Length, IPAddress.Text, 4950);
	}
	
	#region GamepadList
	private void RefreshGamepadList()
	{
		var connected = Input.GetConnectedJoypads();
		GamepadTitleLabel.Text = string.Format(GlobalVars.Instance.GamepadTitleFormat, connected.Count);
		
		foreach (var child in GamepadList.GetChildren()) { child.QueueFree(); }
		foreach (var id in connected)
		{
			var label = InstantiateGamepadLabel();
			label.Init(Input.GetJoyName(id), id);
		}
		
		if(connected.Count != 0 && GlobalVars.Instance.CurrentController == -1) 
			GlobalVars.Instance.CurrentController = connected.First();
		if (connected.Count == 0)
		{
			sending = false;
			GlobalVars.Instance.CurrentController = -1;
		}
		
		SendPackets.Disabled = connected.Count == 0;
	}
	
	private ControllerListing InstantiateGamepadLabel()
	{
		var labelInstance = (ControllerListing)GamepadLabelScene.Instantiate();
		GamepadList.AddChild(labelInstance);
		
		return labelInstance;
	}
	#endregion
}
