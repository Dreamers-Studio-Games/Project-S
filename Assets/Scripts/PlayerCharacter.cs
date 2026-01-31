using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacter : MonoBehaviour, IAttributes
{
  float _health; public float Health
  {
    get => _health;
    set => hud.SetHealth(_health = Mathf.Clamp(value, 0f, 100f));
  }
  public Vector3 Velocity { get; private set; }
  [SerializeField] InputActionAsset actions;
  [SerializeField] float speed;
  [SerializeField] float clawSpeed;
  [SerializeField] VisualEffect beamVFX;
  [SerializeField] Vector3 eulers;
  [SerializeField] bool inFront;
  HUDController hud;
  VisualEffect clawVFX;
  NavMeshAgent agent;
  Animator animator;
  InputAction moveAction;
  InputAction lookAction;
  InputAction lookCursorAction;
  InputAction attackAction;
  InputAction interactAction;
  InputAction previousAction;
  InputAction nextAction;
  Vector2 look;
  bool attack;
  bool interact;
  bool previous;
  bool next;
  bool isStunned;
  float lastAttack;
  float lastLook;
  enum Mask
  {
    Beam,
    Claw
  }
  Mask currentMask;

  void Awake()
  {
    var map = actions.FindActionMap("Player");
    moveAction = map.FindAction("Move");
    lookAction = map.FindAction("Look");
    lookCursorAction = map.FindAction("LookCursor");
    attackAction = map.FindAction("Attack");
    interactAction = map.FindAction("Interact");
    previousAction = map.FindAction("Previous");
    nextAction = map.FindAction("Next");
  }

  void Start()
  {
    hud = GameObject.Find("HUD").GetComponent<HUDController>();
    clawVFX = GameObject.Find("Claw_VFX").GetComponent<VisualEffect>();
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponent<Animator>();
    attack = false;
    interact = false;
    look = Vector2.up;
    lastAttack = -100f;
    isStunned = false;
    currentMask = Mask.Beam;
    Health = 100f;
  }

  void Update()
  {
    previous = !previous && previousAction.IsPressed();
    next = !next && nextAction.IsPressed();
    if (previous || next)
    {
      currentMask = currentMask == Mask.Beam ? currentMask = Mask.Claw : Mask.Beam;
      Debug.Log(currentMask);
    }
    var timeSinceLastAttack = Time.time - lastAttack;
    if (isStunned)
    {
      animator.SetFloat("Velocity", 0f);
      return;
    };
    var move = moveAction.ReadValue<Vector2>();
    if (lookAction.ReadValue<Vector2>().sqrMagnitude > 0.01f)
    {
      look = lookAction.ReadValue<Vector2>().normalized;
      lastLook = Time.time;
    }
    if (Mouse.current.delta.ReadValue() != Vector2.zero)
    {
      look = (lookCursorAction.ReadValue<Vector2>() - 0.5f * new Vector2(Screen.width, Screen.height)).normalized;
      lastLook = Time.time;
    }
    if (Time.time - lastLook < 0.1f)
    {
      transform.rotation = Quaternion.LookRotation(new Vector3(look.x, 0f, look.y));
      if (move.sqrMagnitude > 0.01f)
      {
        var moveDir = Quaternion.Inverse(transform.rotation) * Quaternion.LookRotation(new Vector3(move.x, 0f, move.y)) * Vector3.forward;
        animator.SetFloat("DirX", moveDir.x);
        animator.SetFloat("DirY", moveDir.z);
      }
      else
      {
        animator.SetFloat("DirX", 0f);
        animator.SetFloat("DirY", 1f);
      }
    }
    else if (move.sqrMagnitude > 0.01f)
    {
      transform.rotation = Quaternion.LookRotation(new Vector3(move.x, 0f, move.y));
      animator.SetFloat("DirX", 0f);
      animator.SetFloat("DirY", 1f);
    }
    Debug.DrawLine(transform.position, transform.position + new Vector3(look.x, 0f, look.y));
    beamVFX.transform.parent.rotation = Quaternion.LookRotation(new Vector3(look.x, 0f, look.y));
    attack = !attack && attackAction.IsPressed();
    interact = !interact && interactAction.IsPressed();
    Velocity = speed * new Vector3(move.x, 0f, move.y);
    agent.Move(Time.deltaTime * Velocity);
    var isMovingDiagonal = move.x != 0 && move.y != 0;
    animator.SetFloat("Velocity", move.magnitude); // !isMovingDiagonal && flags == CollisionFlags.CollidedSides ? 0f : 
    if (currentMask == Mask.Beam && timeSinceLastAttack > 1.5f && attack)
    {
      lastAttack = Time.time;
      StartCoroutine(BeamAttackCoroutine());
    }
    if (currentMask == Mask.Claw && timeSinceLastAttack > 1f && attack)
    {
      lastAttack = Time.time;
      StartCoroutine(ClawAttackCoroutine());
    }
  }

  IEnumerator BeamAttackCoroutine()
  {
    beamVFX.Play();
    yield return new WaitForSeconds(1f);
    isStunned = true;
    yield return new WaitForSeconds(0.1f);
    if (Physics.SphereCast(transform.position, 0.25f, transform.forward, out var hit, 10f, LayerMask.GetMask("Enemy")))
    {
      Debug.Log("Hit");
      var attributes = hit.collider.GetComponent<IAttributes>();
      attributes.Health -= 35f;
    }
    yield return new WaitForSeconds(0.4f);
    isStunned = false;
  }

  IEnumerator ClawAttackCoroutine()
  {
    isStunned = true;
    animator.SetTrigger("ClawAttack");
    var dir = transform.forward;
    yield return new WaitForSeconds(0.1f);
    if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out var hit, 5f, LayerMask.GetMask("Enemy")))
    {
      Debug.Log("Hit");
      var otherAgent = hit.collider.GetComponent<NavMeshAgent>();
      otherAgent.Move(1.25f * (hit.transform.position - transform.position).normalized);
      var attributes = hit.collider.GetComponent<IAttributes>();
      attributes.Health -= 35f;
    }
    clawVFX.transform.SetPositionAndRotation(
      transform.position + 0.1f * clawSpeed * dir - 0.2f * Vector3.up,
      Quaternion.LookRotation(dir) * Quaternion.Euler(-30f, -90f, 90f)
    );
    clawVFX.Play();
    var start = Time.time;
    while (Time.time - start < 0.2f)
    {
      agent.Move(clawSpeed * Time.deltaTime * dir);
      yield return null;
    }
    yield return new WaitForSeconds(0.45f);
    isStunned = false;
  }
}