using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuakeImporter
{
    /// <summary>Anything a trigger can activate (doors, plats, etc.) implements this.</summary>
    public interface IQuakeTriggerable
    {
        void Activate(GameObject activator);
    }

    /// <summary>Marker left on the spawned info_player_start GameObject.</summary>
    public class QuakePlayerStart : MonoBehaviour { }

    /// <summary>
    /// Mirrors Quake's func_door: slides between a closed and open position along an axis
    /// derived from its "angle" key (-1 = up, -2 = down, else a horizontal compass angle),
    /// auto-closes after "wait" seconds, and can be activated either by direct touch or by
    /// a linked trigger_multiple (via QuakeTriggerRelay).
    /// </summary>
    public class QuakeFuncDoor : MonoBehaviour, IQuakeTriggerable
    {
        public float Speed = 1.5f;       // unity meters/second
        public float WaitSeconds = 3f;   // <0 means "stay open"
        public float MoveAngleDegrees = 0f;

        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private bool _isOpen;
        private bool _isMoving;

        public void Initialize()
        {
            _closedPosition = transform.position;

            Bounds bounds = ComputeBounds();
            Vector3 moveDir;
            float travelDistance;

            if (Mathf.Approximately(MoveAngleDegrees, -1f))
            {
                moveDir = Vector3.up;
                travelDistance = bounds.size.y;
            }
            else if (Mathf.Approximately(MoveAngleDegrees, -2f))
            {
                moveDir = Vector3.down;
                travelDistance = bounds.size.y;
            }
            else
            {
                moveDir = Quaternion.Euler(0f, -MoveAngleDegrees, 0f) * Vector3.forward;
                travelDistance = Mathf.Max(bounds.size.x, bounds.size.z);
            }

            _openPosition = _closedPosition + moveDir.normalized * travelDistance;

            // The door's own collider stays solid; a separate trigger volume (added below)
            // handles "walk up to the door and it opens" the way vanilla func_door behaves
            // when it has no targetname forcing trigger-only activation.
            var touch = gameObject.AddComponent<QuakeDoorDirectTouch>();
            touch.Owner = this;
        }

        private Bounds ComputeBounds()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(transform.position, Vector3.one);

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        public void Activate(GameObject activator)
        {
            if (_isMoving) return;
            StopAllCoroutines();
            StartCoroutine(_isOpen ? CloseRoutine() : OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            _isMoving = true;
            yield return MoveTo(_openPosition);
            _isOpen = true;
            _isMoving = false;

            if (WaitSeconds >= 0f)
            {
                yield return new WaitForSeconds(WaitSeconds);
                yield return CloseRoutine();
            }
        }

        private IEnumerator CloseRoutine()
        {
            _isMoving = true;
            yield return MoveTo(_closedPosition);
            _isOpen = false;
            _isMoving = false;
        }

        private IEnumerator MoveTo(Vector3 target)
        {
            while ((transform.position - target).sqrMagnitude > 0.0004f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, Speed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
        }
    }

    /// <summary>Lets walking directly into a func_door open it, like vanilla Quake.</summary>
    public class QuakeDoorDirectTouch : MonoBehaviour
    {
        public QuakeFuncDoor Owner;

        private void Start()
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;

            var renderer = GetComponentInChildren<Renderer>();
            Bounds bounds = renderer != null ? renderer.bounds : new Bounds(transform.position, Vector3.one);

            box.center = transform.InverseTransformPoint(bounds.center);
            box.size = bounds.size * 1.1f;
        }

        private void OnTriggerEnter(Collider other)
        {
            Owner.Activate(other.gameObject);
        }
    }

    /// <summary>
    /// Mirrors trigger_multiple: a normally-invisible volume that, once a collider enters
    /// it, calls Activate() on every entity its "target" key resolved to.
    /// </summary>
    public class QuakeTriggerRelay : MonoBehaviour
    {
        public List<GameObject> Targets = new List<GameObject>();
        public bool TriggerOnce = false;

        private bool _fired;

        private void Awake()
        {
            foreach (var col in GetComponentsInChildren<Collider>())
                col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerOnce && _fired) return;
            _fired = true;

            foreach (var target in Targets)
            {
                if (target == null) continue;
                var triggerable = target.GetComponent<IQuakeTriggerable>();
                triggerable?.Activate(other.gameObject);
            }
        }
    }
}
