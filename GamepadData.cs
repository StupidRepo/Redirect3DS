using System;
using System.Linq;
using Godot;

namespace Redirect3DS;

public class GamepadData
{
	const uint CIRCLEPAD_BIND = 0x5d0;
	const uint CSTICK_BIND = 0x7f;
	
	private bool ButtonA { get; set; }
	private bool ButtonB { get; set; }
	private bool ButtonX { get; set; }
	private bool ButtonY { get; set; }

	private bool ButtonSelect { get; set; }
	private bool ButtonStart { get; set; }

	private bool ButtonUp { get; set; }
	private bool ButtonDown { get; set; }
	private bool ButtonLeft { get; set; }
	private bool ButtonRight { get; set; }

	private bool ButtonLB { get; set; }
	private bool ButtonRB { get; set; }

	private bool ButtonGuide { get; set; }

	private Vector2 LeftStick { get; set; }
	private Vector2 RightStick { get; set; }

	public void UpdateGamepadState()
	{
		var controller = GlobalVars.Instance.CurrentController;
		if (controller == -1) return;
	
		ButtonA = Input.IsJoyButtonPressed(controller, JoyButton.A);
		ButtonB = Input.IsJoyButtonPressed(controller, JoyButton.B);
		ButtonX = Input.IsJoyButtonPressed(controller, JoyButton.X);
		ButtonY = Input.IsJoyButtonPressed(controller, JoyButton.Y);
	
		ButtonSelect = Input.IsJoyButtonPressed(controller, JoyButton.Back);
		ButtonStart = Input.IsJoyButtonPressed(controller, JoyButton.Start);
	
		ButtonUp = Input.IsJoyButtonPressed(controller, JoyButton.DpadUp);
		ButtonDown = Input.IsJoyButtonPressed(controller, JoyButton.DpadDown);
		ButtonLeft = Input.IsJoyButtonPressed(controller, JoyButton.DpadLeft);
		ButtonRight = Input.IsJoyButtonPressed(controller, JoyButton.DpadRight);
	
		ButtonLB = Input.IsJoyButtonPressed(controller, JoyButton.LeftShoulder);
		ButtonRB = Input.IsJoyButtonPressed(controller, JoyButton.RightShoulder);
	
		ButtonGuide = Input.IsJoyButtonPressed(controller, JoyButton.Guide);
	
		LeftStick = new Vector2(Input.GetJoyAxis(controller, JoyAxis.LeftX),
			-Input.GetJoyAxis(controller, JoyAxis.LeftY));
		RightStick = new Vector2(Input.GetJoyAxis(controller, JoyAxis.RightX),
			Input.GetJoyAxis(controller, JoyAxis.RightY));
	}

	private uint CalcButtonState()
	{
		uint buttonState = 0xfff;

		/* Original CPP code:
		 u32 hidPad = 0xfff;
		 hidPad &= ~(1 << 0); // A
		 hidPad &= ~(1 << 1); // B
		 hidPad &= ~(1 << 10); // X
		 hidPad &= ~(1 << 11); // Y

		 hidPad &= ~(1 << 2); // Select
		 hidPad &= ~(1 << 3); // Start

		 hidPad &= ~(1 << 4); // DPad Right
		 hidPad &= ~(1 << 5); // DPad Left
		 hidPad &= ~(1 << 6); // DPad Up
		 hidPad &= ~(1 << 7); // DPad Down

		 hidPad &= ~(1 << 8); // R
		 hidPad &= ~(1 << 9); // L
		 */

		if (ButtonA) buttonState &= ~(1u << 0);
		if (ButtonB) buttonState &= ~(1u << 1);
		if (ButtonX) buttonState &= ~(1u << 10);
		if (ButtonY) buttonState &= ~(1u << 11);

		if (ButtonSelect) buttonState &= ~(1u << 2);
		if (ButtonStart) buttonState &= ~(1u << 3);

		if (ButtonRight) buttonState &= ~(1u << 4);
		if (ButtonLeft) buttonState &= ~(1u << 5);
		if (ButtonUp) buttonState &= ~(1u << 6);
		if (ButtonDown) buttonState &= ~(1u << 7);

		if (ButtonRB) buttonState &= ~(1u << 8);
		if (ButtonLB) buttonState &= ~(1u << 9);

		return buttonState;
	}

	// TODO: Implement touch screen support
	private uint CalcTouchScreenState()
	{
		uint touchScreenState = 0x2000000;

		return touchScreenState;
	}

	private uint CalcCirclePadState()
	{
		uint circlePadState = 0x7ff7ff;

		/* Original CPP code:
		     if(lx != 0.0 || ly != 0.0)
		    {
		        u32 x = (u32)(lx * CPAD_BOUND + 0x800);
		        u32 y = (u32)(ly * CPAD_BOUND + 0x800);
		        x = x >= 0xfff ? (lx < 0.0 ? 0x000 : 0xfff) : x;
		        y = y >= 0xfff ? (ly < 0.0 ? 0x000 : 0xfff) : y;

		        circlePadState = (y << 12) | x;
		    }
		 */

		if (LeftStick is { X: 0.0f, Y: 0.0f }) return circlePadState;
		
		var x = (uint) (LeftStick.X * CIRCLEPAD_BIND + 0x800);
		var y = (uint) (LeftStick.Y * CIRCLEPAD_BIND + 0x800);
			
		x = x >= 0xfff ? (uint)(LeftStick.X < 0.0f ? 0x000 : 0xfff) : x;
		y = y >= 0xfff ? (uint)(LeftStick.Y < 0.0f ? 0x000 : 0xfff) : y;
		
		circlePadState = (y << 12) | x;
		return circlePadState;
	}
	
	private uint CalcCStickState()
	{
		var cStickState = 0x80800081;
		
		/* Original CPP code:
		u32 x = (u32)(M_SQRT1_2 * (rx + ry) * CPP_BOUND + 0x80);
        u32 y = (u32)(M_SQRT1_2 * (ry - rx) * CPP_BOUND + 0x80);
        x = x >= 0xff ? (rx < 0.0 ? 0x00 : 0xff) : x;
        y = y >= 0xff ? (ry < 0.0 ? 0x00 : 0xff) : y;

        cppState = (y << 24) | (x << 16) | (irButtonsState << 8) | 0x81;
		 */
		// rx and ry are the right stick's X and Y values
		// we don't care about the IR buttons, that can be just 0.
		
		if (RightStick is { X: 0.0f, Y: 0.0f }) return cStickState;
		
		var x = (uint) (Math.Sqrt(0.5) * (RightStick.X + RightStick.Y) * CSTICK_BIND + 0x80);
		var y = (uint) (Math.Sqrt(0.5) * (RightStick.Y - RightStick.X) * CSTICK_BIND + 0x80);
		
		x = x >= 0xff ? (uint)(RightStick.X < 0.0f ? 0x00 : 0xff) : x;
		y = y >= 0xff ? (uint)(RightStick.Y < 0.0f ? 0x00 : 0xff) : y;
		
		cStickState = (y << 24) | (x << 16) | 0x81;
		return cStickState;
	}
	
	private uint CalcInterfaceState()
	{
		uint interfaceButtons = 0;
		
		if (ButtonGuide) interfaceButtons |= 1;
		
		return interfaceButtons;
	}

	public byte[] PrepareFrameData()
	{
		var frameData = new byte[20];
		
		Array.Copy(BitConverter.GetBytes(CalcButtonState()), 0, frameData, 0, 4);
		Array.Copy(BitConverter.GetBytes(CalcTouchScreenState()), 0, frameData, 4, 4);
		Array.Copy(BitConverter.GetBytes(CalcCirclePadState()), 0, frameData, 8, 4);
		Array.Copy(BitConverter.GetBytes(CalcCStickState()), 0, frameData, 12, 4);
		Array.Copy(BitConverter.GetBytes(CalcInterfaceState()), 0, frameData, 16, 4);
		
		return frameData;
	}
}