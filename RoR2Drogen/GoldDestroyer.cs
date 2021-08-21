using UnityEngine;

namespace RoR2Drogen
{
	class GoldDestroyer : MonoBehaviour
	{
		public void OnDestroy()
		{
			var go = new GameObject("temp gnome");
			go.transform.position = gameObject.transform.position;
			AkSoundEngine.PostEvent(DrogenMain.GnomeHuhPlay, go, (uint)AkCallbackType.AK_EndOfEvent, (object cookie, AkCallbackType type, AkCallbackInfo info) =>
			{
				if (type == AkCallbackType.AK_EndOfEvent)
				{
					Destroy(go);
				}
			}, null);
		}
	}
}
