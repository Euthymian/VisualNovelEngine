using CHARACTER;
using DIALOGUE;
using GRAPHIC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestGraphicLayer : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //StartCoroutine(TestStackGraphicOn1Layer());
            StartCoroutine(TestMultipleLayersPanels());
        }

        IEnumerator TestStackGraphicOn1Layer()
        {
            GraphicPanel panel = GraphicPanelManager.Instance.GetPanel("Background");
            GraphicLayer layer = panel.GetLayer(0, true);

            yield return new WaitForSeconds(1f);

            Texture blendTex = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");

            //layer.SetTexture("Graphics/BG Images/2", blendingTexture: blendTex);
            layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", transitionSpeed:1, useAudio: true);

            yield return new WaitForSeconds(1f);

            layer.SetTexture("Graphics/BG Images/bathhouse", blendingTexture: blendTex, transitionSpeed: 1f);

            yield return new WaitForSeconds(3f);

            layer.currentGraphic.FadeOut();
        }

        IEnumerator TestMultipleLayersPanels()
        {
            GraphicPanel panel = GraphicPanelManager.Instance.GetPanel("Background");
            GraphicLayer layer0 = panel.GetLayer(0, true);
            GraphicLayer layer1 = panel.GetLayer(1, true);

            layer0.SetVideo("Graphics/BG Videos/Nebula");
            layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

            Character kyo = CharacterManager.Instance.CreateCharacter("KyoyaAkase", true);
            yield return kyo.Say("\"I am the storm that is approaching.\"");

            yield return new WaitForSeconds(1);

            GraphicPanel cinPanel = GraphicPanelManager.Instance.GetPanel("Cinematic");
            GraphicLayer cinLayer = cinPanel.GetLayer(0, true);

            cinLayer.SetTexture("Graphics/Gallery/pup");

            yield return DialogueSystem.Instance.Say("Narrator", "No one can become a storm");

            yield return new WaitForSeconds(1);

            cinPanel.Clear();

            yield return new WaitForSeconds(1);

            panel.Clear();
        }
    }
}