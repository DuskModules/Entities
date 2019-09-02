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

		/// <summary> Step it is at for appearing </summary>
		private int appearStep;
		/// <summary> Step it is at for disappearing </summary>
		private int disappearStep;

		// Listen for target entities callback
		protected override void Setup() {
			base.Setup();
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

		/// <summary> On hide </summary>
		protected override void EntityHide() {
			base.EntityHide();

			appearStep = 0;
			for (int i = 0; i < entities.Count; i++) {
				entities[i].HideInstantly();
			}
		}

		//================================[ Appearing ]================================\\
		/// <summary> Appear all things </summary>
		protected override void EntityAppearing() {
			base.EntityAppearing();

			appearDelay.Run(OnAppearStart);
		}

		// Appear start
		private void OnAppearStart() {
			if (!reverseOnDisappear)
				appearStep = 0;
			else
				disappearStep = appearStep;

			if (entities.Count > 0)
				TriggerStepAppear();
			else
				CompleteAppearing();

			disappearDelay.Stop();
			disappearStepDelay.Stop();
		}

		// Triggers the current step to appear
		private void TriggerStepAppear() {
			entities[appearStep].StartAppearing();
			if (appearStep < entities.Count - 1)
				appearStepDelay.Run(NextStepAppear);
			else {
				appearStepDelay.Stop();
				OnEntityAppeared();
			}
		}

		// Trigger next
		private void NextStepAppear() {
			appearStep++;
			if (reverseOnDisappear)
				disappearStep = appearStep;

			TriggerStepAppear();
		}

		// Called when a target behaviour has appeared
		private void OnEntityAppeared() {
			for (int i = 0; i < entities.Count; i++) {
				if (entities[i].state != EntityState.visible)
					return;
			}
			CompleteAppearing();
		}


		//================================[ Disappearing ]================================\\
		/// <summary> Disappear everything </summary>
		protected override void EntityDisappearing() {
			base.EntityDisappearing();

			disappearDelay.Run(OnDisappearStart);
		}

		// Disappear start
		private void OnDisappearStart() {
			if (!reverseOnDisappear)
				disappearStep = 0;
			else
				appearStep = disappearStep;

			if (entities.Count > 0)
				TriggerStepDisappear();
			else
				CompleteDisappearing();

			appearDelay.Stop();
			appearStepDelay.Stop();
		}

		// Triggers the current step to dissapear
		private void TriggerStepDisappear() {
			entities[disappearStep].StartDisappearing();
			if ((disappearStep >= 1 || !reverseOnDisappear) && (disappearStep < entities.Count - 1 || reverseOnDisappear))
				disappearStepDelay.Run(NextStepDisappear);
			else {
				disappearStepDelay.Stop();
				OnEntityDisappeared();
			}
		}
		// Trigger next
		private void NextStepDisappear() {
			if (reverseOnDisappear) {
				disappearStep--;
				appearStep = disappearStep;
			}
			else disappearStep++;

			TriggerStepDisappear();
		}

		// Called when a target behaviour has disappeared.
		private void OnEntityDisappeared() {
			for (int i = 0; i < entities.Count; i++) {
				if (entities[i].state != EntityState.hidden)
					return;
			}
			CompleteDisappearing();
		}

		// Update timer
		private void Update() {
			appearDelay.Update(deltaTime);
			disappearDelay.Update(deltaTime);
			appearStepDelay.Update(deltaTime);
			disappearStepDelay.Update(deltaTime);
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