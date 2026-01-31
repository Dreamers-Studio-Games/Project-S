using UnityEngine;

public class finishLevel : MonoBehaviour
{
    private void OnTriggerEnter3D(BoxCollider collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneController.instance.NextLevel();
        }
    }
}
