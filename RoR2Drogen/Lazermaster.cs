using UnityEngine;

namespace RoR2Drogen
{
	class Lazermaster : MonoBehaviour
	{
		public void OnDisable()
		{
			AkSoundEngine.PostEvent(DrogenMain.LazerStop, gameObject);
		}
	}
}
