using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuskModules.Entities {

  /// <summary> Fires a sequence of entities to appear or dissapear. </summary>
  [AddComponentMenu("DuskModules/Entities/Entity Sequence", 0)]
  public class EntitySequence : Entity {

    /// <summary> The actual entities to fire off </summary>
    [Tooltip("List of entities to appear and dissapear, and in what order.")]
		public List<Entity> entities;
		
		/// <summary> Delay to wait between each step </summary>
		[Tooltip("Delay to wait between the start of each entity to appear.")]
		public TimerValue appearStepDelay;
		/// <summary> Delay to wait between each step </summary>
		[Tooltip("Delay to wait between the start of each entity to dissapear.")]
		public TimerValue disappearStepDelay;
		
		/// <summary> Delay to wait before the whole sequence </summary>
		[Tooltip("Delay to wait before appearing")]
		public TimerValue appearDelay;
		/// <summary> Delay to wait before the whole sequence </summary>
		[Tooltip("Delay to wait before disappearing")]
		public TimerValue disappearDelay;

		/// <summary> Whether to go in reverse when disappearing. </summary>
		[Tooltip("Whether to go in reverse when disappearing")]
		public bool reverseOnDisappear;
		
		/// <summary> Step it is at </summary>
		private int step;
		/// <summary> How many have been completed </summary>
		private int completed;

		// Hook into own events
		protected override void Awake() {
			base.Awake();

			onAppearing += this.OnAppearing;
			onDisappearing += this.OnDisappearing;
			for (int i = 0; i < entities.Count; i++) {
				entities[i].onAppeared += OnEntityAppeared;
				entities[i].onDisappeared += OnEntityDisappeared;
			}
		}

		// Unhook from own events
		protected override void OnDestroy() {
			base.OnDestroy();
			for (int i = 0; i < entities.Count; i++) {
				entities[i].onAppeared -= OnEntityAppeared;
				entities[i].onDisappeared -= OnEntityDisappeared;
			}
		}

		// When appearing starts
		private void OnAppearing() {
			appearDelay.Run(OnAppearStart);
		}
		// Appear start
		private void OnAppearStart() {
			step = 0;
			completed = 0;
			if (entities.Count > 0)
				TriggerStepAppear();
			else
				CompleteStepAppearing();
		}

		// When destroying starts
		private void OnDisappearing() {
			disappearDelay.Run(OnDisappearStart);
		}
		// Disappear start
		private void OnDisappearStart() {
			step = reverseOnDisappear ? entities.Count - 1 : 0;
			completed = 0;
			if (entities.Count > 0)
				TriggerStepDisappear();
			else
				CompleteStepDisappearing();
		}

		// Triggers the current step to appear
		private void TriggerStepAppear() {
			entities[step].StartAppearing();
			step++;
			if (step < entities.Count)
				appearStepDelay.Run(TriggerStepAppear);
			else
				appearStepDelay.Stop();
		}

		// Triggers the current step to dissapear
		private void TriggerStepDisappear() {
			entities[step].StartDisappearing();
			if (reverseOnDisappear) step--;
			else step++;
			if (step >= 0 && step < entities.Count)
				disappearStepDelay.Run(TriggerStepDisappear);
			else
				disappearStepDelay.Stop();
		}

		// Called when a target behaviour has appeared
		private void OnEntityAppeared() {
			completed++;
			if (completed == entities.Count) {
				CompleteStepAppearing();
			}
		}

		// Called when a target behaviour has disappeared.
		private void OnEntityDisappeared() {
			completed++;
			if (completed == entities.Count) {
				CompleteStepDisappearing();
			}
		}

		// Update timer
		private void Update() {
			appearDelay.Update();
			disappearDelay.Update();
			appearStepDelay.Update();
			disappearStepDelay.Update();
		}

		/// <summary> Editor button to reset and fill list with nearest children behaviours. </summary>
		[EditorButton]
		public void FillListWithChildren() {
			entities = new List<Entity>();
			FillListWith(transform);
		}

		private void FillListWith(Transform target) {
			for (int i = 0; i < target.childCount; i++) {
				Transform child = target.GetChild(i);
				Entity found = child.GetComponent<Entity>();
				if (found != null) {
					entities.Add(found);
				}
				else {
					FillListWith(child);
				}
			}
		}
	}
}