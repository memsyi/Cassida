using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
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
            _allowUpDownRotation = true;

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

        [SerializeField]
        private Transform
            _followObject = null,
            _lookAtObject = null;

        [SerializeField]
        Rect _movementArea = new Rect(-5, -5, 10, 10);

        [SerializeField]
        private float
            _movementSpeed = 1f,
            _rotationSpeed = 1f,
            _zoomSpeed = 1f,
            _buttonSpeed = 2f,
            _minimumZoom = 20f,
            _maximumZoom = 3f,
            _zoomDownAngle = 20f,
            _heightMultiplier = 1.3f;

        [SerializeField]
        [Range(0, 100)]
        private int _borderStrength = 5;

        public enum BorderCrossingActions { Nothing, Movement, Rotation, Mixed }
        public enum KeyPressedActions { Nothing, Movement, Rotation }
        public enum KeyResetActions { Nothing, Position, Rotation, Both }

        private const float GeneralMultiplier = 0.3f;

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

        #region Followed Object
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
        #endregion

        #region Controll Values
        public Rect MovementArea
        {
            get { return _movementArea; }
            set { _movementArea = value; }
        }
        public float ZoomDownAngle
        {
            get { return _zoomDownAngle; }
            private set { _zoomDownAngle = value; }
        }
        public float MinimumZoom
        {
            get { return _minimumZoom; }
            private set { _minimumZoom = value; }
        }
        public float MaximumZoom
        {
            get { return _maximumZoom; }
            private set { _maximumZoom = value; }
        }
        public float ButtonSpeed
        {
            get { return _buttonSpeed; }
            private set { _buttonSpeed = value; }
        }
        public float ZoomSpeed
        {
            get { return _zoomSpeed; }
            private set { _zoomSpeed = value; }
        }
        public float RotationSpeed
        {
            get { return _rotationSpeed; }
            private set { _rotationSpeed = value; }
        }
        public float MovementSpeed
        {
            get { return _movementSpeed; }
            private set { _movementSpeed = value; }
        }
        public int BorderStrength
        {
            get { return _borderStrength; }
            private set { _borderStrength = value; }
        }
        #endregion

        public float HeightMultiplier
        {
            get
            {
                var muliplier = transform.position.y * GeneralMultiplier * _heightMultiplier;
                return muliplier;
            }
            private set { _heightMultiplier = value; }
        }

        public Vector3 DefaultPosition { get; private set; }
        public Quaternion DefaultRotation { get; private set; }

        private void HandleUserInput()
        {
            if (Input.anyKey)
            {
                FollowObject = null;
            }

            HandleKeyboardInput();
            HandleMouseInput();

            if (FollowObject)
            {
                transform.position = new Vector3(
                    FollowObject.position.x - FollowObject.transform.forward.x * 10,
                    transform.position.y,
                    FollowObject.position.z - FollowObject.transform.forward.z * 10);
                transform.LookAt(FollowObject);
                return;
            }

            CorrectPositionToMovementArea();

            if (LookAtObject)
            {
                transform.LookAt(LookAtObject);
            }
        }

        #region Keyboard Input
        private void HandleKeyboardInput()
        {
            // Movement
            #region Movement
            if (AllowMovement)
            {
                if (AllowWasdAndKeyArrows)
                {
                    if (Input.GetButton("Vertical"))
                    {
                        MoveForwardOrBackward(Input.GetAxis("Vertical") * ButtonSpeed * GeneralMultiplier);
                    }
                    if (Input.GetButton("Horizontal"))
                    {
                        MoveRightOrLeft(Input.GetAxis("Horizontal") * ButtonSpeed * GeneralMultiplier);
                    }
                }
                if (AllowNumPad)
                {
                    if (Input.GetKey(KeyCode.Keypad3))
                    {
                        MoveRightOrLeft(ButtonSpeed); // Right
                    }
                    else if (Input.GetKey(KeyCode.Keypad1))
                    {
                        MoveRightOrLeft(-ButtonSpeed); // Left
                    }

                    if (Input.GetKey(KeyCode.Keypad5))
                    {
                        MoveForwardOrBackward(ButtonSpeed); // Forward
                    }
                    else if (Input.GetKey(KeyCode.Keypad2))
                    {
                        MoveForwardOrBackward(-ButtonSpeed); // Backward
                    }
                }
                if(AllowOwnKeys)
                {
                    if (Input.GetKey(OwnRightKey))
                    {
                        MoveRightOrLeft(ButtonSpeed); // Right
                    }
                    else if (Input.GetKey(OwnLeftKey))
                    {
                        MoveRightOrLeft(-ButtonSpeed); // Left
                    }

                    if (Input.GetKey(OwnForwardKey))
                    {
                        MoveForwardOrBackward(ButtonSpeed); // Forward
                    }
                    else if (Input.GetKey(OwnBackwardKey))
                    {
                        MoveForwardOrBackward(-ButtonSpeed); // Backward
                    }
                }
            }
            #endregion

            // Rotation
            #region Rotation
            if (AllowRotation)
            {
                if (AllowWasdAndKeyArrows)
                {
                    if (Input.GetKey(KeyCode.E))
                    {
                        RotateRightOrLeft(ButtonSpeed); // Right
                    }
                    else if (Input.GetKey(KeyCode.Q))
                    {
                        RotateRightOrLeft(-ButtonSpeed); // Left
                    }
                }
                if (AllowNumPad)
                {
                    if (Input.GetKey(KeyCode.Keypad6))
                    {
                        RotateRightOrLeft(ButtonSpeed); // Right
                    }
                    else if (Input.GetKey(KeyCode.Keypad4))
                    {
                        RotateRightOrLeft(-ButtonSpeed); // Left
                    }
                }
                if(AllowOwnKeys)
                {
                    if (Input.GetKey(OwnRightRotationKey))
                    {
                        RotateRightOrLeft(ButtonSpeed); // Right
                    }
                    else if (Input.GetKey(OwnLeftRotationKey))
                    {
                        RotateRightOrLeft(-ButtonSpeed); // Left
                    }
                }
            }
            #endregion

            // Zoom
            #region Zoom
            if (AllowWasdAndKeyArrows)
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
            if(AllowOwnKeys)
            {
                if (Input.GetKey(OwnZoomInKey))
                {
                    Zoom(GeneralMultiplier); // In
                }
                else if (Input.GetKey(OwnZoomOutKey))
                {
                    Zoom(-GeneralMultiplier); // Out
                }
            }
            #endregion

            // Reset
            #region Reset
            // Position reset
            if ((KeyResetAction == KeyResetActions.Both || KeyResetAction == KeyResetActions.Position) && Input.GetKey(KeyCode.Space))
            {
                transform.position = DefaultPosition;
            }
            // Rotation reset
            if ((KeyResetAction == KeyResetActions.Both || KeyResetAction == KeyResetActions.Rotation) && (Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Keypad0)))
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
                HandleMouseButtonPressed(LeftMouseButtonAction);
                return;
            }
            // Right Mouse Button
            if (Input.GetMouseButton(1))
            {
                HandleMouseButtonPressed(RightMouseButtonAction);
                return;
            }
            // MouseWheel
            if (Input.GetMouseButton(2))
            {
                HandleMouseButtonPressed(MousewheelAction);
                return;
            }

            // Zoom
            Zoom(Input.GetAxis("Mouse ScrollWheel"));

            // Border Crossing
            HandleBorderCrossing();
        }

        #region Mouse Button
        private void HandleMouseButtonPressed(KeyPressedActions keyPressedAction)
        {
            if (keyPressedAction == KeyPressedActions.Movement && AllowMovement)
            {
                // Movement
                MoveForwardOrBackward(-Input.GetAxis("Mouse Y"));
                MoveRightOrLeft(-Input.GetAxis("Mouse X"));
            }
            else if (keyPressedAction == KeyPressedActions.Rotation && AllowRotation)
            {
                // Rotation
                RotateRightOrLeft(-Input.GetAxis("Mouse X"));

                if (AllowUpDownRotation)
                {
                    RotateUpOrDown(-Input.GetAxis("Mouse Y"));
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

            if (Input.mousePosition.x > Screen.width - BorderStrength && Input.mousePosition.x < Screen.width + BorderStrength * 10)
            {
                mousePositionBorder = MousePositionBorder.Right;
            }
            else if (Input.mousePosition.x < BorderStrength && Input.mousePosition.x > -BorderStrength * 10)
            {
                mousePositionBorder = MousePositionBorder.Left;
            }
            else if (Input.mousePosition.y > Screen.height - BorderStrength && Input.mousePosition.y < Screen.height + BorderStrength * 10)
            {
                mousePositionBorder = MousePositionBorder.Top;
            }
            else if (Input.mousePosition.y < BorderStrength && Input.mousePosition.y > -BorderStrength * 10)
            {
                mousePositionBorder = MousePositionBorder.Bottom;
            }
            #endregion

            if (mousePositionBorder == MousePositionBorder.OnScreen)
            {
                return;
            }

            if (BorderCrossingAction == BorderCrossingActions.Mixed && AllowRotation && AllowMovement)
            {
                HandleMixedBorderCrossing(mousePositionBorder);
                return;
            }

            if (BorderCrossingAction == BorderCrossingActions.Movement && AllowMovement)
            {
                HandleMovementBorderCrossing(mousePositionBorder);
                return;
            }

            if (BorderCrossingAction == BorderCrossingActions.Rotation && AllowRotation)
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

        public void SetCameraPositionAndRotation(Vector3 position, Vector3 rotation)
        {
            transform.position = position;
            transform.eulerAngles = rotation;
        }

        private void CorrectPositionToMovementArea()
        {
            // Forward/ backward
            if (transform.position.z < MovementArea.y)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, MovementArea.y);
            }
            else if (transform.position.z > MovementArea.y + MovementArea.height)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, MovementArea.y + MovementArea.height);
            }

            // Right/ left
            if (transform.position.x < MovementArea.x)
            {
                transform.position = new Vector3(MovementArea.x, transform.position.y, transform.position.z);
            }
            else if (transform.position.x > MovementArea.x + MovementArea.width)
            {
                transform.position = new Vector3(MovementArea.x + MovementArea.width, transform.position.y, transform.position.z);
            }
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

            transform.position += transform.forward * MovementSpeed * GeneralMultiplier * HeightMultiplier * movement;

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
            transform.position += transform.right * MovementSpeed * GeneralMultiplier * HeightMultiplier * movement;
        }

        /// <summary>
        /// <para> positive value for right, negative value for left </para>
        /// </summary>
        private void RotateRightOrLeft(float rotation)
        {
            LookAtObject = null;

            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y + RotationSpeed * rotation,
                0);
        }

        /// <summary>
        /// <para> positive value for up, negative value for down </para>
        /// </summary>
        private void RotateUpOrDown(float rotation)
        {
            LookAtObject = null;

            float rotationX;
            if (rotation > 0)
            {
                rotationX = transform.rotation.eulerAngles.x >= 5 ? transform.rotation.eulerAngles.x - RotationSpeed * rotation : 4.9f;
            }
            else
            {
                rotationX = transform.rotation.eulerAngles.x <= 85 ? transform.rotation.eulerAngles.x - RotationSpeed * rotation : 85.1f;
            }

            transform.rotation = Quaternion.Euler(rotationX, transform.rotation.eulerAngles.y, 0);
        }

        /// <summary>
        /// <para> positive value for in, negative value for out </para>
        /// </summary>
        private void Zoom(float zoom)
        {
            if (transform.rotation.eulerAngles.x > ZoomDownAngle
             && ((zoom > 0 && transform.position.y > MaximumZoom)
             || (zoom < 0 && transform.position.y < MinimumZoom)))
            {
                transform.position += transform.forward * ZoomSpeed * HeightMultiplier * zoom;
            }
            else
            {
                transform.position += Vector3.down * ZoomSpeed * HeightMultiplier * zoom;
            }

            if (transform.position.y < MaximumZoom)
            {
                transform.position = new Vector3(transform.position.x, MaximumZoom, transform.position.z);
            }
            else if (transform.position.y > MinimumZoom)
            {
                transform.position = new Vector3(transform.position.x, MinimumZoom, transform.position.z);
            }
        }
        #endregion

        private void Init()
        {
            DefaultPosition = transform.position;
            DefaultRotation = transform.rotation;
        }

        void Start()
        {
            Init();
        }

        void FixedUpdate()
        {
            HandleUserInput();
        }
    }
}
