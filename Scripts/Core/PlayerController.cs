using FPSGame.Weapons;
using UnityEngine;

namespace FPSGame.Core
{
  /// <summary>
  /// Player input: FPS movement + camera look + weapon commands.
  /// Works with CharacterBase on the same GameObject (composition, not inheritance).
  /// </summary>
  [RequireComponent(typeof(CharacterBase))]
  [RequireComponent(typeof(CharacterController))]
  public class PlayerController : MonoBehaviour
  {
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private WeaponHandler weaponHandler;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private bool lockCursorOnStart = true;

    [Header("Aim")]
    [SerializeField] private float aimMoveSpeedMultiplier = 0.5f;

    [Header("Animation (optional)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveYParam = "MoveY";
    [SerializeField] private string speedParam = "Speed";

    private CharacterBase character;
    private CharacterController controller;
    private Vector3 velocity;
    private float pitch;
    private int jumpsUsed;
    private bool isAiming;

    public bool IsAiming => isAiming;
    public bool IsSprinting => isSprinting;
    public Vector2 MoveInput { get; private set; }

    private bool isSprinting;

    private void Awake()
    {
      character = GetComponent<CharacterBase>();
      controller = GetComponent<CharacterController>();

      if (cameraPivot == null)
      {
        var cam = GetComponentInChildren<Camera>();
        if (cam != null)
          cameraPivot = cam.transform;
      }

      if (weaponHandler == null)
        weaponHandler = GetComponentInChildren<WeaponHandler>();
    }

    private void Start()
    {
      if (lockCursorOnStart)
        SetCursorLocked(true);
    }

    private void OnDisable()
    {
      SetCursorLocked(false);
    }

    private void Update()
    {
      if (GameplayPauseService.IsPaused)
        return;

      if (character == null || !character.IsAlive)
        return;

      HandleLook();
      HandleMovement();
      HandleWeaponInput();
      UpdateAnimator();
    }

    private void HandleLook()
    {
      if (cameraPivot == null)
        return;

      float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
      float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

      transform.Rotate(Vector3.up, mouseX, Space.World);

      pitch -= mouseY;
      pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
      cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
      var stats = character.Stats;
      if (stats == null)
        return;

      bool grounded = controller.isGrounded;
      character.SetGrounded(grounded);

      if (grounded && velocity.y < 0f)
        velocity.y = -2f;

      float horizontal = Input.GetAxisRaw("Horizontal");
      float vertical = Input.GetAxisRaw("Vertical");
      MoveInput = new Vector2(horizontal, vertical);

      Vector3 move = transform.right * horizontal + transform.forward * vertical;
      if (move.sqrMagnitude > 1f)
        move.Normalize();

      bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
      isAiming = Input.GetMouseButton(1);
      isSprinting = sprint && (horizontal != 0f || vertical != 0f) && !isAiming;
      PlayerNoiseEmitter.SetSprinting(gameObject, isSprinting);

      float speed = stats.walkSpeed;
      if (isAiming)
        speed *= aimMoveSpeedMultiplier;
      else if (isSprinting)
        speed = stats.sprintSpeed;

      controller.Move(move * speed * Time.deltaTime);

      if (Input.GetButtonDown("Jump") && grounded && jumpsUsed < stats.jumpLimit)
      {
        velocity.y = Mathf.Sqrt(stats.jumpHeight * -2f * stats.gravity);
        jumpsUsed++;
      }

      if (grounded)
        jumpsUsed = 0;

      velocity.y += stats.gravity * Time.deltaTime;
      controller.Move(velocity * Time.deltaTime);
    }

    private void HandleWeaponInput()
    {
      if (weaponHandler == null)
        return;

      if (Input.GetMouseButton(0))
        weaponHandler.TryPrimaryAction();

      if (Input.GetKeyDown(KeyCode.R))
        weaponHandler.TryReload();

      if (Input.GetKeyDown(KeyCode.Alpha1))
        weaponHandler.SetActiveSlot(0);
      if (Input.GetKeyDown(KeyCode.Alpha2))
        weaponHandler.SetActiveSlot(1);
      if (Input.GetKeyDown(KeyCode.Alpha3))
        weaponHandler.SetActiveSlot(2);

      float scroll = Input.GetAxis("Mouse ScrollWheel");
      if (scroll > 0f)
        weaponHandler.CycleActiveSlot(1);
      else if (scroll < 0f)
        weaponHandler.CycleActiveSlot(-1);
    }

    private void UpdateAnimator()
    {
      if (animator == null)
        return;

      var stats = character.Stats;
      float speedValue = MoveInput.magnitude * (stats != null ? stats.walkSpeed : 0f);

      if (!string.IsNullOrEmpty(moveXParam))
        animator.SetFloat(moveXParam, MoveInput.x);
      if (!string.IsNullOrEmpty(moveYParam))
        animator.SetFloat(moveYParam, MoveInput.y);
      if (!string.IsNullOrEmpty(speedParam))
        animator.SetFloat(speedParam, speedValue);
    }

    public void SetCursorLocked(bool locked)
    {
      Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
      Cursor.visible = !locked;
    }
  }
}
