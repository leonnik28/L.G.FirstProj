using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class HexMapCamera : MonoBehaviour
{
    Transform swivel, stick, target;
    float zoom = 1f;
    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeedMinZoom;
    public float moveSpeedMaxZoom;
    public HexGrid grid;

    public float rotateAng;

    public float rotationSpeed;

    private Vector3 targetPos;
    static HexMapCamera instance;

    private void Start()
    {
        if(target == null)
        {
            target = this.transform;
        }

        targetPos = target.position;
    }

    void Update()
    {

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                AdjustPosition(touch);
            }
        }

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroLastPosition = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOneLastPosition = touchOne.position - touchOne.deltaPosition;

            float distanceTouch = (touchZeroLastPosition - touchOneLastPosition).magnitude;
            float currentDistanceTouch = (touchZero.position - touchOne.position).magnitude;

            float difference = currentDistanceTouch - distanceTouch;

            AdjustZoom(difference* 0.00024f);
        }
    }

    void AdjustPosition(Touch touch)
    {

            Vector3 movePosition = new Vector3(touch.deltaPosition.x * Mathf.Lerp(moveSpeedMaxZoom, moveSpeedMinZoom, zoom) * -1 * Time.deltaTime,
            transform.position.y,
            touch.deltaPosition.y * Mathf.Lerp(moveSpeedMaxZoom, moveSpeedMinZoom, zoom) * -1 * Time.deltaTime);

            float xMax = (grid.cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
            //movePosition.x = Mathf.Clamp(movePosition.x, 0f, xMax);
            float zMax = (grid.cellCountZ - 1) * (1.5f * HexMetrics.outerRadius);
            //movePosition.z = Mathf.Clamp(movePosition.z, 0f, zMax);
            movePosition = transform.rotation * movePosition;

            Vector3 newPosition = transform.position + movePosition;
            newPosition.x = Mathf.Clamp(newPosition.x, 0f, xMax);
            newPosition.z = Mathf.Clamp(newPosition.z, 0f, zMax);

            transform.position = newPosition;
        
    }

    void UpdatePosition(Vector3 position)
    {
        transform.position = position;
    }

    public static void ValidatePosition()
    {
        Vector3 position = Vector3.zero;
        instance.UpdatePosition(position);
    }

    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (grid.cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax = (grid.cellCountZ - 1) * (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }

    void Awake()
    {
        instance = this;
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    public static bool Locked
    {
        set
        {
            instance.enabled = !value;
        }
    }

}
