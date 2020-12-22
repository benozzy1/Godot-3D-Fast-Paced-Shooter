using Godot;
using System;

public class PlayerStateMachine : Node {
    public Player player;
    public StateMachine stateMachine;

    [Export] private NodePath _playerPath;

    [Export] public bool enabled = true;

    public override void _Ready() {
        InitializeStates();
    }

    public IState groundState;
    public IState airState;
    public IState wallState;
    public IState grappleState;
    private void InitializeStates() {
        player = GetNode(_playerPath) as Player;

        groundState = new PlayerGroundState(player);
        airState = new PlayerAirState(player);
        wallState = new PlayerWallState(player);
        grappleState = new PlayerGrappleState(player);

        stateMachine = new StateMachine();
        stateMachine.ChangeState(airState);
    }

    public void ChangeState(IState newState) => stateMachine.ChangeState(newState);

    public void Update(float delta) {if (enabled) stateMachine.Update(delta);}

    public void HandleTransitions() {if (enabled) stateMachine.HandleTransitions();}
}