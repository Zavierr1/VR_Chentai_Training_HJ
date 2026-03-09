using UnityEngine;

public class TabletDestroyer : MonoBehaviour
{
  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Tablet"))
    {
      Destroy(other.gameObject);
    }
  }
}
