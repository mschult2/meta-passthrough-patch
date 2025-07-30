using Unity.XR.CompositionLayers.Services;
using UnityEngine.XR.OpenXR.Features;
using Unity.XR.CompositionLayers.Provider;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Domo.Utilities
{
    /// <summary>
    /// Patch for a bug in the Unity OpenXR Meta package.
    ///
    /// Bug:
    /// Passthrough stops working if the headset loses and regains focus. Ie if you take the headset off and put it back on. Or if you put it to sleep and then wake it up.
    /// Additionally, passthrough doesn't work on the first app run after installing the app.
    ///
    /// Root Cause:
    /// MetaOpenXRPassthroughLayer (an ILayerHandler) incorrectly depends on resources that are destroyed when OpenXRCompositionLayersFeature disposes OpenXRLayerProvider, which in turn disposes all ILayerHandlers.
    /// So afterwards, when OpenXRCompositionLayersFeature news up a new OpenXRLayerProvider, MetaOpenXRPassthroughLayer no longer works.
    ///
    /// Fix:
    /// This class calls OnSessionEnd() before OpenXRCompositionLayersFeature, and sets the layer provider to null. This effectively circumvents OpenXRCompositionLayersFeature.OnSessionEnd(), since it doesn't dispose the layer provider if the layer provider is null.
    ///
    /// Details:
    /// OpenXRCompositionLayersFeature/OpenXRLayerProvider are in the OpenXR Plugin package, published by Unity.
    /// MetaOpenXRPassthroughLayer is in the Unity OpenXR Meta package.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = FeatureName,
        Desc = "Fixes Meta's passthrough resume bug in v77 of the OpenXR Meta plugin.",
        Company = "Niftysoft",
        OpenxrExtensionStrings = "XR_DOMO_composition_layer_cylinder XR_DOMO_composition_layer_equirect XR_DOMO_composition_layer_equirect2 XR_DOMO_composition_layer_cube XR_DOMO_composition_layer_color_scale_bias XR_DOMO_android_surface_swapchain",
        Version = "1.0.0",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        FeatureId = FeatureId,
        // High priority so it's called before OpenXRCompositionLayersFeature.OnSessionEnd().
        Priority = 1000
    )]
#endif
    public class DomoOpenXRCompositionLayersFeature : OpenXRFeature
    {
        public const string FeatureId = "com.niftysoft.openxr.feature.domocompositionlayers";
        internal const string FeatureName = "Domo: Passthrough Patch for OpenXR Meta";

        private ILayerProvider cachedLayerProvider = null;

        protected override void OnSessionBegin(ulong xrSession)
        {
            if (CompositionLayerManager.Instance != null && cachedLayerProvider != null)
            {
                // Note that the necessary assignment below causes the following Meta error to log, although it has no ill effect:
                // "xrCreatePassthroughFB failed with result XR_ERROR_FEATURE_ALREADY_CREATED_PASSTHROUGH_FB."
                CompositionLayerManager.Instance.LayerProvider = cachedLayerProvider;
            }
        }

        protected override void OnSessionEnd(ulong xrSession)
        {
            if (CompositionLayerManager.Instance != null)
            {
                cachedLayerProvider = CompositionLayerManager.Instance.LayerProvider;
                CompositionLayerManager.Instance.LayerProvider = null;
            }
        }
    }
}
