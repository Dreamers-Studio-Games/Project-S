using UnityEngine;

public class CameraController : MonoBehaviour
{
  [SerializeField] PlayerCharacter target;
  [SerializeField] Vector3 offset;
  [SerializeField] float speed;
  [SerializeField] float lookAhead;

  void OnValidate()
  {
    if (!target) return;
    transform.position = target.transform.position + offset;
    transform.LookAt(target.transform);
  }

  void Update()
  {
    var futureTargetPosition = target.transform.position + lookAhead * target.Velocity;
    transform.position = Vector3.Lerp(transform.position, futureTargetPosition + offset, speed * Time.deltaTime);
  }
}