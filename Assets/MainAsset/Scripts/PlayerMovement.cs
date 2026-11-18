using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IRestartable
{
    [Header("Player Skins")]
    [SerializeField] private PlayerVisuals visuals;
    [SerializeField, HideInInspector] private GameObject[] playerModel;
    [SerializeField, HideInInspector] private GameObject[] crouchModel;

    [Header("Speed")]
    [Min(0f)] [SerializeField] private float startSpeed = 10f;
    [Min(0f)] [SerializeField] private float acceleration = 2f;
    [Min(0f)] [SerializeField] private float maxSpeed = 25f;

    [Header("Jump")]
    [SerializeField] private float gravityForce = 55f;
    [SerializeField] private float jumpForce = 20f;
    [Min(0f)] [SerializeField] private float crouchPullSpeed = 30f;

    [Header("Ground")]
    [SerializeField] private Transform groundChecker;
    [SerializeField] private float groundCheckerRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Lane")]
    [SerializeField] private float laneSwitchSpeed = 5f;
    [SerializeField] private float laneSwitchDistance = 4f;

    [Header("Buttons")]
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference touchAction;
    [Range(0.02f, 0.3f)] [SerializeField] private float minSwipeScreenPercent = 0.08f;

    [Header("StartPoint")]
    [SerializeField] private Transform playerStartPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip crouchClip;

    internal InputAction PressAction => crouchAction != null ? crouchAction.action : null;
    internal InputAction PositionAction => touchAction != null ? touchAction.action : null;

    private readonly PointerUiHitTest uiHitTest = new PointerUiHitTest();
    private CharacterController playerController;
    private float currentSpeed;
    private float verticalVelocity;
    private int currentLane;
    private bool isCrouching;
    private bool hasGesture;
    private Vector2 swipeOrigin;
    private TouchControl gestureTouch;
    private int gestureTouchId;

    private void Awake()
    {
        playerController = GetComponent<CharacterController>();
        if (visuals == null)
            visuals = GetComponent<PlayerVisuals>();
        if (visuals == null)
            visuals = gameObject.AddComponent<PlayerVisuals>();

        visuals.UseLegacyModels(playerModel, crouchModel);
        currentSpeed = startSpeed;
        SetCrouchPose(false, force: true);

        if (groundChecker == null || playerStartPoint == null || PressAction == null || PositionAction == null)
            Debug.LogError("PlayerMovement requires a start point, a ground checker and two input actions.", this);
    }

    private void Start()
    {
        Play();
    }

    private void OnDisable()
    {
        ClearGesture();
    }

    private void Update()
    {
        if (groundChecker == null || playerStartPoint == null)
            return;

        float deltaTime = Time.deltaTime;
        bool grounded = Physics.CheckSphere(groundChecker.position, groundCheckerRadius,
            groundLayer, QueryTriggerInteraction.Ignore);

        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * deltaTime);
        ApplyGravity(grounded, deltaTime);
        ReadGesture(out bool crouching, out bool released);
        SetCrouchPose(crouching);

        if (crouching)
            verticalVelocity = -crouchPullSpeed;
        else if (released && grounded)
            Jump();

        float targetX = playerStartPoint.position.x + currentLane * laneSwitchDistance;
        float nextX = Mathf.MoveTowards(transform.position.x, targetX, laneSwitchSpeed * deltaTime);
        Vector3 displacement = new Vector3(nextX - transform.position.x,
            verticalVelocity * deltaTime, currentSpeed * deltaTime);
        playerController.Move(displacement);
    }

    private void ApplyGravity(bool grounded, float deltaTime)
    {
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = 0f;
        else
            verticalVelocity -= gravityForce * deltaTime;
    }

    private void ReadGesture(out bool crouching, out bool released)
    {
        crouching = false;
        released = false;

        InputAction press = PressAction;
        InputAction position = PositionAction;
        if (press == null || position == null || !press.enabled || !position.enabled)
        {
            ClearGesture();
            return;
        }

        bool held = press.IsPressed();
        if (!hasGesture && held && press.WasPressedThisFrame())
        {
            bool knownPosition = PointerUiHitTest.TryGetPosition(press, out Vector2 startPosition);
            if (!knownPosition)
                startPosition = position.ReadValue<Vector2>();

            if (uiHitTest.IsOverUi(startPosition))
                return;

            hasGesture = true;
            swipeOrigin = startPosition;
            gestureTouch = PointerUiHitTest.FindTouch(press);
            if (gestureTouch != null)
                gestureTouchId = gestureTouch.touchId.ReadValue();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayEffect(crouchClip);
        }

        if (!hasGesture)
            return;

        if (gestureTouch != null)
        {
            var phase = gestureTouch.phase.ReadValue();
            if (gestureTouch.touchId.ReadValue() != gestureTouchId ||
                phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                ClearGesture();
                return;
            }

            if (phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                released = true;
                ClearGesture();
                return;
            }
        }

        if (!held)
        {
            released = press.WasReleasedThisFrame();
            ClearGesture();
            return;
        }

        crouching = true;
        Vector2 currentPosition = gestureTouch != null
            ? gestureTouch.position.ReadValue()
            : position.ReadValue<Vector2>();
        float deltaX = currentPosition.x - swipeOrigin.x;
        if (Mathf.Abs(deltaX) > Screen.width * minSwipeScreenPercent)
        {
            currentLane = Mathf.Clamp(currentLane + (deltaX > 0f ? 1 : -1), -1, 1);
            swipeOrigin = currentPosition;
        }
    }

    private void ClearGesture()
    {
        hasGesture = false;
        gestureTouch = null;
    }

    private void Jump()
    {
        verticalVelocity = jumpForce;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEffect(jumpClip);
    }

    private void SetCrouchPose(bool crouching, bool force = false)
    {
        if (!force && isCrouching == crouching)
            return;

        isCrouching = crouching;
        playerController.height = crouching ? 1f : 2f;
        playerController.center = crouching ? new Vector3(0f, -0.5f, 0f) : Vector3.zero;
        visuals.SetCrouching(crouching);
    }

    private void OnDrawGizmos()
    {
        if (groundChecker == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundChecker.position, groundCheckerRadius);
    }

    public void GameOver()
    {
        visuals.SetVisible(false);
    }

    public void Restart()
    {
        if (playerStartPoint == null)
            return;

        gameObject.SetActive(true);
        playerController.enabled = false;
        transform.SetPositionAndRotation(playerStartPoint.position, playerStartPoint.rotation);
        SetCrouchPose(false, force: true);
        playerController.enabled = true;

        currentSpeed = startSpeed;
        currentLane = 0;
        verticalVelocity = 0f;
        ClearGesture();

        Play();
        visuals.SetVisible(true);
    }

    public int GetDistance()
    {
        if (playerStartPoint == null)
            return 0;

        return Mathf.Max(0, (int)(transform.position.z - playerStartPoint.position.z));
    }

    public void Play()
    {
        if (visuals != null)
            visuals.SetSkin(SaveSystem.Instance != null ? SaveSystem.Instance.GetPlayableSkin() : 0);
    }
}
