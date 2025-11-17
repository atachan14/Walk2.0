using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Helpers : MonoBehaviour
{
    public static Helpers Instance;
    private void Awake()
    {
        Instance = this;
    }
   

}
