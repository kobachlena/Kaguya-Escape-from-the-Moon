using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : MonoBehaviour, IRestartable
{
    [Header("PlayerProperties")] private Vector3 movement;
    [SerializeField] GameObject playerModel;
    [SerializeField] GameObject crouchModel;
    private bool isGrounded = true;
    private bool isCrouching = false;

    private float xMovement;
    private float yMovement;
    private float zMovement;
    private CharacterController playerController;

    [Header("Speed")] [Min(0f)] [SerializeField]
    private float startSpeed = 10f;

    [Min(0f)] [SerializeField] private float acceleration = 2f;
    [Min(0f)] [SerializeField] private float maxSpeed = 25f;
    private float currentSpeed;

    [Header("Jump")] [SerializeField] private float gravityForce = 55f;
    [SerializeField] private float jumpForce = 20f;
    private float verticalVelocity = 0f;

    [Header("Ground")] [SerializeField] private Transform groundChecker;
    [SerializeField] private float groundCheckerRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;
    private bool wasGrounded = true;

    [Header("Lane")] [SerializeField] private float laneSwitchSpeed = 5f;
    [SerializeField] private float laneSwitchDistance = 4f;
    private int currentLane = 0;
    private bool inputPressed = false;

    [Header("Buttons")] 
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference touchAction;

    [Header("Swipe")] [Range(0.02f, 0.3f)] [SerializeField]
    private float minSwipeScreenPercent = 0.08f;

    private Vector2 startTouchPosition;
    private bool pointerHeld;

    [Header("StartPoint")] [SerializeField]
    private Transform playerStartPoint;

    [Header("Audio")] 
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip crouchClip;



    private void OnEnable()
    {
        if (crouchAction != null)
            crouchAction.action.Enable();

        if (touchAction != null)
            touchAction.action.Enable();
    }

    private void OnDisable()
    {
        if (crouchAction != null)
            crouchAction.action.Disable();

        if (touchAction != null)
            touchAction.action.Disable();

        ResetInputState();
    }

    private void ResetInputState()
    {
        pointerHeld = false;
        startTouchPosition = Vector2.zero;
    }

    void Awake()
    {
        playerController = GetComponent<CharacterController>();
    }

    void Start()
    {
        currentSpeed = startSpeed;
        playerController.height = 2f;
        playerController.center = new Vector3(0f, 0f, 0f);
        playerModel.SetActive(true);
        crouchModel.SetActive(false);
        wasGrounded = true;
    }

    void Update()
    {
        isGrounded = IsGrounded();
        Acceleration();
        ReadSwipe();
        SwitchLanes();
        Gravity();
        Crouch();
        MoveForward();
        CheckLanding();
    }

    private void Acceleration()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
    }

    private void MoveForward()
    {
        yMovement = verticalVelocity * Time.deltaTime;
        zMovement = currentSpeed * Time.deltaTime;
        movement = new Vector3(xMovement, yMovement, zMovement);
        playerController.Move(movement);
    }

    private void SwitchLanes()
    {
        float targetX = currentLane * laneSwitchDistance;

        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneSwitchSpeed * Time.deltaTime);
        xMovement = newX - transform.position.x;
    }

    private void ReadSwipe()
    {
        Vector2 currentTouchPosition = touchAction.action.ReadValue<Vector2>();

        if (crouchAction.action.WasPressedThisFrame())
        {
            startTouchPosition = touchAction.action.ReadValue<Vector2>();
            return;
        }

        if (!crouchAction.action.IsPressed())
            return;

        Vector2 delta = currentTouchPosition - startTouchPosition;
        float minSwipeDistance = Screen.width * minSwipeScreenPercent;

        if (Mathf.Abs(delta.x) <= minSwipeDistance)
            return;

        if (delta.x > 0)
            ChangeLane(1);
        else
            ChangeLane(-1);

        startTouchPosition = currentTouchPosition;
    }

    private void ChangeLane(int direcction)
    {
        currentLane = Mathf.Clamp(currentLane + direcction, -1, 1);
    }

    private void Gravity()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = 0f;
            return;
        }

        verticalVelocity -= gravityForce * Time.deltaTime;
    }

    private void Jump()
    {
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
            AudioManager.Instance.PlayEffect(jumpClip);
        }

    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundChecker.position, groundCheckerRadius, groundLayer,
            QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmos()
    {
        if (groundChecker == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundChecker.position, groundCheckerRadius);
    }

    private void Crouch()
    {
        bool pressedNow = crouchAction.action.IsPressed();

        if (pressedNow && !pointerHeld)
        {
            pointerHeld = true;
            AudioManager.Instance.PlayEffect(crouchClip);
        }

        if (pressedNow)
        {
            isCrouching = true;

            playerController.height = 1f;
            playerController.center = new Vector3(0f, -0.5f, 0f);

            playerModel.SetActive(false);
            crouchModel.SetActive(true);

            verticalVelocity = -30f;
        }
        else
        {
            isCrouching = false;

            playerController.height = 2f;
            playerController.center = Vector3.zero;

            playerModel.SetActive(true);
            crouchModel.SetActive(false);
        }

        if (pointerHeld &&
            !pressedNow &&
            crouchAction.action.WasReleasedThisFrame())
        {
            pointerHeld = false;
            Jump();
        }
    }

    private void CheckLanding()
    {
        wasGrounded = isGrounded;
    }

    public void GameOver()
    {
        crouchModel.SetActive(false);
        playerModel.SetActive(false);
    }
    
    public void Restart()
    {
        gameObject.SetActive(true);
        playerController.enabled = false;
        transform.SetPositionAndRotation(
            playerStartPoint.position,
            playerStartPoint.rotation
        );
        playerController.enabled = true;

        currentSpeed = startSpeed;
        currentLane = 0;
        verticalVelocity = 0f;
        playerController.height = 2f;
        playerController.center = new Vector3(0f, 0f, 0f);
        playerModel.SetActive(true);
        crouchModel.SetActive(false);
        wasGrounded = true;

        xMovement = 0f;
        yMovement = 0f;
        zMovement = 0f;

        isGrounded = true;
        isCrouching = false;
        pointerHeld = false;
        startTouchPosition = Vector2.zero;
    }

    public int GetDistance()
    {
        return (int)(transform.position.z - playerStartPoint.position.z);
    }
}

   
