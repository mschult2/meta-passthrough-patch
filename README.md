**Usage**
- You must have a Unity project that uses the Unity OpenXR Meta plugin, set up for passthrough in the usual way.
    - Add CompositionLayer to the scene, set to layer OpenXR Meta and order -1. I have to add it at runtime, due to a Unity compiler bug.
    - Enable the OpenXR feature Composition Layers. (The fix would be easier if I could disable it, but Unity grays out the button...)
- Add WiviOpenXRCompositionLayersFeature.cs to your Unity project.
- Enable the OpenXR feature "Passthrough Patch for OpenXR Meta".

# Bug  
Passthrough stops working if the Quest 3 headset loses and regains focus. Which happens when you take the headset off and put it back on. Or if you put it to sleep and then wake it up.Additionally, passthrough doesn't work on the first app run after installing the app.

This appears to be a regression in Unity OpenXR Meta. I used to have an older version of this package where this didn't happen.

# Root Cause  
MetaOpenXRPassthroughLayer (an ILayerHandler) incorrectly depends on resources that are destroyed when OpenXRCompositionLayersFeature disposes OpenXRLayerProvider, which in turn disposes all ILayerHandlers.So afterwards, when OpenXRCompositionLayersFeature news up a new OpenXRLayerProvider, MetaOpenXRPassthroughLayer no longer works.

# Fix  
The provided WiviOpenXRCompositionLayersFeature class is a custom OpenXR feature. It calls OnSessionEnd() before OpenXRCompositionLayersFeature (since this feature cannot be disabled in Unity), and sets the layer provider to null. This effectively circumvents OpenXRCompositionLayersFeature.OnSessionEnd(), since it doesn't dispose of the layer provider if the layer provider is null.

Note that the XR feature Composition Layers annoyingly can't be disabled (the button is grayed out). Which is why this custom XR feature has to specially circumvent it.

Note that this is more of a workaround than a fix. A proper fix for the Unity OpenXR Meta package would be a similar pattern but in the MetaOpenXRPassthroughLayer class.

# Details  
OpenXRCompositionLayersFeature/OpenXRLayerProvider are in the OpenXR Plugin package, published by Unity. MetaOpenXRPassthroughLayer is in the Unity OpenXR Meta package.

# Versions  
Unity 6000.0.51f1  
Unity OpenXR Meta 2.2.0  
OpenXR Plugin 1.15.1

I don't think these packages are related, but I'll include them anyway:  
Meta MR Utility Kit 77.0.0  
Meta XR Core SDK 77.0.0  
Meta XR Platform SDK 77.0.0
