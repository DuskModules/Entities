using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuskModules.Entities {

	/// <summary> An entity that appears itself on start, and destroys itself on disappear. </summary>
	[AddComponentMenu("Entities/Entity Object", 0)]
	public class EntityObject : Entity {

		/// <summary> Whether the entity and all attached scripts have confirmed awoken. </summary>
		private bool awoken;

		// Hook into self
		protected override void Awake() {
			base.Awake();
			onDisappeared += HandleDisappear;
		}

		/// <summary> On enable, it hides and appears automatically. Override it to disable this. </summary>
		protected override void OnEnable() {
			if (state != EntityState.appearing && state != EntityState.visible) {
				HideInstant();
				AttemptAppear();
			}
		}

		/// <summary> Only appear after start has been called. </summary>
		protected virtual void Start() {
			awoken = true;
			AttemptAppear();
		}

		/// <summary> Attempts appearing the entity object </summary>
		private void AttemptAppear() {
			if (awoken)
				StartAppear();
		}

		/// <summary> Handle what should happen when it dissapears </summary>
		protected virtual void HandleDisappear() {
			Destroy(gameObject);
		}
	}

}