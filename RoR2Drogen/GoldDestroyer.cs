using UnityEngine;

namespace RoR2Drogen
{
    class GoldDestroyer : MonoBehaviour
    {
        public void OnDestroy()
        {
            AkSoundEngine.PostEvent(DrogenMain.GnomeHuhPlay, gameObject);
        }
    }
}
