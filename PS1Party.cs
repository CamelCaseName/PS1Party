using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.HighDefinition.Compositor;
using UnityEngine.UI;

namespace PS1PArty
{
    public class PS1Party : MelonMod
    {
        RenderTexture? rt;
        RawImage? image;
        Canvas? canvas;
        HDAdditionalCameraData? cameraData;
        int scaledHeight, scaledWidth;

        MelonPreferences_Entry<int>? Height;

        static PS1Party()
        {
            AssemblyResolverYoinker.SetOurResolveHandlerAtFront();
        }

        public override void OnInitializeMelon()
        {
            var category = MelonPreferences.CreateCategory("PS1Party");
            Height = category.CreateEntry("height", 224, "Height", description: "This it the virtual height of the game now. width scales accordingly");
            MelonPreferences.Save();
            float x = (float)Screen.height / (Height?.Value) ?? 224f;
            scaledWidth = (int)((float)Screen.width / x);
            scaledHeight = (int)((float)Screen.height / x);

            MelonLogger.Msg($"Initializing with set height {Height?.Value ?? 224} => {scaledWidth}x{scaledHeight} instead of {Screen.width}x{Screen.height}");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Disclaimer")
            {
                //todo we have to set filtermode to point for all textures
                foreach (var obj in UnityEngine.Object.FindObjectsOfTypeAll(Il2CppType.Of<Texture>()))
                {
                    var material = obj.TryCast<Texture>();

                    if (material is not null)
                    {
                        material.filterMode = FilterMode.Point;
                    }
                }

                GameObject CanvasGO = new()
                {
                    name = "lowres",
                    layer = LayerMask.NameToLayer("UI")
                };
                canvas = CanvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                //_ = CanvasGO.AddComponent<GraphicRaycaster>();
                float x = Screen.height / Height!.Value;
                scaledWidth = (int)(Screen.width / x);
                scaledHeight = (int)(Screen.height / x);
                rt = new RenderTexture(scaledWidth, scaledHeight, 16, RenderTextureFormat.ARGB4444)
                {
                    filterMode = FilterMode.Point,
                    anisoLevel = 0,
                    depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
                    antiAliasing = 1 //means off
                };
                Camera.main.targetTexture = rt;
                Camera.main.forceIntoRenderTexture = true;
                Camera.main.cameraType = CameraType.SceneView;

                GameObject ImageGO = new()
                {
                    name = "image",
                    layer = LayerMask.NameToLayer("UI")
                };
                ImageGO.transform.parent = CanvasGO.transform;
                image = ImageGO.AddComponent<RawImage>();
                image.texture = rt;
                ImageGO.transform.localPosition = Vector3.zero;
                ImageGO.transform.localScale = new(x, x, x);
                image.rectTransform.sizeDelta = new(rt.width, rt.height);

                //set some more camera settings
                cameraData = Camera.main.GetComponent<HDAdditionalCameraData>();
                var frameSettings = cameraData.m_RenderingPathCustomFrameSettings;
                frameSettings.SetEnabled(FrameSettingsField.Bloom, false);
                frameSettings.SetEnabled(FrameSettingsField.ChromaticAberration, false);
                frameSettings.SetEnabled(FrameSettingsField.Vignette, false);
                frameSettings.SetEnabled(FrameSettingsField.FilmGrain, false);
                frameSettings.SetEnabled(FrameSettingsField.ShadowMaps, false);
                frameSettings.SetEnabled(FrameSettingsField.SSAO, false);
                frameSettings.SetEnabled(FrameSettingsField.SSGI, false);
                frameSettings.SetEnabled(FrameSettingsField.ColorGrading, false);
                frameSettings.SetEnabled(FrameSettingsField.DirectSpecularLighting, false);
                frameSettings.SetEnabled(FrameSettingsField.SubsurfaceScattering, false);
                frameSettings.SetEnabled(FrameSettingsField.Dithering, false);
                frameSettings.SetEnabled(FrameSettingsField.Antialiasing, false);
                frameSettings.SetEnabled(FrameSettingsField.MSAA, false);
                frameSettings.SetEnabled(FrameSettingsField.ContactShadows, false);
                frameSettings.SetEnabled(FrameSettingsField.Shadowmask, false);
                frameSettings.SetEnabled(FrameSettingsField.SSAOAsync, false);
                frameSettings.materialQuality = UnityEngine.Rendering.MaterialQuality.Low;
                cameraData.m_RenderingPathCustomFrameSettings = frameSettings;

                Camera.main.allowMSAA = false;
                Camera.main.allowDynamicResolution = false;
                cameraData.allowDynamicResolution = false;
                cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                cameraData.customRenderingSettings = true;
                rt.antiAliasing = 1;
                QualitySettings.antiAliasing = 0;
                QualitySettings.lodBias = 0.001f;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.globalTextureMipmapLimit = 4;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                CanvasGO.transform.position = new(CanvasGO.transform.position.x, CanvasGO.transform.position.y, 100);

                if (sceneName == "GameMain")
                {
                    var volumeGO = GameObject.Find("Global Volume");
                    var volume = volumeGO?.GetComponent<Volume>();
                    if (volume is not null)
                    {
                        foreach (var item in volume.profile.components)
                        {
                            if (item.name.StartsWith("Exposure"))
                            {
                                for (int i = 0; i < item.parameterList.Count; i++)
                                {
                                    if (item.parameterList[i].GetIl2CppType() == Il2CppType.Of<AdaptationModeParameter>())
                                    {
                                        var param = item.parameterList[i].Cast<AdaptationModeParameter>();
                                        param.value = AdaptationMode.Fixed;
                                        item.parameterList[i] = param;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                foreach (var obj in UnityEngine.Object.FindObjectsOfTypeAll(Il2CppType.Of<Canvas>()))
                {
                    var canvas = obj.TryCast<Canvas>();

                    if (canvas is not null)
                    {
                        if (canvas.name != "lowres")
                        {
                            canvas.enabled = !canvas.enabled;
                            canvas.enabled = !canvas.enabled;
                        }
                    }
                }
            }
        }
    }
}
