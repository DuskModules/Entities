using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuskModules.Entities {

	/// <summary> Entity that is always active, and never deactivates. </summary>
	public class EntityAlwaysActive : EntityCore {

		/// <summary> Never deactivate. </summary>
		protected override void DeactivateEntity() {
			// Do nothing.
		}

	}

}