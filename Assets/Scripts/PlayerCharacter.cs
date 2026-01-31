using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacter : MonoBehaviour
{
  public Vector3 Velocity { get; private set; }
  [SerializeField] InputActionAsset actions;
  [SerializeField] float speed;
  [SerializeField] VisualEffect beamVFX;
  CharacterController controller;
  Animator animator;
  InputAction moveAction;
  InputAction lookAction;
  InputAction lookCursorAction;
  InputAction attackAction;
  InputAction interactAction;
  Vector2 look;
  bool attack;
  bool interact;
  bool isStunned;
  float lastAttack;
  float lastLook;

  void Awake()
  {
    var map = actions.FindActionMap("Player");
    moveAction = map.FindAction("Move");
    lookAction = map.FindAction("Look");
    lookCursorAction = map.FindAction("LookCursor");
    attackAction = map.FindAction("Attack");
    interactAction = map.FindAction("Interact");
  }

  void Start()
  {
    controller = GetComponent<CharacterController>();
    animator = GetComponent<Animator>();
    attack = false;
    interact = false;
    look = Vector2.up;
    lastAttack = -100f;
    isStunned = false;
  }

  void Update()
  {
    var timeSinceLastAttack = Time.time - lastAttack;
    isStunned = timeSinceLastAttack > 1f && timeSinceLastAttack < 1.5f;
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
    var flags = controller.Move(Time.deltaTime * Velocity);
    var isMovingDiagonal = move.x != 0 && move.y != 0;
    animator.SetFloat("Velocity", !isMovingDiagonal && flags == CollisionFlags.CollidedSides ? 0f : move.magnitude);
    if (timeSinceLastAttack > 1.5f && attack)
    {
      lastAttack = Time.time;
      beamVFX.Play();
    }
  }
}