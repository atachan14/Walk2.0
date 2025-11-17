using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool tapAnywhere = false;
    public float pinchDelta;
    private float prevDistance;
    private bool isPinching = false;



    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        TapAnywhereCheck();
        PinchCheck();
    }

    void TapAnywhereCheck()
    {
        tapAnywhere = false;
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            tapAnywhere = true;
        }

        // エディター/PC
        if (Input.GetMouseButtonUp(0))
        {
            tapAnywhere = true;
        }
    }

    void PinchCheck()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float curDist = Vector2.Distance(t0.position, t1.position);

            // ピンチ開始（どちらかの指が新しく触れたら）
            if (!isPinching || t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                prevDistance = curDist;
                pinchDelta = 0;
                isPinching = true;
            }
            else
            {
                pinchDelta = curDist - prevDistance;
                prevDistance = curDist;
            }
        }
        else
        {
            // ピンチ解除時リセット
            if (isPinching)
            {
                isPinching = false;
                pinchDelta = 0;
            }
        }
    }
}
