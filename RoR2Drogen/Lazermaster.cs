using UnityEngine;

namespace RoR2Drogen
{
	class Lazermaster : MonoBehaviour
	{
		public void OnDestroy()
		{
			AkSoundEngine.PostEvent(DrogenMain.LazerStop, gameObject);
		}
	}
}
