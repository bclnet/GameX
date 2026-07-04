using UnityEngine;

namespace QuakeImporter
{
    /// <summary>
    /// Not a Quake player controller - just enough to walk into a level and verify the
    /// import worked (move, look, jump, open doors by walking into them or their triggers).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class QuakeQuickTestPlayer : MonoBehaviour
    {
        public float MoveSpeed = 6f;
        public float LookSpeed = 2f;
        public float JumpHeight = 1.2f;
        public float Gravity = -20f;

        private CharacterController _cc;
        private Camera _cam;
        private float _pitch;
        private Vector3 _velocity;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cam = GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            float mx = Input.GetAxis("Mouse X") * LookSpeed;
            float my = Input.GetAxis("Mouse Y") * LookSpeed;

            transform.Rotate(Vector3.up, mx);
            _pitch = Mathf.Clamp(_pitch - my, -85f, 85f);
            if (_cam != null) _cam.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
            move = Vector3.ClampMagnitude(move, 1f) * MoveSpeed;

            if (_cc.isGrounded)
            {
                _velocity.y = -1f;
                if (Input.GetButtonDown("Jump"))
                    _velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }
            _velocity.y += Gravity * Time.deltaTime;

            _cc.Move((move + new Vector3(0f, _velocity.y, 0f)) * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Escape))
                Cursor.lockState = CursorLockMode.None;
        }

        public static GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            var go = new GameObject("QuakeQuickTestPlayer");
            go.transform.SetPositionAndRotation(position + Vector3.up * 0.1f, rotation);

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(go.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();

            go.AddComponent<QuakeQuickTestPlayer>();
            return go;
        }
    }
}
