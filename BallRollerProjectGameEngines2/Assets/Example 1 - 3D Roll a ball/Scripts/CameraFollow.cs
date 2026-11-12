using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    Vector3 offset;
    public enum FollowMode { SmoothDamp, Lerp }
    [SerializeField] FollowMode followMode;
    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float collisionRadius = 0.2f;
    [SerializeField] float collisionPadding = 0.1f;
    [SerializeField] LayerMask cameraObstacleMask;

    [Header("SmoothDamp Values")]
    public float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;

    [Header("Lerp Values")]
    public float lerpSpeed;
    public float yaw;
    public float pitch;
    public float distance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Determine offset based on initial positions
        offset = transform.position - target.position;
        distance = offset.magnitude;
        yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        pitch = -Mathf.Asin(offset.y / distance) * Mathf.Rad2Deg;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;

        Vector3 desiredPosition = target.position + Quaternion.Euler(pitch, yaw, 0f) * Vector3.forward * distance;

        if (followMode == FollowMode.SmoothDamp)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref velocity, smoothTime
            );
        }
        else if (followMode == FollowMode.Lerp)
        {
            transform.position = Vector3.Lerp(
                transform.position, desiredPosition, lerpSpeed * Time.deltaTime
            );
        }
        Vector3 cameraOffset = transform.position - target.position;
        float cameraDistance = cameraOffset.magnitude;

        if (Physics.SphereCast(
                target.position,
                collisionRadius,
                cameraOffset.normalized,
                out RaycastHit hit,
                cameraDistance,
                cameraObstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            transform.position = target.position
                + cameraOffset.normalized
                * Mathf.Max(0.1f, hit.distance - collisionPadding);
        }
        transform.LookAt(target.position);
    }
}