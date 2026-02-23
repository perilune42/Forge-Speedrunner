using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Passage : MonoBehaviour
{
    public Doorway door1;
    public Doorway door2;
    [HideInInspector] public bool visited;
}
