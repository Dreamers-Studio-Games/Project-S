using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
  [SerializeField] VisualEffect beamVFX;
  [SerializeField] float attackRadius;
  Transform target;
  NavMeshAgent agent;
  Animator animator;

  void Start()
  {
    target = GameObject.Find("PlayerCharacter").transform;
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponent<Animator>();
    StartCoroutine(Pursue());
  }

  IEnumerator Pursue()
  {
    Debug.Log("Pursue");
    agent.isStopped = false;
    animator.SetFloat("Velocity", 1f);
    while (Vector3.Distance(transform.position, target.position) > attackRadius)
    {
      transform.LookAt(target);
      if (agent.velocity.sqrMagnitude > 0.01f)
      {
        var move = agent.velocity.normalized;
        var moveDir = Quaternion.Inverse(transform.rotation) *
          Quaternion.LookRotation(new Vector3(move.x, 0f, move.z)) * Vector3.forward;
        animator.SetFloat("DirX", moveDir.x);
        animator.SetFloat("DirY", moveDir.z);
      }
      agent.destination = target.position;
      yield return null;
    }
    StartCoroutine(Attack());
  }

  IEnumerator Attack()
  {
    Debug.Log("Attack");
    agent.isStopped = true;
    animator.SetFloat("Velocity", 0f);
    beamVFX.Play();
    var start = Time.time;
    while (Time.time - start < 0.75f)
    {
      var targetRotation = Quaternion.LookRotation(target.position - transform.position);
      transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, agent.angularSpeed * Time.deltaTime);
      yield return null;
    }
    yield return new WaitForSeconds(1f);
    StartCoroutine(Pursue());
  }
}