using UnityEngine;

public class EntityState
{
    protected Player player;
    protected StateMachine stateMachine; // Protected makes the stateMachine private to EntityState and its subclasses
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody2D rb;
    protected PlayerInputSet input;

    protected float stateTimer; // Used to track how long we've been in the state, or how long we have left in the state
    protected bool triggerCalled;


    // called once to set up initial variables
    public EntityState(Player player, StateMachine stateMachine, string animBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;

        anim = player.anim;
        rb = player.rb;
        input = player.input;
    }


    // call everytime we enter a new state
    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
    }


    // run logic of the state here
    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }


    public virtual void Exit()
    {
        player.anim.SetBool(animBoolName, false);
    }


    public void CallAnimationTrigger()
    {
        triggerCalled = true;
    }
    
}
