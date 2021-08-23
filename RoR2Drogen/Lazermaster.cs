using UnityEngine;

namespace RoR2Drogen
{
	class Lazermaster : MonoBehaviour
	{
		public bool Alive { get; set; } = true;

		public void OnDisable()
		{
			LazermasterDed();
		}

		public void LazermasterDed()
		{
			Alive = false;
			AkSoundEngine.PostEvent(DrogenMain.LazerStop, gameObject);
		}
	}
}
