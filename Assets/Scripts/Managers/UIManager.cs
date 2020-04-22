using Actors.Base.Interface;
using UI.Hud;
using UI.MainMenu;
using UnityEngine;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        public GameObject UIPrefab;

        private bool isSpawned = false;

        private UI.Hud.UI uiHud;
        
        public void Spawn(Transform parent)
        {
            if (isSpawned)
            {
                return;
            }
            
            GameObject ui = Instantiate(UIPrefab, parent);

            uiHud = ui.GetComponent<UI.Hud.UI>();
//            uiHud.Init();
            isSpawned = true;
        }

        public void SetPlayer(GameObject player)
        {
            uiHud.Init();
            uiHud.healthBar.SetHealthable(player.GetComponent<IHealthable>());
        }

        public ActionBar GetActionBar()
        {
            return uiHud.actionBar;
        }


        public Loading GetLoadScreen()
        {
            return uiHud.loadingScreen;
        }

        public void HideHud()
        {
            uiHud.HideHud();
        }
        
        public void ShowHud()
        {
            uiHud.ShowHud();
        }
    }
}