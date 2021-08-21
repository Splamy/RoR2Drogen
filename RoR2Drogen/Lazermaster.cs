using UnityEngine;

namespace RoR2Drogen
{
	class Lazermaster : MonoBehaviour
	{
		public void Start()
		{
			AkSoundEngine.PostEvent(DrogenMain.LazerStart, gameObject);
		}

		public void OnDestroy()
		{
			AkSoundEngine.PostEvent(DrogenMain.LazerStop, gameObject);
		}
	}
}
