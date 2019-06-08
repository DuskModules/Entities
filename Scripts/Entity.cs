using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuskModules.Entities {

	/// <summary> State of an entity's existence </summary>
	public enum EntityState {
		created,      // When it has not called any event yet, and has just been created.
		appearing,    // When it is becoming visible over time.
		visible,      // When it is visible, and growing or standing by at full visibility
		disappearing, // When it is being invisible over time
		hidden        // When it is fully dissapeared and invisible.
	}

	/// <summary> Behaviour to attach to any object that must be able to visually appear and dissapear. </summary>
	[AddComponentMenu("DuskModules/Entities/Entity", 0)]
	public class Entity : MonoBehaviour {

		/// <summary> The Main Entity on this object. Could be itself, could be something else. </summary>
		protected Entity mainEntity;

		/// <summary> Called when appearing starts. Call CompleteStepAppearing once when done with what you're doing. </summary>
		public event Action onAppearing;
		/// <summary> Called when it has fully appeared </summary>
		public event Action onAppeared;
		/// <summary> Called when destruction starts. Call CompleteStepDisappearing once when done with what you're doing. </summary>
		public event Action onDisappearing;
		/// <summary> Called when destruction completes </summary>
		public event Action onDisappeared;
		/// <summary> Called when hidden </summary>
		public event Action onHidden;

		/// <summary> State of entity </summary>
		public EntityState state { get; protected set; }

		/// <summary> Whether the entity is currently visible </summary>
		public bool visible => state != EntityState.hidden;
		/// <summary> Whether the entity is currently or soon to be fully visible </summary>
		public bool toVisible => state == EntityState.appearing || state == EntityState.visible;

		/// <summary> How many listeners have completed </summary>
		private int listenerStep;
		
		/// <summary> Awakens and checks for main, hooking into its events if this is not it </summary>
		protected virtual void Awake() {
			CheckMainEntity();
			if (mainEntity != this) {
				mainEntity.onAppearing += StartAppear;
				mainEntity.onAppeared += CompleteAppear;
				mainEntity.onDisappearing += StartDisappear;
				mainEntity.onDisappeared += CompleteDisappear;
				mainEntity.onHidden += HideInstant;
				
				// Match current state
				switch (mainEntity.state) {
					case EntityState.created: HideInstant(); break;
					case EntityState.appearing: StartAppear(); break;
					case EntityState.visible: CompleteAppear(); break;
					case EntityState.disappearing: StartDisappear(); break;
					case EntityState.hidden: HideInstant(); break;
				}
			}
		}
		/// <summary> If it has hooked into a different main entity, remove those hooks. </summary>
		protected virtual void OnDestroy() {
			if (mainEntity != this) {
				mainEntity.onAppearing -= StartAppear;
				mainEntity.onAppeared -= CompleteAppear;
				mainEntity.onDisappearing -= StartDisappear;
				mainEntity.onDisappeared -= CompleteDisappear;
				mainEntity.onHidden -= HideInstant;
			}
		}

		// Checks main entity existence
		private void CheckMainEntity() {
			if (mainEntity == null) mainEntity = gameObject.GetComponent<Entity>();
		}

		/// <summary> On enable, it hides and appears automatically. Override it to disable this. </summary>
		protected virtual void OnEnable() {
			if (state != EntityState.appearing && state != EntityState.visible)
				HideInstant();
		}
		/// <summary> On disable, it sets state as hidden. </summary>
		protected virtual void OnDisable() {
			if (state != EntityState.hidden)
				HideInstant();
		}

		/// <summary> Start appearing the object. </summary>
		public void StartAppearing() {
			CheckMainEntity();
			mainEntity.StartAppear();
		}
		/// <summary> Starts appearing this entity. </summary>
		protected void StartAppear() {
			if (state == EntityState.created)
				HideInstant();

			if (state != EntityState.appearing && state != EntityState.visible) {
				gameObject.SetActive(true);

				state = EntityState.appearing;
				if (onAppearing != null && onAppearing.GetInvocationList().Length > 0) {
					listenerStep = onAppearing.GetInvocationList().Length;
					onAppearing.Invoke();
				}
				else
					CompleteAppear();
			}
		}

		/// <summary> Completes one of the appearing listeners. </summary>
		public void CompleteStepAppearing() {
			if (state == EntityState.appearing) {
				listenerStep--;
				if (listenerStep == 0)
					CompleteAppear();
			}
		}

		/// <summary> Completes appearing the object. </summary>
		public void CompleteAppearing() {
			CheckMainEntity();
			mainEntity.CompleteAppear();
		}
		/// <summary> Completes appearing the entity. </summary>
		protected void CompleteAppear() {
			if (state == EntityState.created)
				HideInstant();

			if (state != EntityState.visible) {
				gameObject.SetActive(true);

				if (state != EntityState.appearing)
					onAppearing?.Invoke();

				state = EntityState.visible;
				onAppeared?.Invoke();

				if (mainEntity != this)
					mainEntity.CompleteStepAppearing();
			}
		}

		/// <summary> Starts disappearing the object. </summary>
		public void StartDisappearing() {
			CheckMainEntity();
			mainEntity.StartDisappear();
		}
		/// <summary> Starts disappearing the entity. </summary>
		protected void StartDisappear() {
			if (state != EntityState.disappearing && state != EntityState.hidden) {
				state = EntityState.disappearing;
				if (onDisappearing != null && onDisappearing.GetInvocationList().Length > 0) {
					listenerStep = onDisappearing.GetInvocationList().Length;
					onDisappearing.Invoke();
				}
				else
					CompleteDisappear();
			}
		}

		/// <summary> Completes one of the appearing listeners. </summary>
		public void CompleteStepDisappearing() {
			if (state == EntityState.disappearing) {
				listenerStep--;
				if (listenerStep == 0)
					CompleteDisappear();
			}
		}

		/// <summary> Completes disappearing the object. </summary>
		public void CompleteDisappearing() {
			CheckMainEntity();
			mainEntity.CompleteDisappear();
		}

		/// <summary> Completes disappearing the entity. </summary>
		public virtual void CompleteDisappear() {
			if (state != EntityState.disappearing)
				onDisappearing?.Invoke();

			if (state != EntityState.hidden) {
				state = EntityState.hidden;
				onDisappeared?.Invoke();
				onHidden?.Invoke();

				if (mainEntity != this)
					mainEntity.CompleteStepDisappearing();
			}
		}

		/// <summary> Instantly hides the object. </summary>
		public void HideInstantly() {
			CheckMainEntity();
			mainEntity.HideInstant();
		}
		/// <summary> Instantly hides the entity. </summary>
		protected void HideInstant() {
			if (state != EntityState.hidden) {
				state = EntityState.hidden;
				onHidden?.Invoke();
			}
		}
	}
}