using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuskModules.Entities {

	/// <summary> Controller for any object with entities attached </summary>
	[AddComponentMenu("Entities/Entity Core")]
	public class EntityCore : Entity {

		/// <summary> Called when entity core is activated </summary>
		public event Action onActivate;

		/// <summary> Parts on this entity </summary>
		public List<Entity> entities { get; protected set; }
		/// <summary> Parts in progress of animating </summary>
		private List<Entity> progress;


		//================================[ Sub Entities ]================================\\
		/// <summary> Setup handles all initialization, and is called by everything else to make sure it's ready. </summary>
		protected override void Setup() {
			base.Setup();

			entities = new List<Entity>(gameObject.GetComponents<Entity>());
			entities.Remove(this);

			if (state == EntityState.created) {
				state = EntityState.hidden;
				InvokeHidden();
			}

			if (core != this) {
				Debug.LogError("Multiple Entity Core scripts are attached to " + gameObject.name + "! This is not recommended and can lead to strange behaviour.");
			}
		}

		/// <summary> Adds entity to keep in check </summary>
		public void AddEntity(Entity entity) {
			CheckSetup();

			if (!entities.Contains(entity)) {
				entities.Add(entity);
				switch (state) {
					case EntityState.hidden: OtherEntityHide(entity); break;
					case EntityState.appearing:
						if (!progress.Contains(entity)) progress.Add(entity);
						OtherEntityAppearing(entity);
						break;
					case EntityState.visible:
						OtherEntityAppearing(entity);
						OtherEntityAppeared(entity);
						break;
					case EntityState.disappearing:
						if (!progress.Contains(entity)) progress.Add(entity);
						OtherEntityDisappearing(entity);
						break;
				}
			}
		}

		/// <summary> Removes entity from records </summary>
		public void RemoveEntity(Entity entity) {
			CheckSetup();

			if (entities.Contains(entity)) {
				entities.Remove(entity);
				
				if (progress != null && progress.Contains(entity)) {
					progress.Remove(entity);
					CheckCompletion();
				}
			}
		}


		//================================[ Appearing ]================================\\
		/// <summary> When this object starts appearing, appear all others </summary>
		protected sealed override void EntityAppearing() {
			if (state != EntityState.appearing && state != EntityState.visible) {
				state = EntityState.appearing;
				
				if (entities.Count > 0) {
					progress = new List<Entity>(entities);
					InvokeAppearing();
				}
				else {
					AppearInstantly();
				}
			}
		}

		/// <summary> When this object completes appearing </summary>
		protected sealed override void EntityAppeared() {
			if (state != EntityState.visible) {
				if (state != EntityState.appearing) {
					InvokeAppearing();
				}
				
				state = EntityState.visible;
				InvokeAppeared();
			}
		}


		//================================[ Disappearing ]================================\\
		/// <summary> When this object starts disappearing </summary>
		protected sealed override void EntityDisappearing() {
			if (state != EntityState.disappearing && state != EntityState.hidden) {
				state = EntityState.disappearing;

				int count = entities.Count;
				if (count > 0) {
					progress = new List<Entity>(entities);
					InvokeDisappearing();
				}
				else {
					DisappearInstantly();
				}
			}
		}

		/// <summary> When this object completes disappearing </summary>
		protected sealed override void EntityDisappeared() {
			if (state != EntityState.hidden) {
				if (state != EntityState.disappearing) {
					InvokeDisappearing();
				}

				state = EntityState.hidden;

				InvokeDisappeared();
				InvokeHidden();
			}
		}

		/// <summary> When this object hides instantly </summary>
		protected sealed override void EntityHide() {
			if (state != EntityState.hidden) {
				state = EntityState.hidden;
				InvokeHidden();
			}
		}
		

		//================================[ Completion ]================================\\
		/// <summary> Called by entity that completed. </summary>
		public void CompleteAppearStep(Entity entity) {
			if (state == EntityState.appearing)
				CompleteStep(entity);
		}

		/// <summary> Called by entity that completed. </summary>
		public void CompleteDisappearStep(Entity entity) {
			if (state == EntityState.disappearing)
				CompleteStep(entity);
		}

		/// <summary> Remove this entity from progress list </summary>
		protected void CompleteStep(Entity entity) {
			if (progress == null || entity == this) return;
			CheckSetup();

			if (progress.Contains(entity)) {
				progress.Remove(entity);
				CheckCompletion();
			}
		}

		/// <summary> Checks completion of current animating </summary>
		private void CheckCompletion() {
			if (progress == null) return;
			if (progress.Count == 0) {
				if (state == EntityState.appearing)
					AppearInstantly();
				else if (state == EntityState.disappearing)
					DisappearInstantly();
			}
		}


		//================================[ Invokes ]================================\\
		/// <summary> Invokes all listeners for onAppearing </summary>
		protected sealed override void InvokeAppearing() {
			ActivateEntity();
			for (int i = 0; i < entities.Count; i++) {
				OtherEntityAppearing(entities[i]);
			}
			base.InvokeAppearing();
		}

		/// <summary> Invokes all listeners for onAppeared </summary>
		protected sealed override void InvokeAppeared() {
			ActivateEntity();
			for (int i = 0; i < entities.Count; i++) {
				OtherEntityAppeared(entities[i]);
			}
			base.InvokeAppeared();
		}

		/// <summary> Invokes all listeners for onDisappearing </summary>
		protected sealed override void InvokeDisappearing() {
			for (int i = 0; i < entities.Count; i++) {
				OtherEntityDisappearing(entities[i]);
			}
			base.InvokeDisappearing();
		}

		/// <summary> Invokes all listeners for onDisappeared </summary>
		protected sealed override void InvokeDisappeared() {
			for (int i = 0; i < entities.Count; i++) {
				OtherEntityDisappeared(entities[i]);
			}
			base.InvokeDisappeared();
		}

		/// <summary> Invokes all listeners for onHidden </summary>
		protected sealed override void InvokeHidden() {
			for (int i = 0; i < entities.Count; i++) {
				OtherEntityHide(entities[i]);
			}
			base.InvokeHidden();
			DeactivateEntity();
		}

		/// <summary> Called when the entity object must be activated </summary>
		protected virtual void ActivateEntity() {
			if (!gameObject.activeSelf) {
				gameObject.SetActive(true);
				InvokeActivate();
			}
		}

		/// <summary> Called when the enttiy object must be deactivated </summary>
		protected virtual void DeactivateEntity() {
			gameObject.SetActive(false);
		}

		/// <summary> Invokes activation </summary>
		protected void InvokeActivate() {
			onActivate?.Invoke();
		}
	}
}