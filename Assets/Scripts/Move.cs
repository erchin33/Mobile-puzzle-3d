
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 1f;
    public float moveDuration = 0.5f;

    [Header("Animation Settings")]
    public Ease playerEase = Ease.Linear;
    public Ease movableEase = Ease.Linear;

    [Header("Layer Settings")]
    public LayerMask groundLayer;
    public LayerMask movableLayer;

    [Header("Camera Shake Settings")]
    public Camera mainCamera;
    public float shakeDuration = 0.3f;
    public float shakeStrength = 0.2f;

    private bool canMove = true;
    private Vector2 initialTouchPosition;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleSwipeInput();
    }

    private void HandleSwipeInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                initialTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 swipeDelta = touch.position - initialTouchPosition;
                if (canMove)
                {
                    HandleSwipeDirection(swipeDelta);
                }
            }
        }
    }

    private void HandleSwipeDirection(Vector2 swipeDelta)
    {
        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            if (swipeDelta.x > 0)
            {
                TryMove(Vector3.forward); // Sað kaydýrma -> Ýleri hareket
            }
            else
            {
                TryMove(Vector3.back); // Sol kaydýrma -> Geri hareket
            }
        }
        else
        {
            if (swipeDelta.y > 0)
            {
                TryMove(Vector3.left); // Yukarý kaydýrma -> Sol hareket
            }
            else
            {
                TryMove(Vector3.right); // Aþaðý kaydýrma -> Sað hareket
            }
        }
    }

    private void TryMove(Vector3 direction)
    {
        Vector3 targetPosition = transform.position + direction * moveDistance;

        if (!IsBlocked(transform.position, direction))
        {
            Collider[] hitMovables = Physics.OverlapSphere(targetPosition, 0.1f, movableLayer);

            if (hitMovables.Length > 0)
            {
                MoveMovable(hitMovables[0], direction);
            }
            else
            {
                MovePlayer(targetPosition);
                ActivateRigidbodyIfNeeded(targetPosition);
            }
        }
        else
        {
            // Hareket edilemediði durumda kamera shake efektini çaðýr
            if (mainCamera != null)
            {
                mainCamera.transform.DOShakePosition(shakeDuration, shakeStrength);
            }
        }
    }

    private void MoveMovable(Collider movable, Vector3 direction)
    {
        Vector3 movableTargetPosition = movable.transform.position + direction * moveDistance;

        if (!IsBlocked(movable.transform.position, direction))
        {
            movable.transform.DOMove(movableTargetPosition, moveDuration)
                .SetEase(movableEase)
                .OnComplete(() =>
                {
                    canMove = true;
                    ActivateRigidbodyIfNeeded(movableTargetPosition);
                });

            MovePlayer(movable.transform.position);
            ActivateRigidbodyIfNeeded(movable.transform.position);
        }
        else
        {
            // Hareket edilemediði durumda kamera shake efektini çaðýr
            if (mainCamera != null)
            {
                mainCamera.transform.DOShakePosition(shakeDuration, shakeStrength);
            }
        }
    }

    private void MovePlayer(Vector3 targetPosition)
    {
        transform.DOMove(targetPosition, moveDuration)
            .SetEase(playerEase)
            .OnComplete(() => canMove = true);
        canMove = false;
    }

    private void ActivateRigidbodyIfNeeded(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.1f, groundLayer);
        if (colliders.Length == 0)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }

    private bool IsBlocked(Vector3 currentPosition, Vector3 direction)
    {
        Vector3 checkPosition = currentPosition + direction * moveDistance;
        Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.1f, groundLayer);
        return colliders.Length > 0;
    }
}