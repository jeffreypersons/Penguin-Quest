using UnityEngine;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    [System.Serializable]
    [AddComponentMenu("PenguinMassConfig")]
    [RequireComponent(typeof(PenguinSkeleton))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PenguinMassConfig : MonoBehaviour
    {
        private const float MASS_AMOUNT_DEFAULT       =   250.00f;
        private const float MASS_AMOUNT_MIN           =     0.00f;
        private const float MASS_AMOUNT_MAX           = 10000.00f;
        private const float MASS_CENTER_COORD_DEFAULT =     0.00f;
        private const float MASS_CENTER_COORD_MIN     =  -500.00f;
        private const float MASS_CENTER_COORD_MAX     =   500.00f;

        [Header("Mass Settings")]
        [Tooltip("Constant (fixed) total mass for rigidbody")]
        [Range(MASS_AMOUNT_MIN, MASS_AMOUNT_MAX)]
        [SerializeField] private float mass = MASS_AMOUNT_DEFAULT;

        [Tooltip("Center of mass x component relative to skeletal root (ie smaller x means more prone to fall backwards)")]
        [Range(MASS_CENTER_COORD_MIN, MASS_CENTER_COORD_MAX)]
        [SerializeField] private float centerOfMassX = MASS_CENTER_COORD_DEFAULT;

        [Tooltip("Center of mass y component relative to skeletal root (ie smaller y means more resistant to falling over)")]
        [Range(MASS_CENTER_COORD_MIN, MASS_CENTER_COORD_MAX)]
        [SerializeField] private float centerOfMassY = MASS_CENTER_COORD_DEFAULT;

        private Animator penguinAnimator;
        private Rigidbody2D penguinRigidBody;

        public void Reset()
        {
            penguinRigidBody.useAutoMass  = false;
            penguinRigidBody.mass         = mass;
            penguinRigidBody.centerOfMass = new Vector2(centerOfMassX, centerOfMassY);
        }

        void Awake()
        {
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (!penguinRigidBody || !Application.IsPlaying(penguinRigidBody) || penguinRigidBody.useAutoMass)
            {
                return;
            }

            if (!Mathf.Approximately(centerOfMassX, penguinRigidBody.centerOfMass.x) ||
                !Mathf.Approximately(centerOfMassY, penguinRigidBody.centerOfMass.y))
            {
                penguinRigidBody.centerOfMass = new Vector2(centerOfMassX, centerOfMassY);
            }
            if (!Mathf.Approximately(mass, penguinRigidBody.mass))
            {
                penguinRigidBody.mass = mass;
            }
        }
        #endif
    }
}
