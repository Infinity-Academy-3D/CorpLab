using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class ForkliftController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float triggerThreshold = 0.1f;

    [Header("Steering")]
    [SerializeField] private float maxSteeringAngle = 45f;
    [SerializeField] private float wheelBase = 3f;
    [SerializeField] private Transform blWheel;
    [SerializeField] private Transform brWheel;

    [Header("Garfos")]
    [SerializeField] private Transform forkAssembly;
    [SerializeField] private float forkSpeed = 1f;
    [SerializeField] private float forkMinHeight = 0f;
    [SerializeField] private float forkMaxHeight = 3f;

    private float forkCurrentHeight;
    private Rigidbody forkRigidbody;

    private InputDevice rightController;
    private InputDevice leftController;
    private Quaternion blWheelBaseRot;
    private Quaternion brWheelBaseRot;

    public bool IsOccupied { get; private set; } = false;

    void Start()
    {
        TryGetControllers();
        if (blWheel != null) blWheelBaseRot = blWheel.localRotation;
        if (brWheel != null) brWheelBaseRot = brWheel.localRotation;
        if (forkAssembly != null)
        {
            forkCurrentHeight = forkAssembly.position.y;
            forkRigidbody = forkAssembly.GetComponent<Rigidbody>();
        }
    }

    public void Enter()
    {
        IsOccupied = true;
        TryGetControllers();
    }

    public void Exit()
    {
        IsOccupied = false;
    }

    void TryGetControllers()
    {
        var devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);
        if (devices.Count > 0) rightController = devices[0];

        devices.Clear();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, devices);
        if (devices.Count > 0) leftController = devices[0];
    }

    void Update()
    {
        if (!IsOccupied) return;

        if (!rightController.isValid || !leftController.isValid)
            TryGetControllers();

        HandleMovement();
        HandleSteering();
    }

    void FixedUpdate()
    {
        if (!IsOccupied) return;
        HandleForks();
    }

    void HandleMovement()
    {
        rightController.TryGetFeatureValue(CommonUsages.trigger, out float rightTrigger);
        leftController.TryGetFeatureValue(CommonUsages.trigger, out float leftTrigger);

        float direction = 0f;
        if (rightTrigger > triggerThreshold) direction -= rightTrigger;
        if (leftTrigger > triggerThreshold)  direction += leftTrigger;

        if (direction != 0f)
            transform.Translate(Vector3.right * moveSpeed * direction * Time.deltaTime, Space.Self);
    }

    void HandleForks()
    {
        if (forkAssembly == null) return;

        rightController.TryGetFeatureValue(CommonUsages.grip, out float rightGrip);
        leftController.TryGetFeatureValue(CommonUsages.grip, out float leftGrip);

        float forkDirection = 0f;
        if (rightGrip > triggerThreshold) forkDirection += rightGrip;
        if (leftGrip  > triggerThreshold) forkDirection -= leftGrip;

        if (forkDirection == 0f) return;

        // Controla via world Y para ignorar rotacao do Animator
        forkCurrentHeight = Mathf.Clamp(
            forkCurrentHeight + forkDirection * forkSpeed * Time.fixedDeltaTime,
            forkMinHeight, forkMaxHeight);

        if (forkRigidbody != null)
        {
            Vector3 target = forkRigidbody.position;
            target.y = forkCurrentHeight;
            forkRigidbody.MovePosition(target);
        }
        else
        {
            Vector3 pos = forkAssembly.position;
            pos.y = forkCurrentHeight;
            forkAssembly.position = pos;
        }
    }

    void HandleSteering()
    {
        if (!leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 stick))
            return;

        float steeringAngle = -stick.x * maxSteeringAngle;

        Quaternion steerRot = Quaternion.AngleAxis(steeringAngle, Vector3.up);
        if (blWheel != null) blWheel.localRotation = blWheelBaseRot * steerRot;
        if (brWheel != null) brWheel.localRotation = brWheelBaseRot * steerRot;

        if (Mathf.Abs(stick.x) > 0.05f)
        {
            rightController.TryGetFeatureValue(CommonUsages.trigger, out float rightTrigger);
            leftController.TryGetFeatureValue(CommonUsages.trigger, out float leftTrigger);

            float speed = 0f;
            if (rightTrigger > triggerThreshold) speed -= rightTrigger;
            if (leftTrigger > triggerThreshold)  speed += leftTrigger;

            if (Mathf.Abs(speed) > triggerThreshold)
            {
                float turnRate = (moveSpeed * speed / wheelBase)
                                 * Mathf.Tan(steeringAngle * Mathf.Deg2Rad);
                transform.Rotate(Vector3.up, turnRate * Mathf.Rad2Deg * Time.deltaTime, Space.World);
            }
        }
    }
}
