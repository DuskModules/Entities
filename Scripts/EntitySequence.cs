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

		// Hook into own events
		protected override void Awake() {
			base.Awake();
			
			onHidden += OnHidden;
			onAppearing += OnAppearing;
			onDisappearing += OnDisappearing;
			for (int i = 0; i < entities.Count; i++) {
				entities[i].onAppeared += OnEntityAppeared;
				entities[i].onDisappeared += OnEntityDisappeared;
			}

			switch (state) {
				case EntityState.created: OnHidden(); break;
				case EntityState.appearing: OnAppearing(); break;
				case EntityState.visible: OnAppearing(); break;
				case EntityState.disappearing: OnDisappearing(); break;
				case EntityState.hidden: OnHidden(); break;
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

		// When instantly hidden
		private void OnHidden() {
			appearStep = 0;
			for (int i = 0; i < entities.Count; i++) {
				entities[i].HideInstantly();
			}
		}

		//================================[ Appearing ]================================\\
		// When appearing starts
		private void OnAppearing() {
			appearDelay.Run(OnAppearStart);
		}

		// Appear start
		private void OnAppearStart() {
			disappearDelay.Stop();
			disappearStepDelay.Stop();

			if (entities.Count > 0)
				TriggerStepAppear();
			else
				CompleteStepAppearing();
		}

		// Triggers the current step to appear
		private void TriggerStepAppear() {
			entities[appearStep].StartAppearing();
			if (appearStep < entities.Count - 1)
				appearStepDelay.Run(NextStepAppear);
			else
				appearStepDelay.Stop();
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
			CompleteStepAppearing();
		}


		//================================[ Disappearing ]================================\\
		// When destroying starts
		private void OnDisappearing() {
			disappearDelay.Run(OnDisappearStart);
		}

		// Disappear start
		private void OnDisappearStart() {
			appearDelay.Stop();
			appearStepDelay.Stop();

			if (entities.Count > 0)
				TriggerStepDisappear();
			else
				CompleteStepDisappearing();
		}

		// Triggers the current step to dissapear
		private void TriggerStepDisappear() {
			entities[disappearStep].StartDisappearing();
			if ((disappearStep >= 1 || !reverseOnDisappear) && (disappearStep < entities.Count - 1 || reverseOnDisappear))
				disappearStepDelay.Run(NextStepDisappear);
			else
				disappearStepDelay.Stop();
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
			CompleteStepDisappearing();
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