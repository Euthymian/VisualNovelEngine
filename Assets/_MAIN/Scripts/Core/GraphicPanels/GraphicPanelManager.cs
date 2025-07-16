using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GRAPHIC
{
    public class GraphicPanelManager : MonoBehaviour
    {
        public const float DEFAULT_TRANSITION_SPEED = 3f;

        public static GraphicPanelManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }

        [field:SerializeField] public GraphicPanel[] allPanels { get; private set; }

        public GraphicPanel GetPanel(string panelName)
        {
            panelName = panelName.ToLower();
            foreach (GraphicPanel panel in allPanels)
            {
                if (panel.panelName.ToLower() == panelName)
                {
                    return panel;
                }
            }

            Debug.LogError($"Panel with name {panelName} not found.");
            return null;
        }
    }
}