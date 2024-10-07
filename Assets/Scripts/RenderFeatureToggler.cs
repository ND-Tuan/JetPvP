using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;



public class RenderFeatureToggler : MonoBehaviour
{
    [SerializeField]
    private List<ScriptableRendererFeature> renderFeatures = new List<ScriptableRendererFeature>();
    [SerializeField]
    private UniversalRenderPipelineAsset pipelineAsset;

    public void ActivateRenderFeatures( int index, bool isEnabled)
    {
        renderFeatures[index].SetActive(isEnabled);
    }
}