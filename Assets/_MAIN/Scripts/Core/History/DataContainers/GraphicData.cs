using GRAPHIC;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

namespace HISTORY
{
    [System.Serializable]
    public class GraphicData
    {
        public string panelName;
        public List<LayerData> layerDatas;

        [System.Serializable]
        public class LayerData
        {
            public int depth;
            public string graphicName;
            public string graphicPath;
            public bool isVideo;
            public bool useAudio;

            public LayerData(GraphicLayer layer)
            {
                depth = layer.layerDepth;

                if (layer.currentGraphic == null)
                    return;

                GraphicObject graphic = layer.currentGraphic;
                graphicName = graphic.graphicName;
                graphicPath = graphic.graphicPath;
                isVideo = graphic.isVideo;
                useAudio = graphic.useAudio;
            }
        }

        public static List<GraphicData> Capture()
        {
            List<GraphicData> graphicPanels = new List<GraphicData>();

            foreach (GraphicPanel graphicPanel in GraphicPanelManager.Instance.allPanels)
            {
                if (graphicPanel.isClear)
                    continue;

                GraphicData data = new GraphicData();
                data.panelName = graphicPanel.panelName;
                data.layerDatas = new List<LayerData>();
                foreach (GraphicLayer layer in graphicPanel.layers)
                {
                    LayerData layerData = new LayerData(layer);
                    data.layerDatas.Add(layerData);
                }

                graphicPanels.Add(data);
            }

            return graphicPanels;
        }

        public static void Apply(List<GraphicData> data)
        {
            List<string> cache = new List<string>();

            foreach (GraphicData panelData in data)
            {
                GraphicPanel panel = GraphicPanelManager.Instance.GetPanel(panelData.panelName);

                panel.Clear(immediate: true);

                foreach (LayerData layerData in panelData.layerDatas)
                {
                    GraphicLayer layer = panel.GetLayer(layerData.depth, createIfDoesNotExist: true);
                    if (layer.currentGraphic == null || layer.currentGraphic.graphicName != layerData.graphicName)
                    {
                        if (layerData.isVideo)
                        {
                            VideoClip clip = HistoryCache.LoadVideoClip(layerData.graphicPath);
                            if(clip != null)
                                layer.SetVideo(clip, filePath:layerData.graphicPath, immediate: true, useAudio: layerData.useAudio);
                            else
                                Debug.LogWarning($"History State: Couldnt load VideoClip in cache with path: {layerData.graphicPath}");
                        }
                        else
                        {
                            Texture tex = HistoryCache.LoadTexture(layerData.graphicPath);
                            if (tex != null)
                                layer.SetTexture(tex, filePath:layerData.graphicPath, immediate: true);
                            else
                                Debug.LogWarning($"History State: Couldnt load Texture in cache with path: {layerData.graphicPath}");
                        }
                    }
                }

                cache.Add(panel.panelName);
            }

            foreach(var graphicPanel in GraphicPanelManager.Instance.allPanels)
            {
                if (!cache.Contains(graphicPanel.panelName))
                    graphicPanel.Clear(immediate: true);
            }
        }
    }
}