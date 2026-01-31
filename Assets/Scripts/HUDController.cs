using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
  VisualElement healthBar;

  void Start()
  {
    var document = GetComponent<UIDocument>();
    healthBar = document.rootVisualElement.Q<VisualElement>("Health");
  }

  public void SetHealth(float value)
  {
    healthBar.style.width = Length.Percent(value);
  }
}