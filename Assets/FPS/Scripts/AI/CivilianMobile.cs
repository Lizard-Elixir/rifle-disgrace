using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
	[RequireComponent(typeof(EnemyController))]
	public class CivilianMobile : MonoBehaviour
	{
		public enum AIState
		{
			Idle,
			Flee
		}

		public Animator Animator;

		[Tooltip("Fraction of the civilian's sight range at which it will stop running away from the player")]
		[Range(0f, 1f)]
		public float FleeStopDistanceRatio = 0.5f;

		[Tooltip("The random hit damage effects")]
		public ParticleSystem[] RandomHitSparks;

		public ParticleSystem[] OnDetectVfx;
		public AudioClip OnDetectSfx;

		[Header("Sound")] public AudioClip MovementSound;
		public MinMaxFloat PitchDistortionMovementSpeed;

		public AIState AiState { get; private set; }
		EnemyController m_EnemyController;
		AudioSource m_AudioSource;

		const string k_AnimMoveSpeedParameter = "MoveSpeed";
		const string k_AnimAttackParameter = "Attack";
		const string k_AnimAlertedParameter = "Alerted";
		const string k_AnimOnDamagedParameter = "OnDamaged";

		void Start()
		{
			m_EnemyController = GetComponent<EnemyController>();
			DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyMobile>(m_EnemyController, this,
				gameObject);

			m_EnemyController.onDetectedTarget += OnDetectedTarget;
			m_EnemyController.onLostTarget += OnLostTarget;
			m_EnemyController.SetPathDestinationToClosestNode();
			m_EnemyController.onDamaged += OnDamaged;

			// Start idling
			AiState = AIState.Idle;

			// adding a audio source to play the movement sound on it
			m_AudioSource = GetComponent<AudioSource>();
			DebugUtility.HandleErrorIfNullGetComponent<AudioSource, EnemyMobile>(m_AudioSource, this, gameObject);
			m_AudioSource.clip = MovementSound;
			m_AudioSource.Play();
		}

		void Update()
		{
			UpdateAiStateTransitions();
			UpdateCurrentAiState();

			float moveSpeed = m_EnemyController.NavMeshAgent.velocity.magnitude;

			// Update animator speed parameter
			Animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);

			// changing the pitch of the movement sound depending on the movement speed
			m_AudioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max,
				moveSpeed / m_EnemyController.NavMeshAgent.speed);
		}

		void UpdateAiStateTransitions()
		{
			// Handle transitions 
			switch (AiState)
			{
				case AIState.Idle:
					// Transition to flee when there is a line of sight to the threat
					if (m_EnemyController.IsSeeingTarget && m_EnemyController.IsTargetInAttackRange)
					{
						AiState = AIState.Flee;
						m_EnemyController.SetNavDestination(transform.position);
					}

					break;
				case AIState.Flee:
					// Transition to idle when no longer a threat in sight range
					if (!m_EnemyController.IsTargetInAttackRange)
					{
						AiState = AIState.Idle;
					}

					break;
			}
		}

		void UpdateCurrentAiState()
		{
			// Handle logic 
			switch (AiState)
			{
				case AIState.Idle:
					break;
				case AIState.Flee:
					m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
					m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
					m_EnemyController.OrientWeaponsTowards(m_EnemyController.KnownDetectedTarget.transform.position);
					break;
			}
		}

		void OnDetectedTarget()
		{
			if (AiState == AIState.Idle)
			{
				AiState = AIState.Flee;
			}

			for (int i = 0; i < OnDetectVfx.Length; i++)
			{
				OnDetectVfx[i].Play();
			}

			if (OnDetectSfx)
			{
				AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
			}

			Animator.SetBool(k_AnimAlertedParameter, true);
		}

		void OnLostTarget()
		{
			if (AiState == AIState.Flee)
			{
				AiState = AIState.Idle;
			}

			for (int i = 0; i < OnDetectVfx.Length; i++)
			{
				OnDetectVfx[i].Stop();
			}

			Animator.SetBool(k_AnimAlertedParameter, false);
		}

		void OnDamaged()
		{
			if (RandomHitSparks.Length > 0)
			{
				int n = Random.Range(0, RandomHitSparks.Length - 1);
				RandomHitSparks[n].Play();
			}

			Animator.SetTrigger(k_AnimOnDamagedParameter);
		}
	}
}