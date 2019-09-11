using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuskModules.Entities {

	/// <summary> An entity that appears itself on start, and destroys itself on disappear. </summary>
	[AddComponentMenu("Entities/Entity Object")]
	public class EntityObject : EntityCore {
		
		/// <summary> Listen for it's own disappearing. </summary>
		protected override void Setup() {
			base.Setup();
			StartAppearing();
		}

		/// <summary> On enable, show itself </summary>
		protected virtual void OnEnable() {
			StartAppearing();
		}
		/// <summary> Set to hidden if disabled by external means </summary>
		protected virtual void OnDisable() {
			HideInstantly();
		}

		/// <summary> Destroy the object. </summary>
		protected override void DeactivateEntity() {
			Destroy(gameObject);
		}

	}
}