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

}