using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace GRAPHIC
{
    [System.Serializable]
    public class GraphicPanel
    {
        public string panelName;
        public GameObject rootPanel;
        public List<GraphicLayer> layers = new List<GraphicLayer>();

        public GraphicLayer GetLayer(int layerDepth, bool createIfDoesNotExist)
        {
            foreach (GraphicLayer layer in layers)
            {
                if (layer.layerDepth == layerDepth)
                    return layer;
            }

            if (createIfDoesNotExist)
                return CreateLayer(layerDepth);

            return null;
        }

        private GraphicLayer CreateLayer(int layerDepth)
        {
            GraphicLayer newLayer = new GraphicLayer();
            GameObject panel = new GameObject(string.Format(GraphicLayer.LAYER_OBJECT_NAME_FORMAT, layerDepth));
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            panel.AddComponent<CanvasGroup>();
            panel.transform.SetParent(rootPanel.transform, false);

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            newLayer.panel = panel.transform;
            newLayer.layerDepth = layerDepth;

            int index = layers.FindIndex(layer => layer.layerDepth > layerDepth);
            if (index == -1)
                layers.Add(newLayer);
            else
                layers.Insert(index, newLayer);

            for (int i = 0; i < layers.Count; i++)
                layers[i].panel.SetSiblingIndex(layers[i].layerDepth);

            return newLayer;
        }

        public void Clear(float transitionSpeed = 1, Texture blendTexure = null, bool immediate = false)
        {
            foreach (GraphicLayer layer in layers)
                layer.Clear(transitionSpeed, blendTexure, immediate);
            layers.Clear();
        }
    }
}