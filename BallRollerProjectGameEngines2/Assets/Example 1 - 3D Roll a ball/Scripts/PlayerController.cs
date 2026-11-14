using UnityEngine;
using System.Collections.Generic;

// Physics-based ball controller: WASD/arrow input is turned into a push
// (Rigidbody.AddForce) relative to the camera's facing direction, so
// "forward" always means "away from the camera" no matter how it's rotated.
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform raycastStartPoint;
    [SerializeField] GameManager gameManager;

    [Header("Settings")]
    [SerializeField] float moveForce = 10f;
    [SerializeField] float jumpForce = 1f;
    [SerializeField] float jumpVelocity = 10f;
    [SerializeField] float groundCheckDistance = 0.7f;
    [SerializeField] bool invertControls = false;
    [SerializeField] float wallJumpHorizontalSpeed = 10f;
    [SerializeField] float wallJumpVerticalSpeed = 10f;
    bool wallJumpRequested = false;
    bool wallJumped = false;
    bool touchingWall = false;
    Vector3 wallNormal;

    Vector3 moveInput;

    readonly Dictionary<Collider, Vector3> wallContacts = new Dictionary<Collider, Vector3>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            wallJumpRequested = true;
        }
        // GetAxisRaw returns -1, 0, or 1 with no smoothing - contrary to Input.GetAxis
        Vector3 inputInCameraSpace = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical"));

        if (invertControls)
        {
            inputInCameraSpace *= -1f;
        }
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        // TransformVector transforms the local direction into world space using
        // the camera's current rotation.
        moveInput = Vector3.ClampMagnitude(right * inputInCameraSpace.x + forward * inputInCameraSpace.z, 1f);

        // Remove the y component, we only want to move the ball horizontally, nomralize afterwards.
        moveInput.y = 0;
        moveInput.Normalize();

    }

    void FixedUpdate()
    {
        // Continous physics forces belong in FixedUpdate, since it runs on physics's own
        // fixed timestep instead of once per rendered frame like Update.
        if (wallJumpRequested) {
            if(wallContacts.Count > 0 && !wallJumped)
            {
                Vector3 normals = Vector3.zero;
                foreach(Vector3 normal in wallContacts.Values)
                {
                    normals += normal;
                }
                Vector3 awayFromWall = Vector3.ProjectOnPlane(normals, Vector3.up).normalized;
                rb.linearVelocity = awayFromWall * wallJumpHorizontalSpeed + Vector3.up * wallJumpVerticalSpeed;
                wallJumped = true;
                wallJumpRequested = false;
            }
            wallJumpRequested = false;
        }
        rb.AddForce(moveInput.normalized * moveForce);
        
    }

    // The grounded check here is a short downward raycast: if it hits anything, the ball is touching ground.
    bool IsGrounded()
    {
        return Physics.Raycast(raycastStartPoint.position, Vector3.down, groundCheckDistance);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<KillZone>() != null)
        {
            gameManager.OnPlayerEnteredKillzone();
        }
        UpdateWallContact(collision);

        if (rb.linearVelocity.y > 0){
            return;
        }
        for(int i = 0; i < collision.contactCount; i++){
            // 0.5F is to prevent slopes past a 60° angle to be counted as surfaces for jumping
            if(collision.GetContact(i).normal.y > 0.5f)
            {   
                wallJumped = false;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
                break;
            }
            else if (collision.GetContact(i).normal.y <= 0.1f)
            {
                wallNormal = collision.GetContact(i).normal;
                touchingWall = true;
                break;
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        UpdateWallContact(collision);

        if (rb.linearVelocity.y > 0){
            return;
        }

        for(int i = 0; i < collision.contactCount; i++){
            // 0.5F is to prevent slopes past a 60° angle to be counted as surfaces for jumping
            if(collision.GetContact(i).normal.y > 0.5f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
                break;
            }
            else if (collision.GetContact(i).normal.y <= 0.5f)
            {
                wallNormal = collision.GetContact(i).normal;
                touchingWall = true;
                break;
            }
        }
    }

    // Called by GameManager to move the ball back to the start point - used
    // both for the initial spawn and every respawn after death.
    public void OnRespawn(Vector3 startPoint)
    {
        rb.position = startPoint;
        // Without resetting these, the ball would keep its old momentum and
        // spin after being teleported back to the start.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        wallContacts.Clear();
        wallJumped = false;
        wallJumpRequested = false;
    }

    private void UpdateWallContact(Collision collision)
    {
        Vector3 wallNormal = Vector3.zero;

        for(int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            Vector3 normal =contact.normal;
            Vector3 towardsBall = rb.worldCenterOfMass - contact.point;

            if (Vector3.Dot(normal, towardsBall) < 0f)
            {
                normal = -normal;
            }

            // Only near-vertical contacts count as walls
            if (Mathf.Abs(normal.y) <= 0.1f)
            {
                wallNormal = normal;
                break;
            }
        }
        if (wallNormal != Vector3.zero)
        {
            wallContacts[collision.collider] = wallNormal;
        }
        else
        {
            wallContacts.Remove(collision.collider);
        }
    }
    void OnCollisionExit(Collision collision)
    {
        wallContacts.Remove(collision.collider);
    }
}
