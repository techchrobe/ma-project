using UnityEngine;

public class PlayerControlls : MonoBehaviour
{
    CharacterController controller;
    [SerializeField] GameObject groundCheckGo;
    [SerializeField] GameObject cam;
    [SerializeField] float speed = 1;
    [SerializeField] FixedJoystick variableJoystick;

    [SerializeField] float rotationSmoothTime;
    float currentAngle;
    float currentAngleVelocity;

    [SerializeField] float deathDistance = 100;

    [Header("Gravity")]
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float gravityMultiplier = 2;
    [SerializeField] float groundedGravity = -0.5f;
    [SerializeField] float jumpHeight = 0.8f;
    private float velocityY;
    private bool jumped = false;

    [Header("Screens")]
    [SerializeField] GameObject endscreen;

    public float JumpHeight { get => jumpHeight; }


    // Start is called before the first frame update 
    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        endscreen.SetActive(false);
    }

    private void Update()
    {
        HandleMovement();
        HandleGravityAndJump();
        CheckDeath();
    }

    private void HandleMovement()
    {
        //capturing Input from Player
        Vector3 movement = new Vector3(variableJoystick.Horizontal, 0, variableJoystick.Vertical).normalized;
        Vector3 speedVector = new Vector3(variableJoystick.Horizontal, 0, variableJoystick.Vertical);
        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 rotatedMovement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(rotatedMovement * (speed * speedVector.magnitude) * Time.deltaTime);
        }
    }

    private void HandleGravityAndJump()
    {
        if (IsGrounded() && velocityY < 0f)
            velocityY = groundedGravity;
        if (IsGrounded() && (Input.GetKeyDown(KeyCode.Space) || jumped))
        {
            velocityY = Mathf.Sqrt(jumpHeight * 2f * gravity);
            jumped = false;
        }
        velocityY -= gravity * gravityMultiplier * Time.deltaTime;
        controller.Move(Vector3.up * velocityY * Time.deltaTime);
    }

    private void CheckDeath()
    {
        if (transform.position.y < (cam.transform.position.y - deathDistance))
        {
            GameManager.Instance.ResetPlayer();
        }
    }

    bool IsGrounded()
    {
        if (Physics.SphereCast(groundCheckGo.transform.position, 0.03f, Vector3.down, out RaycastHit hit))
        {
            if (hit.distance <= 0.06f)
            {
                return true;
            }
        }
        return false;
    }

    public void Reset()
    {
        velocityY = 0;
    }

    public void Jump()
    {
        jumped = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Goal"))
        {
            endscreen.SetActive(true);
        }
    }
}
