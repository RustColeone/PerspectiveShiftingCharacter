using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Control Data")]
    InputController inputController;
    Vector2 movementInput;
    Vector2 cameraInput;

    public InputElement northInput = new InputElement();

    [Header("Player Camera Data")]
    public Transform cameraSystem;
    public Transform cameraPivot;
    public Transform cameraObject;
    public Transform cameraFollowTarget;
    [Range(0, 10)]
    public float cameraFollowSpeed;
    [Range(0, 10)]
    public float cameraRotateSpeed;
    public float cameraMaxAngle;
    public float cameraMinAngle;
    public Vector2 cameraAngles;

    [Header("Player Movement Data")]
    public CharacterController controller;
    public Vector3 moveDirection;
    [Range(0,10)]
    public float rotationSpeed;
    [Range(0, 10)]
    public float movementSpeed;
    public bool isThirdPerson;

    #region inputControls
    private void OnEnable()
    {
        if(inputController == null)
        {
            inputController = new InputController();
            inputController.Movement.Move.performed += inputController => movementInput = inputController.ReadValue<Vector2>();
            inputController.Movement.Camera.performed += inputController => cameraInput = inputController.ReadValue<Vector2>();

            inputController.Actions.ChangeView.started += inputController => northInput.risingEdge = true;
            inputController.Actions.ChangeView.performed += inputController => northInput.longPress = true;
            inputController.Actions.ChangeView.canceled += inputController => northInput.releaseEdges();
        }
        inputController.Enable();
    }
    private void OnDisable()
    {
        inputController.Disable();
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        MovementRotation();

        //TODO:character follows camera rotation
        //character follows camera and input
        //focus/lock on camera

        CameraChangeView();
        CameraMovement();
    }

    Vector3 normalVector = Vector3.up;
    private void HandleMovement()
    {
        moveDirection = cameraObject.forward * movementInput.y;
        moveDirection +=  cameraObject.right * movementInput.x;
        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
        projectedVelocity.Normalize();
        projectedVelocity *= movementSpeed;
        controller.Move(projectedVelocity * Time.deltaTime);
    }
    private void MovementRotation()
    {
        Vector3 targetDir = Vector3.zero;
        if (isThirdPerson)
        {
            targetDir = cameraObject.forward * movementInput.y;
            targetDir +=  cameraObject.right * movementInput.x;
        }
        else
        {
            targetDir = cameraObject.forward;
        }

        if(targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }
        Vector3 projectedDirection = Vector3.ProjectOnPlane(targetDir, normalVector);
        projectedDirection.Normalize();

        Quaternion targetDirection = Quaternion.LookRotation(projectedDirection);
        Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetDirection, rotationSpeed * Time.deltaTime);
        transform.rotation = smoothRotation;
    }

    private void CameraMovement()
    {
        cameraSystem.position = Vector3.Lerp(cameraSystem.position, cameraFollowTarget.position, Time.deltaTime * cameraFollowSpeed);

        cameraAngles.x += (cameraInput.x * cameraFollowSpeed) * Time.fixedDeltaTime;
        cameraAngles.y -= (cameraInput.y * cameraFollowSpeed) * Time.fixedDeltaTime;
        if (isThirdPerson)
            cameraAngles.y = Mathf.Clamp(cameraAngles.y, cameraMinAngle, cameraMaxAngle);
        else
            cameraAngles.y = Mathf.Clamp(cameraAngles.y, cameraMinAngle * 1.5f, cameraMaxAngle *1.5f);
        Vector3 rotation = Vector3.zero;

        rotation.y = cameraAngles.x;
        cameraSystem.rotation = Quaternion.Euler(rotation);

        rotation = Vector3.zero;
        rotation.x = cameraAngles.y;

        cameraPivot.localRotation = Quaternion.Euler(rotation);
    }

    private void CameraChangeView()
    {
        if (northInput.longPress)
        {
            isThirdPerson = true;
            Vector3 newPosition = new Vector3(0, 5, -15);
            //cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, newPosition, Time.deltaTime * cameraFollowSpeed * 2f);
            cameraObject.localPosition = Vector3.Lerp(cameraObject.localPosition, newPosition, Time.deltaTime * cameraFollowSpeed * 2f);
        }
        else
        {
            isThirdPerson = false;
            Vector3 newPosition = Vector3.zero;
            //cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, newPosition, Time.deltaTime * cameraFollowSpeed * 2f);
            cameraObject.localPosition = Vector3.Lerp(cameraObject.localPosition, newPosition, Time.deltaTime * cameraFollowSpeed * 2f);
        }
    }




    private void LateUpdate()
    {
        northInput.resetEdges();
    }


}
