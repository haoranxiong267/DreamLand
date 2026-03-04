using UnityEngine;

public class CameraFixedChild : MonoBehaviour
{
    [Header("Fixed Rotation Settings")]
    [Tooltip("Camera's fixed local rotation relative to the player (Euler angles)")]
    public Vector3 fixedLocalRotation = new Vector3(90f, 0f, 0f);

    [Header("Position Offset")]
    [Tooltip("Camera's fixed local position offset relative to the player")]
    public Vector3 localPositionOffset = new Vector3(0f, 12f, 0f);

    [Tooltip("Position follow smoothing speed, higher value = faster follow")]
    public float positionSmoothTime = 0f;
    private Vector3 positionSmoothVelocity = Vector3.zero;

    // Cache Transform for performance
    private Transform myTransform;
    private Transform parentTransform;

    void Start()
    {
        // Get component references
        myTransform = this.transform;
        parentTransform = myTransform.parent;

        if (parentTransform == null)
        {
            Debug.LogError("CameraFixedChild: This script must be attached to a GameObject with a parent (e.g., a Camera as a child of Player).");
            this.enabled = false; // Disable script
            return;
        }

        // Initialize: Immediately apply fixed position and rotation
        ApplyFixedTransform();
    }

    void LateUpdate()
    {
        // Execute in LateUpdate to ensure it's applied after player movement and rotation
        ApplyFixedTransform();
    }

    void ApplyFixedTransform()
    {
        // 调试：确认方法被调用
        Debug.Log($"CameraFixedChild: 正在应用固定变换。当前本地旋转: {transform.localRotation.eulerAngles}");
        
        // 1. Calculate target local position (in parent's space)
        Vector3 targetLocalPosition = localPositionOffset;

        // 2. Smoothly update local position (optional, for reducing jitter)
        myTransform.localPosition = Vector3.SmoothDamp(
            myTransform.localPosition,
            targetLocalPosition,
            ref positionSmoothVelocity,
            positionSmoothTime
        );

        // 3. Force set local rotation to fixed value
        // Use Quaternion.Euler to convert Vector3 to quaternion
        myTransform.localRotation = Quaternion.Euler(fixedLocalRotation);
    }

    // Draw helper lines in editor Scene view for easier parameter adjustment
    void OnDrawGizmosSelected()
    {
        if (transform.parent != null)
        {
            Gizmos.color = Color.cyan;
            // Draw target position in world coordinates
            Vector3 worldTargetPos = transform.parent.TransformPoint(localPositionOffset);
            Gizmos.DrawWireSphere(worldTargetPos, 0.5f);
            Gizmos.DrawLine(transform.parent.position, worldTargetPos);
        }
    }
}