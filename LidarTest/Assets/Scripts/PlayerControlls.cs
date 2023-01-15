using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlls : MonoBehaviour
{
    CharacterController controller;
    [SerializeField] GameObject cam;
    [SerializeField] float speed = 1;
    [SerializeField] FixedJoystick variableJoystick;

    [SerializeField] float rotationSmoothTime;
    float currentAngle;
    float currentAngleVelocity;

    [Header("Gravity")]
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float gravityMultiplier = 2;
    [SerializeField] float groundedGravity = -0.5f;
    [SerializeField] float jumpHeight = 3f;
    private float velocityY;
    private bool jumped = false;

    [Header("Screens")]
    [SerializeField] GameObject endscreen;
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
    }

    private void HandleMovement()
    {
        //capturing Input from Player
        Vector3 movement = new Vector3(variableJoystick.Horizontal, 0, variableJoystick.Vertical).normalized;
        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 rotatedMovement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(rotatedMovement * speed * Time.deltaTime);
        }
    }

    void HandleGravityAndJump()
    {
        if (controller.isGrounded && velocityY < 0f)
            velocityY = groundedGravity;
        if (controller.isGrounded && (Input.GetKeyDown(KeyCode.Space) || jumped))
        {
            velocityY = Mathf.Sqrt(jumpHeight * 2f * gravity);
            jumped = false;
        }
        velocityY -= gravity * gravityMultiplier * Time.deltaTime;
        controller.Move(Vector3.up * velocityY * Time.deltaTime);
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
