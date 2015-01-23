﻿using UnityEngine;
using System.Collections;

public enum BorderCrossingActions { Nothing, Movement, Rotation, Mixed }
public enum KeyPressedActions { Nothing, Movement, Rotation }
public enum KeyResetActions { Nothing, Position, Rotation, Both }

[System.Serializable]
public class SettingsCameraControlls
{
    [SerializeField]
    private BorderCrossingActions _borderCrossingAction = BorderCrossingActions.Mixed;

    [SerializeField]
    private KeyPressedActions
        _leftMouseButtonAction = KeyPressedActions.Nothing,
        _rightMouseButtonAction = KeyPressedActions.Movement,
        _mousewheelAction = KeyPressedActions.Rotation;

    [SerializeField]
    private KeyResetActions _keyResetAction = KeyResetActions.Both;

    [SerializeField]
    private bool
        _allowWasdAndKeyArrows = true,
        _allowNumPad = true,
        _allowOwnKeys = false,
        _allowMovement = true,
        _allowRotation = true,
        _allowUpDownRotation = true,
        _allowZoomRotation = true,
        _zoomToMousePosition = true,
        _cameraBaseView = false;

    #region Controlls
    public BorderCrossingActions BorderCrossingAction
    {
        get { return _borderCrossingAction; }
        set { _borderCrossingAction = value; }
    }
    public KeyPressedActions RightMouseButtonAction
    {
        get { return _rightMouseButtonAction; }
        set { _rightMouseButtonAction = value; }
    }
    public KeyPressedActions LeftMouseButtonAction
    {
        get { return _leftMouseButtonAction; }
        set { _leftMouseButtonAction = value; }
    }
    public KeyPressedActions MousewheelAction
    {
        get { return _mousewheelAction; }
        set { _mousewheelAction = value; }
    }
    public KeyResetActions KeyResetAction
    {
        get { return _keyResetAction; }
        set { _keyResetAction = value; }
    }

    public bool AllowOwnKeys
    {
        get { return _allowOwnKeys; }
        set { _allowOwnKeys = value; }
    }
    public bool AllowNumPad
    {
        get { return _allowNumPad; }
        set { _allowNumPad = value; }
    }
    public bool AllowWasdAndKeyArrows
    {
        get { return _allowWasdAndKeyArrows; }
        set { _allowWasdAndKeyArrows = value; }
    }
    public bool AllowMovement
    {
        get { return _allowMovement; }
        set { _allowMovement = value; }
    }
    public bool AllowRotation
    {
        get { return _allowRotation; }
        set { _allowRotation = value; }
    }
    public bool AllowUpDownRotation
    {
        get { return _allowUpDownRotation; }
        set { _allowUpDownRotation = value; }
    }
    public bool AllowZoomRotation
    {
        get { return _allowZoomRotation; }
        set { _allowZoomRotation = value; }
    }
    public bool ZoomToMousePosition
    {
        get { return _zoomToMousePosition; }
        set { _zoomToMousePosition = value; }
    }
    public bool CameraBaseView
    {
        get { return _cameraBaseView; }
        set { _cameraBaseView = value; }
    }
    #endregion
}

[System.Serializable]
public class SettingsCameraKeys
{
    [SerializeField]
    private string
        _ownForwardKey = "w",
        _ownLeftKey = "a",
        _ownBackwardKey = "s",
        _ownRightKey = "d",
        _ownLeftRotationKey = "q",
        _ownRightRotationKey = "e",
        _ownZoomInKey = "x",
        _ownZoomOutKey = "y";

    #region Keys
    public string OwnZoomOutKey
    {
        get { return _ownZoomOutKey[0].ToString(); }
        set { _ownZoomOutKey = value[0].ToString(); }
    }
    public string OwnZoomInKey
    {
        get { return _ownZoomInKey[0].ToString(); }
        set { _ownZoomInKey = value[0].ToString(); }
    }
    public string OwnRightRotationKey
    {
        get { return _ownRightRotationKey[0].ToString(); }
        set { _ownRightRotationKey = value[0].ToString(); }
    }
    public string OwnLeftRotationKey
    {
        get { return _ownLeftRotationKey[0].ToString(); }
        set { _ownLeftRotationKey = value[0].ToString(); }
    }
    public string OwnRightKey
    {
        get { return _ownRightKey[0].ToString(); }
        set { _ownRightKey = value[0].ToString(); }
    }
    public string OwnBackwardKey
    {
        get { return _ownBackwardKey[0].ToString(); }
        set { _ownBackwardKey = value[0].ToString(); }
    }
    public string OwnLeftKey
    {
        get { return _ownLeftKey[0].ToString(); }
        set { _ownLeftKey = value[0].ToString(); }
    }
    public string OwnForwardKey
    {
        get { return _ownForwardKey[0].ToString(); }
        set { _ownForwardKey = value[0].ToString(); }
    }
    #endregion
}

[System.Serializable]
public class SettingsCameraExtras
{
    [SerializeField]
    private Transform
        _followObject = null,
        _lookAtObject = null;

    [SerializeField]
    Rect _movementArea = new Rect(-50, -50, 100, 100);

    [SerializeField]
    [Range(0, 100)]
    private int _borderStrength = 5;

    #region Extras
    public Transform LookAtObject
    {
        get { return _lookAtObject; }
        set { _lookAtObject = value; }
    }
    public Transform FollowObject
    {
        get { return _followObject; }
        set { _followObject = value; }
    }
    public Rect MovementArea
    {
        get { return _movementArea; }
        set { _movementArea = value; }
    }
    public int BorderStrength
    {
        get { return _borderStrength; }
        private set { _borderStrength = value; }
    }
    #endregion
}

[System.Serializable]
public class SettingsCameraSettings
{
    [SerializeField]
    private float
        _movementSpeed = 0.5f,
        _rotationSpeed = 1.5f,
        _zoomSpeed = 2f,
        _buttonSpeed = 1f,
        _mouseButtonSpeed = 1.5f,
        _minimumZoom = 20f,
        _maximumZoom = 0.75f,
        _minimumZoomRotation = -0.75f,
        _zoomRotationSpeed = 12f,
        _zoomDownAngle = 50f,
        _maximumZoomDownAngle = 70f,
        _heightMultiplier = 1f,
        _lerpSpeed = 2.0f;

    #region Settings
    public float ZoomDownAngle
    {
        get { return _zoomDownAngle; }
        set { _zoomDownAngle = value; }
    }
    public float MaximumZoomDownAngle
    {
        get { return _maximumZoomDownAngle; }
        set { _maximumZoomDownAngle = value; }
    }
    public float MinimumZoom
    {
        get { return _minimumZoom; }
        set { _minimumZoom = value; }
    }
    public float MaximumZoom
    {
        get { return _maximumZoom; }
        set { _maximumZoom = value; }
    }
    public float MinimumZoomRotation
    {
        get { return _minimumZoomRotation; }
        set { _minimumZoomRotation = value; }
    }
    public float ZoomRotationSpeed
    {
        get { return _zoomRotationSpeed; }
        set { _zoomRotationSpeed = value; }
    }
    public float MouseButtonSpeed
    {
        get { return _mouseButtonSpeed; }
        set { _mouseButtonSpeed = value; }
    }
    public float ButtonSpeed
    {
        get { return _buttonSpeed; }
        set { _buttonSpeed = value; }
    }
    public float ZoomSpeed
    {
        get { return _zoomSpeed; }
        set { _zoomSpeed = value; }
    }
    public float RotationSpeed
    {
        get { return _rotationSpeed; }
        set { _rotationSpeed = value; }
    }
    public float MovementSpeed
    {
        get { return _movementSpeed; }
        set { _movementSpeed = value; }
    }
    public float HeightMultiplier
    {
        get { return _heightMultiplier; }
        set { _heightMultiplier = value; }
    }
    public float LerpSpeed
    {
        get { return _lerpSpeed; }
        set { _lerpSpeed = value; }
    }
    #endregion
}

[RequireComponent(typeof(PhotonView))]
public class CameraController : Photon.MonoBehaviour
{
    #region Variables
    [SerializeField]
    private SettingsCameraControlls _cameraControlls;

    [SerializeField]
    private SettingsCameraKeys _cameraKeys;

    [SerializeField]
    private SettingsCameraExtras _cameraExtras;

    [SerializeField]
    private SettingsCameraSettings _cameraSettings;

    private const float GeneralMultiplier = 0.3f;

    public float HeightMultiplier
    {
        get
        {
            var muliplier = transform.position.y * GeneralMultiplier * CameraSettings.HeightMultiplier;
            return muliplier;
        }
        private set { CameraSettings.HeightMultiplier = value; }
    }

    public Vector3 DefaultPosition { get; private set; }
    public Quaternion DefaultRotation { get; private set; }

    public SettingsCameraControlls CameraControlls
    {
        get { return _cameraControlls; }
        set { _cameraControlls = value; }
    }
    public SettingsCameraKeys CameraKeys
    {
        get { return _cameraKeys; }
        set { _cameraKeys = value; }
    }
    public SettingsCameraExtras CameraExtras
    {
        get { return _cameraExtras; }
        set { _cameraExtras = value; }
    }
    public SettingsCameraSettings CameraSettings
    {
        get { return _cameraSettings; }
        set { _cameraSettings = value; }
    }
    #endregion

    private void HandleUserInput()
    {
        if (Input.anyKey)
        {
            CameraExtras.FollowObject = null;
        }

        HandleKeyboardInput();
        HandleMouseInput();

        if (CameraExtras.FollowObject)
        {
            transform.position = new Vector3(
                CameraExtras.FollowObject.position.x - CameraExtras.FollowObject.transform.forward.x * 10,
                transform.position.y,
                CameraExtras.FollowObject.position.z - CameraExtras.FollowObject.transform.forward.z * 10);
            transform.LookAt(CameraExtras.FollowObject);
            return;
        }

        CorrectPositionToMovementArea();

        if (CameraExtras.LookAtObject)
        {
            transform.LookAt(CameraExtras.LookAtObject);
        }
    }

    #region Keyboard Input
    private void HandleKeyboardInput()
    {
        // Movement
        #region Movement
        if (CameraControlls.AllowMovement)
        {
            if (CameraControlls.AllowWasdAndKeyArrows)
            {
                if (Input.GetButton("Vertical"))
                {
                    MoveForwardOrBackward(Input.GetAxis("Vertical") * CameraSettings.ButtonSpeed);
                }
                if (Input.GetButton("Horizontal"))
                {
                    MoveRightOrLeft(Input.GetAxis("Horizontal") * CameraSettings.ButtonSpeed);
                }
            }
            if (CameraControlls.AllowNumPad)
            {
                if (Input.GetKey(KeyCode.Keypad3))
                {
                    MoveRightOrLeft(CameraSettings.ButtonSpeed); // Right
                }
                else if (Input.GetKey(KeyCode.Keypad1))
                {
                    MoveRightOrLeft(-CameraSettings.ButtonSpeed); // Left
                }

                if (Input.GetKey(KeyCode.Keypad5))
                {
                    MoveForwardOrBackward(CameraSettings.ButtonSpeed); // Forward
                }
                else if (Input.GetKey(KeyCode.Keypad2))
                {
                    MoveForwardOrBackward(-CameraSettings.ButtonSpeed); // Backward
                }
            }
            if (CameraControlls.AllowOwnKeys)
            {
                if (Input.GetKey(CameraKeys.OwnRightKey))
                {
                    MoveRightOrLeft(CameraSettings.ButtonSpeed); // Right
                }
                else if (Input.GetKey(CameraKeys.OwnLeftKey))
                {
                    MoveRightOrLeft(-CameraSettings.ButtonSpeed); // Left
                }

                if (Input.GetKey(CameraKeys.OwnForwardKey))
                {
                    MoveForwardOrBackward(CameraSettings.ButtonSpeed); // Forward
                }
                else if (Input.GetKey(CameraKeys.OwnBackwardKey))
                {
                    MoveForwardOrBackward(-CameraSettings.ButtonSpeed); // Backward
                }
            }
        }
        #endregion

        // Rotation
        #region Rotation
        if (CameraControlls.AllowRotation)
        {
            if (CameraControlls.AllowWasdAndKeyArrows)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    RotateRightOrLeft(CameraSettings.ButtonSpeed); // Right
                }
                else if (Input.GetKey(KeyCode.Q))
                {
                    RotateRightOrLeft(-CameraSettings.ButtonSpeed); // Left
                }
            }
            if (CameraControlls.AllowNumPad)
            {
                if (Input.GetKey(KeyCode.Keypad6))
                {
                    RotateRightOrLeft(CameraSettings.ButtonSpeed); // Right
                }
                else if (Input.GetKey(KeyCode.Keypad4))
                {
                    RotateRightOrLeft(-CameraSettings.ButtonSpeed); // Left
                }
            }
            if (CameraControlls.AllowOwnKeys)
            {
                if (Input.GetKey(CameraKeys.OwnRightRotationKey))
                {
                    RotateRightOrLeft(CameraSettings.ButtonSpeed); // Right
                }
                else if (Input.GetKey(CameraKeys.OwnLeftRotationKey))
                {
                    RotateRightOrLeft(-CameraSettings.ButtonSpeed); // Left
                }
            }
        }
        #endregion

        // Zoom
        #region Zoom
        if (CameraControlls.AllowWasdAndKeyArrows)
        {
            if (Input.GetKey(KeyCode.KeypadPlus))
            {
                Zoom(GeneralMultiplier); // In
            }
            else if (Input.GetKey(KeyCode.KeypadMinus))
            {
                Zoom(-GeneralMultiplier); // Out
            }
        }
        if (CameraControlls.AllowOwnKeys)
        {
            if (Input.GetKey(CameraKeys.OwnZoomInKey))
            {
                Zoom(GeneralMultiplier); // In
            }
            else if (Input.GetKey(CameraKeys.OwnZoomOutKey))
            {
                Zoom(-GeneralMultiplier); // Out
            }
        }
        #endregion

        // Reset
        #region Reset
        // Position reset
        if ((CameraControlls.KeyResetAction == KeyResetActions.Both || CameraControlls.KeyResetAction == KeyResetActions.Position) && Input.GetKey(KeyCode.Space))
        {
            transform.position = DefaultPosition;
        }
        // Rotation reset
        if ((CameraControlls.KeyResetAction == KeyResetActions.Both || CameraControlls.KeyResetAction == KeyResetActions.Rotation) && (Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Keypad0)))
        {
            transform.rotation = DefaultRotation;
        }
        #endregion
    }
    #endregion

    #region Mouse Input
    private void HandleMouseInput()
    {
        // Left Mouse Button
        if (Input.GetMouseButton(0))
        {
            HandleMouseButtonPressed(CameraControlls.LeftMouseButtonAction);
            return;
        }
        // Right Mouse Button
        if (Input.GetMouseButton(1))
        {
            HandleMouseButtonPressed(CameraControlls.RightMouseButtonAction);
            return;
        }
        // MouseWheel
        if (Input.GetMouseButton(2))
        {
            HandleMouseButtonPressed(CameraControlls.MousewheelAction);
            return;
        }

        // Zoom
        Zoom(Input.GetAxis("Mouse ScrollWheel"));
        CorrectCameraRotationToCurve();

        // Border Crossing
        HandleBorderCrossing();
    }

    #region Mouse Button
    private void HandleMouseButtonPressed(KeyPressedActions keyPressedAction)
    {
        if (keyPressedAction == KeyPressedActions.Movement && CameraControlls.AllowMovement)
        {
            // Movement
            MoveForwardOrBackward(-Input.GetAxis("Mouse Y") * CameraSettings.MouseButtonSpeed);
            MoveRightOrLeft(-Input.GetAxis("Mouse X") * CameraSettings.MouseButtonSpeed);
        }
        else if (keyPressedAction == KeyPressedActions.Rotation && CameraControlls.AllowRotation)
        {
            // Rotation
            RotateRightOrLeft(-Input.GetAxis("Mouse X") * CameraSettings.MouseButtonSpeed);

            if (CameraControlls.AllowUpDownRotation)
            {
                RotateUpOrDown(-Input.GetAxis("Mouse Y") * CameraSettings.MouseButtonSpeed);
            }
        }
    }
    #endregion

    #region Border Crossing
    private enum MousePositionBorder { OnScreen, Right, Left, Top, Bottom }
    private MousePositionBorder mousePositionBorder = MousePositionBorder.OnScreen;

    private void HandleBorderCrossing()
    {
        #region Check mouse position border
        mousePositionBorder = MousePositionBorder.OnScreen;

        if (Input.mousePosition.x > Screen.width - CameraExtras.BorderStrength && Input.mousePosition.x < Screen.width + CameraExtras.BorderStrength * 10)
        {
            mousePositionBorder = MousePositionBorder.Right;
        }
        else if (Input.mousePosition.x < CameraExtras.BorderStrength && Input.mousePosition.x > -CameraExtras.BorderStrength * 10)
        {
            mousePositionBorder = MousePositionBorder.Left;
        }
        else if (Input.mousePosition.y > Screen.height - CameraExtras.BorderStrength && Input.mousePosition.y < Screen.height + CameraExtras.BorderStrength * 10)
        {
            mousePositionBorder = MousePositionBorder.Top;
        }
        else if (Input.mousePosition.y < CameraExtras.BorderStrength && Input.mousePosition.y > -CameraExtras.BorderStrength * 10)
        {
            mousePositionBorder = MousePositionBorder.Bottom;
        }
        #endregion

        if (mousePositionBorder == MousePositionBorder.OnScreen)
        {
            return;
        }

        if (CameraControlls.BorderCrossingAction == BorderCrossingActions.Mixed && CameraControlls.AllowRotation && CameraControlls.AllowMovement)
        {
            HandleMixedBorderCrossing(mousePositionBorder);
            return;
        }

        if (CameraControlls.BorderCrossingAction == BorderCrossingActions.Movement && CameraControlls.AllowMovement)
        {
            HandleMovementBorderCrossing(mousePositionBorder);
            return;
        }

        if (CameraControlls.BorderCrossingAction == BorderCrossingActions.Rotation && CameraControlls.AllowRotation)
        {
            HandleRotationBorderCrossing(mousePositionBorder);
            return;
        }
    }
    private void HandleMixedBorderCrossing(MousePositionBorder mousePositionBorder)
    {
        switch (mousePositionBorder)
        {
            case MousePositionBorder.Right:
                if (Input.mousePosition.y < Screen.height / 2)
                {
                    MoveRightOrLeft(GeneralMultiplier);
                    break;
                }
                RotateRightOrLeft(1);
                break;
            case MousePositionBorder.Left:
                if (Input.mousePosition.y < Screen.height / 2)
                {
                    MoveRightOrLeft(-GeneralMultiplier);
                    break;
                }
                RotateRightOrLeft(-1);
                break;
            case MousePositionBorder.Top:
                MoveForwardOrBackward(GeneralMultiplier);
                break;
            case MousePositionBorder.Bottom:
                MoveForwardOrBackward(-GeneralMultiplier);
                break;
            default:
                break;
        }
    }
    private void HandleMovementBorderCrossing(MousePositionBorder mousePositionBorder)
    {
        switch (mousePositionBorder)
        {
            case MousePositionBorder.Right:
                MoveRightOrLeft(GeneralMultiplier);
                break;
            case MousePositionBorder.Left:
                MoveRightOrLeft(-GeneralMultiplier);
                break;
            case MousePositionBorder.Top:
                MoveForwardOrBackward(GeneralMultiplier);
                break;
            case MousePositionBorder.Bottom:
                MoveForwardOrBackward(-GeneralMultiplier);
                break;
            default:
                break;
        }
    }
    private void HandleRotationBorderCrossing(MousePositionBorder mousePositionBorder)
    {
        switch (mousePositionBorder)
        {
            case MousePositionBorder.Right:
                RotateRightOrLeft(1);
                break;
            case MousePositionBorder.Left:
                RotateRightOrLeft(-1);
                break;
            case MousePositionBorder.Top:
                RotateUpOrDown(1);
                break;
            case MousePositionBorder.Bottom:
                RotateUpOrDown(-1);
                break;
            default:
                break;
        }
    }
    #endregion
    #endregion

    public void SetCameraPosition(Vector3 position)
    {
        transform.position = CalculatePositionToLookAtPosition(position) + Vector3.up * transform.position.y;
    }

    public void SetCameraToBasePosition(bool lerp)
    {
        var basePosition = BaseManager.Get().OwnBase.BaseParent.position;
        basePosition += Vector3.up * transform.position.y;

        if (!lerp)
        {
            transform.position = basePosition;
            transform.LookAt(Vector3.zero);
            SetCameraPosition(basePosition);
            return;
        }

        MoveToPosition(basePosition);
    }

    [RPC]
    private void SetCameraToBasePosition(PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        SetCameraToBasePosition(false);
    }

    public void SetCameraToBasePositionAtPlayer(PhotonPlayer photonPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        photonView.RPC(RPCs.SetCameraToBasePosition, photonPlayer);
    }

    private void CorrectPositionToMovementArea()
    {
        // Forward/ backward
        if (transform.position.z < CameraExtras.MovementArea.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, CameraExtras.MovementArea.y);
        }
        else if (transform.position.z > CameraExtras.MovementArea.y + CameraExtras.MovementArea.height)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, CameraExtras.MovementArea.y + CameraExtras.MovementArea.height);
        }

        // Right/ left
        if (transform.position.x < CameraExtras.MovementArea.x)
        {
            transform.position = new Vector3(CameraExtras.MovementArea.x, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > CameraExtras.MovementArea.x + CameraExtras.MovementArea.width)
        {
            transform.position = new Vector3(CameraExtras.MovementArea.x + CameraExtras.MovementArea.width, transform.position.y, transform.position.z);
        }
    }

    private void CorrectCameraRotationToCurve()
    {
        if (!CameraControlls.AllowZoomRotation || CameraControlls.CameraBaseView)
        {
            return;
        }
        //    SetCorrectCameraRotation();
        //}
        ////else if (transform.rotation.x < CameraSettings.ZoomDownAngle && !CameraControlls.CameraBaseView)
        ////{
        ////    RotateToDefault();
        ////}
        //else if (CameraControlls.CameraBaseView && (transform.rotation.x < CameraSettings.MaximumZoomDownAngle || Vector3.Distance(transform.position, currentSelectedObjectPosition) > 0.01f))
        //{
        //    MoveToSelectedObject();
        //}
        //}

        //private void SetCorrectCameraRotation()
        //{
        //float currentRotation = transform.position.y * CameraSettings.ZoomRotationSpeed + CameraSettings.MinimumZoomRotation;
        float correctRotation = 0;

        if (transform.position.y > CameraSettings.MinimumZoom / 2)
        {
            correctRotation = Mathf.Min(CameraSettings.ZoomDownAngle + transform.position.y * CameraSettings.ZoomRotationSpeed / 2 - (CameraSettings.MinimumZoom / 2) * CameraSettings.ZoomRotationSpeed / 2, CameraSettings.MaximumZoomDownAngle);
        }
        else
        {
            correctRotation = Mathf.Min(transform.position.y * CameraSettings.ZoomRotationSpeed + CameraSettings.MinimumZoomRotation, CameraSettings.ZoomDownAngle);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(correctRotation, transform.rotation.eulerAngles.y, 0), Time.deltaTime * CameraSettings.LerpSpeed);
    }

    //private void RotateToDefault()
    //{
    //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(CameraSettings.ZoomDownAngle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z), Time.deltaTime * CameraSettings.LerpSpeed);
    //}

    //public void StartObjectView(Vector3 objectPosition)
    //{
    //    CameraControlls.CameraBaseView = true;
    //    currentSelectedObjectPosition = new Vector3(objectPosition.x, transform.position.y, objectPosition.z);
    //}

    public void MoveToPosition(Vector3 target)
    {
        StartCoroutine(MoveToTarget(target));
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(CameraSettings.MaximumZoomDownAngle, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z), Time.deltaTime * CameraSettings.LerpSpeed);
    }

    private IEnumerator MoveToTarget(Vector3 target)
    {
        var correctTarget = CalculatePositionToLookAtPosition(target);

        var finalTarget = new Vector3(correctTarget.x, transform.position.y, correctTarget.z);

        while (Vector3.Distance(transform.position, finalTarget) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, finalTarget, Time.deltaTime * 3 * CameraSettings.LerpSpeed);
            yield return null;
        }
    }

    private Vector3 CalculatePositionToLookAtPosition(Vector3 position)
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hitInfo;

        var isRaycastColliding = MouseController.Get().Map.collider.Raycast(ray, out hitInfo, 100f);

        if (!isRaycastColliding)
        {
            return position;
        }

        //var cameraLookAtPosition = hitInfo.point - transform.position;
        return position - hitInfo.point + transform.position;
    }

    #region Controlls
    /// <summary>
    /// <para> positive value for forward, negative value for backward </para>
    /// </summary>
    private void MoveForwardOrBackward(float movement)
    {
        /* If we don't want the camera height to change when moving, we have to
         * save the start height and reset it after moving.
         * Also we need to check whether we are out of movement area.
         */
        var startingHeight = transform.position.y;

        transform.position += transform.forward * CameraSettings.MovementSpeed * GeneralMultiplier * HeightMultiplier * movement;

        transform.position = new Vector3(transform.position.x, startingHeight, transform.position.z);
    }

    /// <summary>
    /// <para> positive value for right, negative value for left </para>
    /// </summary>
    private void MoveRightOrLeft(float movement)
    {
        /*
         * We need to check whether we are out of movement area.
         */
        transform.position += transform.right * CameraSettings.MovementSpeed * GeneralMultiplier * HeightMultiplier * movement;
    }

    /// <summary>
    /// <para> positive value for right, negative value for left </para>
    /// </summary>
    private void RotateRightOrLeft(float rotation)
    {
        CameraExtras.LookAtObject = null;

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y + CameraSettings.RotationSpeed * rotation,
            0);
    }

    /// <summary>
    /// <para> positive value for up, negative value for down </para>
    /// </summary>
    private void RotateUpOrDown(float rotation)
    {
        CameraExtras.LookAtObject = null;

        float rotationX;
        if (rotation > 0)
        {
            rotationX = transform.rotation.eulerAngles.x >= 5 ? transform.rotation.eulerAngles.x - CameraSettings.RotationSpeed * rotation : 4.9f;
        }
        else
        {
            rotationX = transform.rotation.eulerAngles.x <= 85 ? transform.rotation.eulerAngles.x - CameraSettings.RotationSpeed * rotation : 85.1f;
        }

        transform.rotation = Quaternion.Euler(rotationX, transform.rotation.eulerAngles.y, 0);
    }

    /// <summary>
    /// <para> positive value for in, negative value for out </para>
    /// </summary>
    private void Zoom(float zoom)
    {
        if (CameraControlls.ZoomToMousePosition)
        {
            if (zoom > 0 && transform.position.y > CameraSettings.MaximumZoom + 1)
            {
                var mousePositionOnMap = MouseController.Get().MousePositionOnMap;
                var zoomToPosition = new Vector3(mousePositionOnMap.x, CameraSettings.MaximumZoom, mousePositionOnMap.y);
                var zoomDirection = (transform.position - zoomToPosition).normalized;
                transform.position -= zoomDirection * CameraSettings.ZoomSpeed * HeightMultiplier * zoom;
            }
            else if (zoom < 0 && transform.position.y < CameraSettings.MinimumZoom)
            {
                transform.position += transform.forward * CameraSettings.ZoomSpeed * HeightMultiplier * zoom;
            }
        }
        else
        {
            if (transform.rotation.eulerAngles.x > CameraSettings.ZoomDownAngle
         && ((zoom > 0 && transform.position.y > CameraSettings.MaximumZoom)
         || (zoom < 0 && transform.position.y < CameraSettings.MinimumZoom)))
            {
                transform.position += transform.forward * CameraSettings.ZoomSpeed * HeightMultiplier * zoom;
            }
            else
            {
                transform.position += Vector3.down * CameraSettings.ZoomSpeed * HeightMultiplier * zoom;
            }
        }

        if (transform.position.y < CameraSettings.MaximumZoom)
        {
            transform.position = new Vector3(transform.position.x, CameraSettings.MaximumZoom, transform.position.z);
        }
        else if (transform.position.y > CameraSettings.MinimumZoom)
        {
            transform.position = new Vector3(transform.position.x, CameraSettings.MinimumZoom, transform.position.z);
        }
    }
    #endregion

    private void Init()
    {
        DefaultPosition = transform.position;
        DefaultRotation = transform.rotation;
    }

    private void Start()
    {
        Init();
    }

    private void Awake()
    {
        //Check for Singleton
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.LogError("Second instance!");
            return;
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
        HandleMouseInput();
    }

    private static CameraController _instance = null;
    public static CameraController Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<CameraController>();
        }

        return _instance;
    }
}