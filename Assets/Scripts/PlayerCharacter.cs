using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacter : MonoBehaviour
{
  public Vector3 Velocity { get; private set; }
  [SerializeField] InputActionAsset actions;
  [SerializeField] float speed;
  CharacterController controller;
  Animator animator;
  InputAction moveAction;
  InputAction lookAction;
  InputAction attackAction;
  InputAction interactAction;
  bool attack;
  bool interact;

  void Awake()
  {
    var map = actions.FindActionMap("Player");
    moveAction = map.FindAction("Move");
    lookAction = map.FindAction("LookCursor");
    attackAction = map.FindAction("Attack");
    interactAction = map.FindAction("Interact");
  }

  void Start()
  {
    controller = GetComponent<CharacterController>();
    animator = GetComponent<Animator>();
    attack = false;
    interact = false;
  }

  void Update()
  {
    var move = moveAction.ReadValue<Vector2>();
    var look = (lookAction.ReadValue<Vector2>() - 0.5f * new Vector2(Screen.width, Screen.height)).normalized;
    Debug.DrawLine(transform.position, transform.position + new Vector3(look.x, 0f, look.y));
    attack = !attack && attackAction.IsPressed();
    interact = !interact && interactAction.IsPressed();
    Velocity = speed * new Vector3(move.x, 0f, move.y);
    var flags = controller.Move(Time.deltaTime * Velocity);
    transform.LookAt(transform.position + new Vector3(move.x, 0f, move.y));
    var isMovingDiagonal = move.x != 0 && move.y != 0;
    animator.SetFloat("Velocity", !isMovingDiagonal && flags == CollisionFlags.CollidedSides ? 0f : move.magnitude);
  }
}