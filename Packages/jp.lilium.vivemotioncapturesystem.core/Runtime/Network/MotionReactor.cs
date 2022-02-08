using UnityEngine;
using Unity.Collections;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using VMCCore.Facial;
using VMCCore.Retarget;


namespace VMCCore
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Facial.Public.Facial))]
    public class MotionReactor : MonoBehaviour
    {

        static readonly FaceKey[] kMouseBlendShapePresets = new FaceKey[] {
            FacePreset.A,
            FacePreset.I,
            FacePreset.U,
            FacePreset.E,
            FacePreset.O,
        };

        public MotionProvider provider;

        private HumanPoseHandler _humanPoseHandler;
        private HumanPose _currentPose;

        private PlayableGraph _graph;
        private AnimationScriptPlayable _streamPlayable;

        private NativeArray<MuscleHandle> _muscleHandles;
        private NativeArray<float> _muscleValues;

        public Animator animator => _animator = _animator != null ? _animator : GetComponent<Animator> ();
        Animator _animator;

        public Face face => _face = _face != null ? _face : GetComponent<Face> ();
        Face _face;

        [Tooltip ("最も高い口形素のみを使用する")]
        public bool maxVisemesEmphasis = true;

        [Tooltip ("口形素の倍率")]
        public float visemesMultiply = 1.0f;

        [Tooltip ("最小判定のしきい値")]
        public float visemesThreashold = 0.1f;

        [Tooltip ("「A」の口形のみにする")]
        public bool onlyVisemesA = false;

        [Tooltip ("口を閉じる時間")]
        public float closeTime = 0.5f;

        // 起点
        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private void Awake ()
        {
            UpdateStartPosition();
        }

        private void OnEnable ()
        {
            Debug.Assert (animator != null);
            Debug.Assert (animator.avatar != null);

            // EnableのON/Offでずれる現象を防ぐ
            transform.localPosition = _startPosition;
            transform.localRotation = _startRotation;

            // この時点の相対位置として表示される。
            _humanPoseHandler = new HumanPoseHandler (animator.avatar, animator.transform);
            _currentPose = new HumanPose ();
            _currentPose.muscles = new float[95];

            _graph = PlayableGraph.Create ($"{gameObject.name}.Stream");

            var output = AnimationPlayableOutput.Create (_graph, name, animator);

            // PlayableDirectorより先に姿勢を決めておく必要があるためSortingOrderで早めに処理されるよう設定する。
            // Unity 2019.3 以下の場合Timelineとの合成に相性問題あり。
            AnimationPlayableOutputExtensions.SetSortingOrder (output, 50);

            var job = new HumanPosePlayableJob ();
            _streamPlayable = AnimationScriptPlayable.Create (_graph, job);

            output.SetSourcePlayable (_streamPlayable);

            _graph.Play ();

            _muscleHandles = new NativeArray<MuscleHandle> (MuscleHandle.muscleHandleCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _muscleValues = new NativeArray<float> (MuscleHandle.muscleHandleCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            // マッスルハンドルを取得してJOBに登録する
            // TODO: HumanPosePlayableJob に移動
            MuscleHandle[] muscleHandles = new MuscleHandle[MuscleHandle.muscleHandleCount];
            MuscleHandle.GetMuscleHandles (muscleHandles);
            for (int i = 0; i < muscleHandles.Length; i++) {
                this._muscleHandles[i] = muscleHandles[i];
            }
        }

        private void OnDisable ()
        {
            _humanPoseHandler.Dispose ();
            _muscleHandles.Dispose ();
            _muscleValues.Dispose ();

            if (_graph.IsValid ())
                _graph.Destroy ();

        }


        void Start()
        {
        }


        void Update ()
        {
            if (provider == null) return;

            var motionPose = provider.motionPose;

            if (motionPose.isCreated) {
                var job = _streamPlayable.GetJobData<HumanPosePlayableJob> ();

                // 姿勢
                this._muscleValues.CopyFrom (motionPose.muscles);
                job.bodyLocalPosition = motionPose.bodyPosition;
                job.bodyLocalRotation = motionPose.bodyRotation;
                job.leftFootPosition = motionPose.goalPositions[(int)AvatarIKGoal.LeftFoot];
                job.rightFootPosition = motionPose.goalPositions[(int)AvatarIKGoal.RightFoot];
                job.leftFootRotation = motionPose.goalRotations[(int)AvatarIKGoal.LeftFoot];
                job.rightFootRotation = motionPose.goalRotations[(int)AvatarIKGoal.RightFoot];
                job.muscleHandles = this._muscleHandles;
                job.muscleValues = this._muscleValues;


                if (face != null) {

                    // 表情
                    for (int i = 0; i < MotionPose.HumanFacialExpressionsCount; i++) {
                        var facial = motionPose.facials[i];
                        if (facial.facial == 0) continue;
                        face.SetValue (new FaceKey (facial.facial, facial.variation), facial.weight);
                    }

                    // 入力
                    for (int i = 0; i < MotionPose.HumanFacialExpressionsCount; i++) {
                        var input = motionPose.inputsFloat[i];

                        if (input.key.LengthInBytes == 0) continue;

                        var key = face.avatar.Resolve(input.key);
                        if (key.preset == FacePreset.Unknown) continue;

                        face.SetEasingValue (key, input.value);
                    }

                    // リップシンク
                    _UpdateLipSync();

                }
                _streamPlayable.SetJobData (job);
            }
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        void _UpdateLipSync ()
        {
            if (face == null) return;
            if (provider == null) return;

            var motionPose = provider.motionPose;            

            int maxIndex = 0;
            float maxVisemes = 0;
            float[] visemes = new float[5];
            float loudness = motionPose.visemes[5];
            for (int i = 0; i < visemes.Length; i++) {
                visemes[i] = motionPose.visemes[i];
                if (visemes[i] < visemesThreashold) visemes[i] = 0;

                if (maxVisemes < visemes[i]) {
                    maxIndex = i;
                    maxVisemes = visemes[i];
                }
            }

            if (maxVisemesEmphasis) {
                for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                    if (i != maxIndex) visemes[i] = 0.0f;
                }
            }
            // すべてを母音を「A」にまとめる。
            if (onlyVisemesA) {
                face.SetValue (kMouseBlendShapePresets[0], maxVisemes * visemesMultiply);
            }
            /// A, I, U, E, O のそれぞれに反映する。
            else {
                for (int i = 0; i < kMouseBlendShapePresets.Length; i++) {
                    var prevValue = face.GetValue (kMouseBlendShapePresets[i]);
                    var value = visemes[i] * visemesMultiply;

                    value = (value > prevValue) ? value : Mathf.Clamp(prevValue - Time.deltaTime / closeTime, value, 2);

                    face.SetValue (kMouseBlendShapePresets[i], value);
                }
            }
        }

        /// <summary>
        /// 起点位置の更新
        /// </summary>
        public void UpdateStartPosition()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
        }

    }


}

