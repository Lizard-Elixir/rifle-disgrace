using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class ObjectiveDoNotHarmCivilians : Objective
	{
		protected override void Start()
		{
			base.Start();

			EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

			// set a title and description specific for this type of objective, if it hasn't one
			if (string.IsNullOrEmpty(Title))
				Title = "Do not harm any civilians";

			// This objective starts completed but can be failed.
			CompleteObjective(string.Empty, string.Empty, string.Empty);
		}

		void OnEnemyKilled(EnemyKillEvent evt)
		{
			if (IsFailed)
				return;

			if (evt.Enemy.CompareTag("Civilian"))
			{
				FailObjective(string.Empty, "Objective failed : " + Title);
			}
		}

		void OnDestroy()
		{
			EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
		}
	}
}