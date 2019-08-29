using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuskModules.Entities {
	
	/// <summary> Behaviour to attach to any object that must be able to visually appear and dissapear. </summary>
	[AddComponentMenu("DuskModules/Entities/Entity")]
	public class Entity : MonoBehaviour {

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
		
		/// <summary> The core Entity on this object. Could be itself, could be something else. </summary>
		protected EntityCore core;

		/// <summary> State of entity </summary>
		public EntityState state { get; protected set; }

		/// <summary> Whether the entity is currently visible </summary>
		public bool visible => state != EntityState.hidden;
		/// <summary> Whether the entity is currently or soon to be fully visible </summary>
		public bool toVisible => state == EntityState.appearing || state == EntityState.visible;
		
		/// <summary> Whether the entity has setup and is ready </summary>
		public bool hasSetup { get; private set; }

		/// <summary> The type of timescale used. Uses deltaTime normally, but is set to unscaled if it is a RectTransform, which is likely in UI. </summary>
		public TimeType useTimeScale { get; set; }
		/// <summary> Passed time for this entity effect </summary>
		public float deltaTime {
			get {
				switch (useTimeScale) {
					case TimeType.deltaTime: return Time.deltaTime;
					case TimeType.unscaledDeltaTime: return DuskUtility.interfaceDeltaTime;
				}
				return 0;
			}
		}

		/// <summary> Awakens and checks for main, hooking into its events if this is not it </summary>
		protected virtual void Awake() {
			CheckSetup();
		}
		/// <summary> If it has hooked into a different main entity, remove those hooks. </summary>
		protected virtual void OnDestroy() {
			core.RemoveEntity(this);
		}

		/// <summary> Attempts to setup, executed only once. </summary>
		protected void CheckSetup() {
			if (!hasSetup) {
				hasSetup = true;
				Setup();
			}
		}

		/// <summary> Sets up and initializes the entity </summary>
		protected virtual void Setup() {
			if (transform.IsOfType(typeof(RectTransform)))
				useTimeScale = TimeType.unscaledDeltaTime;
			else
				useTimeScale = TimeType.deltaTime;

			core = gameObject.GetComponent<EntityCore>();
			if (core == null)
				core = gameObject.AddComponent<EntityCore>();
			if (core != this)
				core.AddEntity(this);
		}
		

		//================================[ Appearing ]================================\\
		/// <summary> Start appearing the object. </summary>
		public void StartAppearing() {
			CheckSetup();
			core.EntityAppearing();
		}

		/// <summary> Starts appearing this entity. </summary>
		protected virtual void EntityAppearing() {
			state = EntityState.appearing;
			InvokeAppearing();
		}
		
		/// <summary> Completes appearing the object. </summary>
		public void AppearInstantly() {
			CheckSetup();
			core.EntityAppeared();
		}

		/// <summary> Completes appearing the entity itself. </summary>
		public void CompleteAppearing() {
			CheckSetup();
			core.CompleteAppearStep(this);
		}

		/// <summary> Completes appearing the entity. </summary>
		protected virtual void EntityAppeared() {
			state = EntityState.visible;
			InvokeAppeared();
		}


		//================================[ Disappearing ]================================\\
		/// <summary> Starts disappearing the object. </summary>
		public void StartDisappearing() {
			CheckSetup();
			core.EntityDisappearing();
		}

		/// <summary> Starts disappearing the entity. </summary>
		protected virtual void EntityDisappearing() {
			state = EntityState.disappearing;
			InvokeDisappearing();
		}
		
		/// <summary> Instantly completes disappearing the entire object. </summary>
		public void DisappearInstantly() {
			CheckSetup();
			core.EntityDisappeared();
		}

		/// <summary> Completes disappearing the entity itself </summary>
		public void CompleteDisappearing() {
			CheckSetup();
			core.CompleteDisappearStep(this);
		}

		/// <summary> Completes disappearing the entity. </summary>
		protected virtual void EntityDisappeared() {
			state = EntityState.hidden;
			InvokeDisappeared();
		}

		/// <summary> Instantly hides the object. </summary>
		public void HideInstantly() {
			CheckSetup();
			core.EntityHide();
		}

		/// <summary> Instantly hides the entity. </summary>
		protected virtual void EntityHide() {
			state = EntityState.hidden;
			InvokeHidden();
		}


		//================================[ Invokes ]================================\\
		/// <summary> Invokes all listeners for onAppearing </summary>
		protected virtual void InvokeAppearing() {
			onAppearing?.Invoke();
		}
		/// <summary> Invokes all listeners for onAppeared </summary>
		protected virtual void InvokeAppeared() {
			onAppeared?.Invoke();
		}
		/// <summary> Invokes all listeners for onDisappearing </summary>
		protected virtual void InvokeDisappearing() {
			onDisappearing?.Invoke();
		}
		/// <summary> Invokes all listeners for onDisappeared </summary>
		protected virtual void InvokeDisappeared() {
			onDisappeared?.Invoke();
		}
		/// <summary> Invokes all listeners for onHidden </summary>
		protected virtual void InvokeHidden() {
			onHidden?.Invoke();
		}

		/// <summary> Appear another entity, used by core. </summary>
		protected void OtherEntityAppearing(Entity entity) {
			entity.CheckSetup();
			entity.EntityAppearing();
		}
		/// <summary> Appear another entity, used by core. </summary>
		protected void OtherEntityAppeared(Entity entity) {
			entity.CheckSetup();
			entity.EntityAppeared();
		}
		/// <summary> Disappear another entity, used by core. </summary>
		protected void OtherEntityDisappearing(Entity entity) {
			entity.CheckSetup();
			entity.EntityDisappearing();
		}
		/// <summary> Disappear another entity, used by core. </summary>
		protected void OtherEntityDisappeared(Entity entity) {
			entity.CheckSetup();
			entity.EntityDisappeared();
		}
		/// <summary> Hide another entity, used by core. </summary>
		protected void OtherEntityHide(Entity entity) {
			entity.CheckSetup();
			entity.EntityHide();
		}

	}
}