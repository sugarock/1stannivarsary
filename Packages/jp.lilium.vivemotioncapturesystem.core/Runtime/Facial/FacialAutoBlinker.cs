using System.Collections;
using UnityEngine;

namespace VMCCore.Facial.Public
{

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Facial))]
    public class FacialAutoBlinker : MonoBehaviour
    {
        public Face face { get; private set; }

        [SerializeField]
        float m_interVal = 5.0f;

        [SerializeField]
        float _closingTime = 0.06f;

        [SerializeField]
        float _openingSeconds = 0.03f;

        [SerializeField]
        float _closeSeconds = 0.1f;

        protected Coroutine _coroutine;

        float _nextRequest;
        bool _request;
        public bool Request
        {
            get { return _request; }
            set
            {
                if (Time.time < _nextRequest)
                {
                    return;
                }
                _request = value;
                _nextRequest = Time.time + 1.0f;
            }
        }

        private void Awake ()
        {
            if (face == null) face = GetComponent<Face> ();
        }

        private void OnEnable ()
        {
            if (face == null) return;
            _coroutine = StartCoroutine (BlinkRoutine ());
        }

        private void OnDisable ()
        {
            if (_coroutine != null) {
                StopCoroutine (_coroutine);
                _coroutine = null;
            }
        }

        public void OnModelChanged (Animator target)
        {
            StopAllCoroutines ();
            face = null;

            if (target != null) {
                face = target.GetComponent<Face> ();
                _coroutine = StartCoroutine (BlinkRoutine ());
            }
        }

        private void Start()
        {
            var target = GetComponent<Animator> ();
            if (target != null) {
                OnModelChanged (target);
            }
        }

        protected IEnumerator BlinkRoutine()
        {
            while (face != null)
            {
                var waitTime = Time.time + Random.value * m_interVal;
                while (waitTime > Time.time)
                {
                    if (Request)
                    {
                        _request = false;
                        break;
                    }
                    yield return null;
                }

                // close
                var value = 0.0f;
                var closeSpeed = 1.0f / _closeSeconds;
                while (true)
                {
                    value += Time.deltaTime * closeSpeed;
                    face.SetValue (FacePreset.Blink, Mathf.Clamp01 (value));

                    if (value >= 1.0f) break;

                    yield return null;
                }

                // wait...
                yield return new WaitForSeconds(_closingTime);

                // open
                value = 1.0f;
                var openSpeed = 1.0f / _openingSeconds;
                while (true)
                {
                    value -= Time.deltaTime * openSpeed;
                    face.SetValue (FacePreset.Blink, Mathf.Clamp01 (value));

                    if (value < 0) break;

                    yield return null;
                }
            }
        }


    }
}
