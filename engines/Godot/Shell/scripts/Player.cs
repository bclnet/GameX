using Godot;
using System;

public partial class Player : RigidBody3D {
	const float MouseSensitivity = 0.001f;
	float TwistInput = 0f;
	float PitchInput = 0f;
	Node3D TwistPivot;
	Node3D PitchPivot;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		TwistPivot = GetNode<Node3D>("TwistPivot");
		PitchPivot = GetNode<Node3D>("TwistPivot/PitchPivot");
		Input.SetMouseMode(Input.MouseModeEnum.Captured);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		var input = Vector3.Zero;
		input.X = Input.GetAxis("move_left", "move_right");
		input.Z = Input.GetAxis("move_forward", "move_back");
		ApplyCentralForce(TwistPivot.Basis * input * 1200.0f * (float)delta);
		
		if (Input.IsActionJustPressed("ui_cancel")) { Input.SetMouseMode(Input.MouseModeEnum.Visible); }
		
		TwistPivot.RotateY(TwistInput);
		PitchPivot.RotateX(PitchInput);
		var _ = PitchPivot.Rotation; _.X = Mathf.Clamp(_.X, Mathf.DegToRad(-30), Mathf.DegToRad(30)); PitchPivot.Rotation = _;
		TwistInput = PitchInput = 0f;
	}
	
	public override void _UnhandledInput(InputEvent ev) {
		if (ev is InputEventMouseMotion m) {
			if (Input.GetMouseMode() == Input.MouseModeEnum.Captured) {
				TwistInput = -m.Relative.X * MouseSensitivity;
				PitchInput = -m.Relative.Y * MouseSensitivity;
			}
		}
	}
}
