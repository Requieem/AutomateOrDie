using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.Runtime.Characters
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Character : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        [SerializeField] protected Animator m_animator;
        [SerializeField] protected SpriteRenderer m_spriteRenderer;
        [SerializeField] protected MovementModel m_movementModel;
        [SerializeField] protected InputActionReference m_moveAction;

        protected Vector2 MovementInput;
        protected Vector2 Velocity;
        protected Rigidbody2D Rigidbody;

        protected virtual void OnEnable()
        {
            m_moveAction.action.Enable();
            m_moveAction.action.performed += OnMove;
            m_moveAction.action.canceled += OnMove;
        }

        protected virtual void OnDisable()
        {
            m_moveAction.action.Disable();
            m_moveAction.action.performed -= OnMove;
            m_moveAction.action.canceled -= OnMove;
        }

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Rigidbody.gravityScale = 0;
            Rigidbody.freezeRotation = true;
        }

        protected virtual void OnMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        protected virtual void FixedUpdate()
        {
            Vector2 targetVelocity = MovementInput * m_movementModel.MaxSpeed;

            // Speed up toward target velocity
            if (MovementInput != Vector2.zero)
            {
                Velocity = Vector2.MoveTowards(
                    Velocity,
                    targetVelocity,
                    m_movementModel.Acceleration * Time.fixedDeltaTime
                );
            }
            else
            {
                // Decelerate
                Velocity = Vector2.MoveTowards(
                    Velocity,
                    Vector2.zero,
                    m_movementModel.Deceleration * Time.fixedDeltaTime
                );
            }

            Rigidbody.linearVelocity = Velocity;
            m_animator.SetFloat(Speed, Velocity.magnitude);

            UpdateFacingDirection(Velocity);
        }

        protected void UpdateFacingDirection(Vector2 velocity)
        {
            if (velocity.x > 0.1f)
                m_spriteRenderer.flipX = false;
            else if (velocity.x < -0.1f)
                m_spriteRenderer.flipX = true;
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (m_movementModel.Bounciness <= 0f)
                return;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector2 normal = contact.normal;
                Vector2 reflectedVelocity = Vector2.Reflect(Velocity, normal) * m_movementModel.Bounciness;

                Velocity = reflectedVelocity;
                Rigidbody.linearVelocity = reflectedVelocity;
                break; // apply bounce once
            }
        }
    }
}