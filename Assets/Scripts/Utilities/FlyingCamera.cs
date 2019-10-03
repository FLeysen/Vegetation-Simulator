using UnityEngine;

public class FlyingCamera : MonoBehaviour
{
    [SerializeField] private float _standardVelocity = 15f;
    [SerializeField] private float _velocityDecreaseFactor = 0.5f;
    [SerializeField] private float _velocityIncreaseFactor = 2f;
    [SerializeField] private float _rotationalVelocity = 60f;

    [SerializeField] private string _mouseXAxis = "Mouse X";
    [SerializeField] private string _mouseYAxis = "Mouse Y";

    [SerializeField] private KeyCode _leftButton = KeyCode.A;
    [SerializeField] private KeyCode _forwardButton = KeyCode.W;
    [SerializeField] private KeyCode _backButton = KeyCode.S;
    [SerializeField] private KeyCode _rightButton = KeyCode.D;
    [SerializeField] private KeyCode _upButton = KeyCode.E;
    [SerializeField] private KeyCode _downButton = KeyCode.Q;

    [SerializeField] private KeyCode _fasterMoveButton = KeyCode.LeftShift;
    [SerializeField] private KeyCode _slowerMoveButton = KeyCode.LeftControl;
    [SerializeField] private KeyCode _holdToMoveButton = KeyCode.Mouse1;

    private float _xRotation = 0f;
    private float _yRotation = 0f;
    private bool _isKeyHeld = false;

    void Update()
    {
        if (!_isKeyHeld)
        {
            if (Input.GetKeyDown(_holdToMoveButton))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _isKeyHeld = true;
            }
            else
                return;
        }
        else if (Input.GetKeyUp(_holdToMoveButton))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _isKeyHeld = false;
            return;
        }

        Vector3 displacement = Vector3.zero;
        float frameMovement = _standardVelocity * Time.deltaTime;

        if (Input.GetKey(_downButton))
            displacement += transform.up * frameMovement;
        if (Input.GetKey(_upButton))
            displacement -= transform.up * frameMovement;

        if (Input.GetKey(_leftButton))
            displacement -= transform.right * frameMovement;
        if (Input.GetKey(_rightButton))
            displacement += transform.right * frameMovement;

        if (Input.GetKey(_backButton))
            displacement -= transform.forward * frameMovement;
        if (Input.GetKey(_forwardButton))
            displacement += transform.forward * frameMovement;

        if (Input.GetKey(_fasterMoveButton))
            displacement *= _velocityIncreaseFactor;
        if (Input.GetKey(_slowerMoveButton))
            displacement *= _velocityDecreaseFactor;

        transform.position += displacement;

        _xRotation += Time.deltaTime * _rotationalVelocity * Input.GetAxis(_mouseXAxis);
        _yRotation += Time.deltaTime * _rotationalVelocity * Input.GetAxis(_mouseYAxis);
        transform.localRotation = Quaternion.AngleAxis(_xRotation, Vector3.up) * Quaternion.AngleAxis(_yRotation, Vector3.left);
    }
}