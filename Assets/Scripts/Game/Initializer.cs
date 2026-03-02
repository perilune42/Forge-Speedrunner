using UnityEngine;

// last in execution order
public class Initializer : MonoBehaviour
{
    private void Start()
    {
        Game.Instance.StartGame();
    }
}