﻿using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

namespace XSToon
{
    public class XSToonInspector : ShaderGUI
    {
        BindingFlags bindingFlags = BindingFlags.Public |
                                    BindingFlags.NonPublic |
                                    BindingFlags.Instance |
                                    BindingFlags.Static;

        //Assign all properties as null at first to stop hundreds of warnings spamming the log when script gets compiled.
        //If they aren't we get warnings, because assigning with reflection seems to make Unity think that the properties never actually get used.
        MaterialProperty _VertexColorAlbedo = null;
        MaterialProperty _TilingMode = null;
        MaterialProperty _Culling = null;
        MaterialProperty _BlendMode = null;
        MaterialProperty _MainTex = null;
        MaterialProperty _HSVMask = null;
        MaterialProperty _Saturation = null;
        MaterialProperty _Hue = null;
        MaterialProperty _Value = null;
        MaterialProperty _Color = null;
        MaterialProperty _Cutoff = null;
        MaterialProperty _FadeDither = null;
        MaterialProperty _FadeDitherDistance = null;
        MaterialProperty _BumpMap = null;
        MaterialProperty _BumpScale = null;
        MaterialProperty _DetailNormalMap = null;
        MaterialProperty _DetailMask = null;
        MaterialProperty _DetailNormalMapScale = null;
        MaterialProperty _ReflectionMode = null;
        MaterialProperty _ReflectionBlendMode = null;
        MaterialProperty _MetallicGlossMap = null;
        MaterialProperty _BakedCubemap = null;
        MaterialProperty _Matcap = null;
        MaterialProperty _MatcapTintToDiffuse = null;
        MaterialProperty _MatcapTint = null;
        MaterialProperty _ReflectivityMask = null;
        MaterialProperty _Metallic = null;
        MaterialProperty _Glossiness = null;
        MaterialProperty _Reflectivity = null;
        MaterialProperty _ClearCoat = null;
        MaterialProperty _ClearcoatStrength = null;
        MaterialProperty _ClearcoatSmoothness = null;
        MaterialProperty _EmissionMap = null;
        MaterialProperty _ScaleWithLight = null;
        MaterialProperty _ScaleWithLightSensitivity = null;
        MaterialProperty _EmissionColor = null;
        MaterialProperty _EmissionToDiffuse = null;
        MaterialProperty _RimColor = null;
        MaterialProperty _RimIntensity = null;
        MaterialProperty _RimRange = null;
        MaterialProperty _RimThreshold = null;
        MaterialProperty _RimSharpness = null;
        MaterialProperty _RimAlbedoTint = null;
        MaterialProperty _RimCubemapTint = null;
        MaterialProperty _RimAttenEffect = null;
        MaterialProperty _SpecularSharpness = null;
        MaterialProperty _SpecularMap = null;
        MaterialProperty _SpecularIntensity = null;
        MaterialProperty _SpecularArea = null;
        MaterialProperty _AnisotropicSpecular = null;
        MaterialProperty _AnisotropicReflection = null;
        MaterialProperty _SpecularAlbedoTint = null;
        MaterialProperty _RampSelectionMask = null;
        MaterialProperty _Ramp = null;
        MaterialProperty _ShadowRim = null;
        MaterialProperty _ShadowRimRange = null;
        MaterialProperty _ShadowRimThreshold = null;
        MaterialProperty _ShadowRimSharpness = null;
        MaterialProperty _ShadowRimAlbedoTint = null;
        MaterialProperty _OcclusionMap = null;
        MaterialProperty _OcclusionIntensity = null;
        MaterialProperty _OcclusionMode = null;
        MaterialProperty _ThicknessMap = null;
        MaterialProperty _SSColor = null;
        MaterialProperty _SSDistortion = null;
        MaterialProperty _SSPower = null;
        MaterialProperty _SSScale = null;
        MaterialProperty _HalftoneDotSize = null;
        MaterialProperty _HalftoneDotAmount = null;
        MaterialProperty _HalftoneLineAmount = null;
        MaterialProperty _HalftoneLineIntensity = null;
        MaterialProperty _HalftoneType = null;
        MaterialProperty _UVSetAlbedo = null;
        MaterialProperty _UVSetNormal = null;
        MaterialProperty _UVSetDetNormal = null;
        MaterialProperty _UVSetDetMask = null;
        MaterialProperty _UVSetMetallic = null;
        MaterialProperty _UVSetSpecular = null;
        MaterialProperty _UVSetReflectivity = null;
        MaterialProperty _UVSetThickness = null;
        MaterialProperty _UVSetOcclusion = null;
        MaterialProperty _UVSetEmission = null;
        MaterialProperty _UVSetClipMap = null;
        MaterialProperty _Stencil = null;
        MaterialProperty _StencilComp = null;
        MaterialProperty _StencilOp = null;
        MaterialProperty _OutlineAlbedoTint = null;
        MaterialProperty _OutlineLighting = null;
        MaterialProperty _OutlineMask = null;
        MaterialProperty _OutlineWidth = null;
        MaterialProperty _OutlineColor = null;
        MaterialProperty _OutlineNormalMode = null;
        MaterialProperty _OutlineUVSelect = null;
        MaterialProperty _ShadowSharpness = null;
        MaterialProperty _AdvMode = null;
        MaterialProperty _ClipMap = null;
        MaterialProperty _ClipAgainstVertexColorGreaterZeroFive = null;
        MaterialProperty _ClipAgainstVertexColorLessZeroFive = null;
        MaterialProperty _IOR = null;
        MaterialProperty _UseRefraction = null;
        MaterialProperty _RefractionModel = null;
        MaterialProperty _NormalMapMode = null;

        //Material Properties for Patreon Plugins
        MaterialProperty _LeftRightPan = null;
        MaterialProperty _UpDownPan = null;
        MaterialProperty _Twitchyness = null;
        MaterialProperty _AttentionSpan = null;
        MaterialProperty _FollowPower = null;
        MaterialProperty _FollowLimit = null;
        MaterialProperty _LookSpeed = null;
        MaterialProperty _IrisSize = null;
        MaterialProperty _EyeOffsetLimit = null;
        //--

        static bool showMainSettings = true;
        static bool showNormalMapSettings = false;
        static bool showShadows = true;
        static bool showSpecular = false;
        static bool showReflection = false;
        static bool showRimlight = false;
        static bool showHalftones = false;
        static bool showSubsurface = false;
        static bool showOutlines = false;
        static bool showEmission = false;
        static bool showAdvanced = false;
        static bool showEyeTracking = false;
        static bool showRefractionSettings = false;

        bool isPatreonShader = false;
        bool isEyeTracking = false;
        bool isOutlined = false;
        bool isCutout = false;
        bool isCutoutMasked = false;
        bool isDithered = false;
        bool isRefractive = false;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            Material material = materialEditor.target as Material;
            Shader shader = material.shader;

            isCutout = material.GetInt("_BlendMode") == 1;
            isDithered = material.GetInt("_BlendMode") == 2;
            isRefractive = material.GetInt("_UseRefraction") == 1;
            isOutlined = shader.name.Contains("Outline");
            isPatreonShader = shader.name.Contains("Patreon");
            isEyeTracking = shader.name.Contains("EyeTracking");


            //Find all material properties listed in the script using reflection, and set them using a loop only if they're of type MaterialProperty.
            //This makes things a lot nicer to maintain and cleaner to look at.
            foreach (var property in GetType().GetFields(bindingFlags))
            {
                if (property.FieldType == typeof(MaterialProperty))
                {
                    try { property.SetValue(this, FindProperty(property.Name, props)); } catch { /*Is it really a problem if it doesn't exist?*/ }
                }
            }

            EditorGUI.BeginChangeCheck();
            {
                XSStyles.ShurikenHeaderCentered("XSToon v" + XSStyles.ver);
                material.SetShaderPassEnabled("Always", isRefractive);
                materialEditor.ShaderProperty(_AdvMode, new GUIContent("Shader Mode", "Setting this to 'Advanced' will give you access to things such as stenciling, and other expiremental/advanced features."));
                materialEditor.ShaderProperty(_Culling, new GUIContent("Culling Mode", "Changes the culling mode. 'Off' will result in a two sided material, while 'Front' and 'Back' will cull those sides respectively"));
                materialEditor.ShaderProperty(_TilingMode, new GUIContent("Tiling Mode", "Setting this to Merged will tile and offset all textures based on the Main texture's Tiling/Offset."));
                materialEditor.ShaderProperty(_BlendMode, new GUIContent("Blend Mode", "Blend mode of the material. (Opaque, transparent, cutout, etc.)"));
                materialEditor.ShaderProperty(_UseRefraction, new GUIContent("Refraction", "Should this material be refractive? (Warning, this can be expensive!)"));

                DoBlendModeSettings(material);
                DrawMainSettings(materialEditor);
                DrawShadowSettings(materialEditor, material);
                DrawOutlineSettings(materialEditor);
                DrawNormalSettings(materialEditor);
                DrawSpecularSettings(materialEditor);
                DrawReflectionsSettings(materialEditor, material);
                DrawRefractionSettings(materialEditor);
                DrawEmissionSettings(materialEditor);
                DrawRimlightSettings(materialEditor);
                DrawHalfToneSettings(materialEditor);
                DrawTransmissionSettings(materialEditor);
                DrawAdvancedSettings(materialEditor);
                DrawPatreonSettings(materialEditor);
                XSStyles.DoFooter();
            }
        }

        private void DoBlendModeSettings(Material material)
        {
            int mode = material.GetInt("_BlendMode");
            switch (mode)
            {
                case 0: //Opaque
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One, (int)UnityEngine.Rendering.BlendMode.Zero, (int)UnityEngine.Rendering.RenderQueue.Geometry, 1, 0);
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHATEST_ON");
                    break;

                case 1: //Cutout
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One, (int)UnityEngine.Rendering.BlendMode.Zero, (int)UnityEngine.Rendering.RenderQueue.AlphaTest, 1, 0);
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    break;

                case 2: //Dithered
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One, (int)UnityEngine.Rendering.BlendMode.Zero, (int)UnityEngine.Rendering.RenderQueue.AlphaTest, 1, 0);
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    break;

                case 3: //Alpha To Coverage
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One, (int)UnityEngine.Rendering.BlendMode.Zero, (int)UnityEngine.Rendering.RenderQueue.AlphaTest, 1, 1);
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    break;

                case 4: //Transparent
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, (int)UnityEngine.Rendering.RenderQueue.Transparent, 0, 0);
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHATEST_ON");
                    break;

                case 5: //Fade
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.SrcAlpha, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, (int)UnityEngine.Rendering.RenderQueue.Transparent, 0, 0);
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHATEST_ON");
                    break;

                case 6: //Additive
                    SetBlend(material, (int)UnityEngine.Rendering.BlendMode.One, (int)UnityEngine.Rendering.BlendMode.One, (int)UnityEngine.Rendering.RenderQueue.Transparent, 0, 0);
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHATEST_ON");
                    break;
            }
        }

        private void SetBlend(Material material, int src, int dst, int renderQueue, int zwrite, int alphatocoverage)
        {
            material.SetInt("_SrcBlend", src);
            material.SetInt("_DstBlend", dst);
            material.SetInt("_ZWrite", zwrite);
            material.SetInt("_AlphaToCoverage", alphatocoverage);

            material.renderQueue = isRefractive ? (int)UnityEngine.Rendering.RenderQueue.Overlay - 1 : renderQueue;
        }

        private void DrawMainSettings(MaterialEditor materialEditor)
        {
            showMainSettings = XSStyles.ShurikenFoldout("Main Settings", showMainSettings);
            if (showMainSettings)
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture", "The main Albedo texture."), _MainTex, _Color);
                if (isCutout)
                {
                    materialEditor.ShaderProperty(_Cutoff, new GUIContent("Cutoff", "The Cutoff Amount"), 2);
                }
                materialEditor.ShaderProperty(_UVSetAlbedo, new GUIContent("UV Set", "The UV set to use for the Albedo Texture."), 2);
                materialEditor.TextureScaleOffsetProperty(_MainTex);

                materialEditor.TexturePropertySingleLine(new GUIContent("HSV Mask", "RGB Mask: R = Hue,  G = Saturation, B = Brightness"), _HSVMask);
                materialEditor.ShaderProperty(_Hue, new GUIContent("Hue", "Controls Hue of the final output from the shader."));
                materialEditor.ShaderProperty(_Saturation, new GUIContent("Saturation", "Controls saturation of the final output from the shader."));
                materialEditor.ShaderProperty(_Value, new GUIContent("Brightness", "Controls value of the final output from the shader."));

                if (isDithered)
                {
                    XSStyles.SeparatorThin();
                    materialEditor.ShaderProperty(_FadeDither, new GUIContent("Distance Fading", "Make the shader dither out based on the distance to the camera."));
                    materialEditor.ShaderProperty(_FadeDitherDistance, new GUIContent("Fade Threshold", "The distance at which the fading starts happening."));
                }
            }
        }

        private void DrawShadowSettings(MaterialEditor materialEditor, Material material)
        {
            showShadows = XSStyles.ShurikenFoldout("Shadows", showShadows);
            if (showShadows)
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Ramp Selection Mask", "A black to white mask that determins how far up on the multi ramp to sample. 0 for bottom, 1 for top, 0.5 for middle, 0.25, and 0.75 for mid bottom and mid top respectively."), _RampSelectionMask);

                XSStyles.SeparatorThin();
                if (_RampSelectionMask.textureValue != null)
                {
                    string rampMaskPath = AssetDatabase.GetAssetPath(_RampSelectionMask.textureValue);
                    TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(rampMaskPath);
                    if (ti.sRGBTexture)
                    {
                        if (XSStyles.HelpBoxWithButton(new GUIContent("This texture is not marked as Linear.", "This is recommended for the mask"), new GUIContent("Fix Now")))
                        {
                            ti.sRGBTexture = false;
                            AssetDatabase.ImportAsset(rampMaskPath, ImportAssetOptions.ForceUpdate);
                            AssetDatabase.Refresh();
                        }
                    }
                }

                materialEditor.TexturePropertySingleLine(new GUIContent("Shadow Ramp", "Shadow Ramp, Dark to Light should be Left to Right"), _Ramp);
                materialEditor.ShaderProperty(_ShadowSharpness, new GUIContent("Shadow Sharpness", "Controls the sharpness of recieved shadows, as well as the sharpness of 'shadows' from Vertex Lighting."));

                XSStyles.SeparatorThin();
                materialEditor.ShaderProperty(_OcclusionMode, new GUIContent("Occlusion Mode", "How to calculate the occlusion map contribution"));
                materialEditor.TexturePropertySingleLine(new GUIContent("Occlusion Map", "Occlusion Map, used to darken areas on the model artifically."), _OcclusionMap);
                materialEditor.ShaderProperty(_OcclusionIntensity, new GUIContent("Intensity", "Occlusion intensity"), 2);
                materialEditor.ShaderProperty(_UVSetOcclusion, new GUIContent("UV Set", "The UV set to use for the Occlusion Texture"), 2);
                materialEditor.TextureScaleOffsetProperty(_OcclusionMap);

                XSStyles.SeparatorThin();
                XSStyles.constrainedShaderProperty(materialEditor, _ShadowRim, new GUIContent("Shadow Rim", "Shadow Rim Color. Set to white to disable."), 0);
                materialEditor.ShaderProperty(_ShadowRimAlbedoTint, new GUIContent("Shadow Rim Albedo Tint", "How much the Albedo texture should effect the Shadow Rim color."));
                materialEditor.ShaderProperty(_ShadowRimRange, new GUIContent("Range", "Range of the Shadow Rim"), 2);
                materialEditor.ShaderProperty(_ShadowRimThreshold, new GUIContent("Threshold", "Threshold of the Shadow Rim"), 2);
                materialEditor.ShaderProperty(_ShadowRimSharpness, new GUIContent("Sharpness", "Sharpness of the Shadow Rim"), 2);
                XSStyles.callGradientEditor(material);
            }
        }

        private void DrawOutlineSettings(MaterialEditor materialEditor)
        {
            if (isOutlined)
            {
                showOutlines = XSStyles.ShurikenFoldout("Outlines", showOutlines);
                if (showOutlines)
                {
                    materialEditor.ShaderProperty(_OutlineNormalMode, new GUIContent("Outline Normal Mode", "How to calcuate the outline expand direction. Using mesh normals may result in split edges."));

                    if (_OutlineNormalMode.floatValue == 2)
                        materialEditor.ShaderProperty(_OutlineUVSelect, new GUIContent("Normals UV", "UV Channel to pull the modified normals from for outlines."));

                    materialEditor.ShaderProperty(_OutlineLighting, new GUIContent("Outline Lighting", "Makes outlines respect the lighting, or be emissive."));
                    materialEditor.ShaderProperty(_OutlineAlbedoTint, new GUIContent("Outline Albedo Tint", "Includes the color of the Albedo Texture in the calculation for the color of the outline."));
                    materialEditor.TexturePropertySingleLine(new GUIContent("Outline Mask", "Outline width mask, black will make the outline minimum width."), _OutlineMask);
                    materialEditor.ShaderProperty(_OutlineWidth, new GUIContent("Outline Width", "Width of the Outlines"));
                    XSStyles.constrainedShaderProperty(materialEditor, _OutlineColor, new GUIContent("Outline Color", "Color of the outlines"), 0);
                }
            }
        }

        private void DrawNormalSettings(MaterialEditor materialEditor)
        {
            showNormalMapSettings = XSStyles.ShurikenFoldout("Normal Maps", showNormalMapSettings);
            if (showNormalMapSettings)
            {
                materialEditor.ShaderProperty(_NormalMapMode, new GUIContent("Normal Map Source", "How to alter the normals of the mesh, using which source?"));
                if (_NormalMapMode.floatValue == 0)
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map"), _BumpMap);
                    materialEditor.ShaderProperty(_BumpScale, new GUIContent("Normal Strength", "Strength of the main Normal Map"), 2);
                    materialEditor.ShaderProperty(_UVSetNormal, new GUIContent("UV Set", "The UV set to use for the Normal Map"), 2);
                    materialEditor.TextureScaleOffsetProperty(_BumpMap);

                    XSStyles.SeparatorThin();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Detail Normal Map", "Detail Normal Map"), _DetailNormalMap);
                    materialEditor.ShaderProperty(_DetailNormalMapScale, new GUIContent("Detail Normal Strength", "Strength of the detail Normal Map"), 2);
                    materialEditor.ShaderProperty(_UVSetDetNormal, new GUIContent("UV Set", "The UV set to use for the Detail Normal Map"), 2);
                    materialEditor.TextureScaleOffsetProperty(_DetailNormalMap);

                    XSStyles.SeparatorThin();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Detail Mask", "Mask for Detail Maps"), _DetailMask);
                    materialEditor.ShaderProperty(_UVSetDetMask, new GUIContent("UV Set", "The UV set to use for the Detail Mask"), 2);
                    materialEditor.TextureScaleOffsetProperty(_DetailMask);
                }
            }
        }

        private void DrawSpecularSettings(MaterialEditor materialEditor)
        {
            showSpecular = XSStyles.ShurikenFoldout("Specular", showSpecular);
            if (showSpecular)
            {
                XSStyles.SeparatorThin();

                materialEditor.TexturePropertySingleLine(new GUIContent("Specular Map(R,G,B)", "Specular Map. Red channel controls Intensity, Green controls how much specular is tinted by Albedo, and Blue controls Smoothness (Only for Blinn-Phong, and GGX)."), _SpecularMap);
                materialEditor.TextureScaleOffsetProperty(_SpecularMap);
                materialEditor.ShaderProperty(_UVSetSpecular, new GUIContent("UV Set", "The UV set to use for the Specular Map"), 2);
                materialEditor.ShaderProperty(_SpecularIntensity, new GUIContent("Intensity", "Specular Intensity."), 2);
                materialEditor.ShaderProperty(_SpecularArea, new GUIContent("Roughness", "Roughness"), 2);
                materialEditor.ShaderProperty(_SpecularAlbedoTint, new GUIContent("Albedo Tint", "How much the specular highlight should derive color from the albedo of the object."), 2);
                materialEditor.ShaderProperty(_SpecularSharpness, new GUIContent("Sharpness", "How hard of and edge transitions should the specular have?"), 2);
                materialEditor.ShaderProperty(_AnisotropicSpecular, new GUIContent("Anisotropy", "The amount of anisotropy the surface has - this will stretch the reflection along an axis (think bottom of a frying pan)"), 2);
            }
        }

        private void DrawReflectionsSettings(MaterialEditor materialEditor, Material material)
        {
            showReflection = XSStyles.ShurikenFoldout("Reflections", showReflection);
            if (showReflection)
            {
                materialEditor.ShaderProperty(_ReflectionMode, new GUIContent("Reflection Mode", "Reflection Mode."));

                if (_ReflectionMode.floatValue == 0) // PBR
                {
                    materialEditor.ShaderProperty(_ReflectionBlendMode, new GUIContent("Reflection Blend Mode", "Blend mode for reflection. Additive is Color + reflection, Multiply is Color * reflection, and subtractive is Color - reflection"));
                    materialEditor.ShaderProperty(_ClearCoat, new GUIContent("Clearcoat", "Clearcoat"));

                    XSStyles.SeparatorThin();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Fallback Cubemap", " Used as fallback in 'Unity' reflection mode if reflection probe is black."), _BakedCubemap);

                    XSStyles.SeparatorThin();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Metallic Map", "Metallic Map, Metallic on Red Channel, Smoothness on Alpha Channel. \nIf Clearcoat is enabled, Clearcoat Smoothness on Green Channel, Clearcoat Reflectivity on Blue Channel."), _MetallicGlossMap);
                    materialEditor.TextureScaleOffsetProperty(_MetallicGlossMap);
                    materialEditor.ShaderProperty(_UVSetMetallic, new GUIContent("UV Set", "The UV set to use for the Metallic Smoothness Map"), 2);
                    materialEditor.ShaderProperty(_AnisotropicReflection, new GUIContent("Anisotropy", "The amount of anisotropy the surface has - this will stretch the reflection along an axis (think bottom of a frying pan)"), 2);
                    materialEditor.ShaderProperty(_Metallic, new GUIContent("Metallic", "Metallic, set to 1 if using metallic map"), 2);
                    materialEditor.ShaderProperty(_Glossiness, new GUIContent("Smoothness", "Smoothness, set to 1 if using metallic map"), 2);
                    materialEditor.ShaderProperty(_ClearcoatSmoothness, new GUIContent("Clearcoat Smoothness", "Smoothness of the clearcoat."), 2);
                    materialEditor.ShaderProperty(_ClearcoatStrength, new GUIContent("Clearcoat Reflectivity", "The strength of the clearcoat reflection."), 2);
                }
                else if (_ReflectionMode.floatValue == 1) //Baked cube
                {
                    materialEditor.ShaderProperty(_ReflectionBlendMode, new GUIContent("Reflection Blend Mode", "Blend mode for reflection. Additive is Color + reflection, Multiply is Color * reflection, and subtractive is Color - reflection"));
                    materialEditor.ShaderProperty(_ClearCoat, new GUIContent("Clearcoat", "Clearcoat"));

                    XSStyles.SeparatorThin();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Baked Cubemap", "Baked cubemap."), _BakedCubemap);

                    XSStyles.SeparatorThin();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Metallic Map", "Metallic Map, Metallic on Red Channel, Smoothness on Alpha Channel. \nIf Clearcoat is enabled, Clearcoat Smoothness on Green Channel, Clearcoat Reflectivity on Blue Channel."), _MetallicGlossMap);
                    materialEditor.TextureScaleOffsetProperty(_MetallicGlossMap);
                    materialEditor.ShaderProperty(_UVSetMetallic, new GUIContent("UV Set", "The UV set to use for the MetallicSmoothness Map"), 2);
                    materialEditor.ShaderProperty(_AnisotropicReflection, new GUIContent("Anisotropic", "Anisotropic, stretches reflection in an axis."), 2);
                    materialEditor.ShaderProperty(_Metallic, new GUIContent("Metallic", "Metallic, set to 1 if using metallic map"), 2);
                    materialEditor.ShaderProperty(_Glossiness, new GUIContent("Smoothness", "Smoothness, set to 1 if using metallic map"), 2);
                    materialEditor.ShaderProperty(_ClearcoatSmoothness, new GUIContent("Clearcoat Smoothness", "Smoothness of the clearcoat."), 2);
                    materialEditor.ShaderProperty(_ClearcoatStrength, new GUIContent("Clearcoat Reflectivity", "The strength of the clearcoat reflection."), 2);
                }
                else if (_ReflectionMode.floatValue == 2) //Matcap
                {
                    materialEditor.ShaderProperty(_ReflectionBlendMode, new GUIContent("Reflection Blend Mode", "Blend mode for reflection. Additive is Color + reflection, Multiply is Color * reflection, and subtractive is Color - reflection"));

                    XSStyles.SeparatorThin();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Matcap", "Matcap Texture"), _Matcap, _MatcapTint);
                    materialEditor.ShaderProperty(_Glossiness, new GUIContent("Matcap Blur", "Matcap blur, blurs the Matcap, set to 1 for full clarity"), 2);
                    materialEditor.ShaderProperty(_MatcapTintToDiffuse, new GUIContent("Tint To Diffuse", "Tints matcap to diffuse color."), 2);
                    material.SetFloat("_Metallic", 0);
                    material.SetFloat("_ClearCoat", 0);
                    material.SetTexture("_MetallicGlossMap", null);
                }
                if (_ReflectionMode.floatValue != 3)
                {
                    XSStyles.SeparatorThin();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Reflectivity Mask", "Mask for reflections."), _ReflectivityMask);
                    materialEditor.TextureScaleOffsetProperty(_ReflectivityMask);
                    materialEditor.ShaderProperty(_UVSetReflectivity, new GUIContent("UV Set", "The UV set to use for the Reflectivity Mask"), 2);
                    materialEditor.ShaderProperty(_Reflectivity, new GUIContent("Reflectivity", "The strength of the reflections."), 2);
                }
                if (_ReflectionMode.floatValue == 3)
                {
                    material.SetFloat("_Metallic", 0);
                    material.SetFloat("_ReflectionBlendMode", 0);
                    material.SetFloat("_ClearCoat", 0);
                }
            }
        }

        private void DrawEmissionSettings(MaterialEditor materialEditor)
        {
            showEmission = XSStyles.ShurikenFoldout("Emission", showEmission);
            if (showEmission)
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Emission Map", "Emissive map. White to black, unless you want multiple colors."), _EmissionMap, _EmissionColor);
                materialEditor.TextureScaleOffsetProperty(_EmissionMap);
                materialEditor.ShaderProperty(_UVSetEmission, new GUIContent("UV Set", "The UV set to use for the Emission Map"), 2);
                materialEditor.ShaderProperty(_EmissionToDiffuse, new GUIContent("Tint To Diffuse", "Tints the emission to the Diffuse Color"), 2);

                XSStyles.SeparatorThin();
                materialEditor.ShaderProperty(_ScaleWithLight, new GUIContent("Scale w/ Light", "Scales the emission intensity based on how dark or bright the environment is."));
                if (_ScaleWithLight.floatValue == 0)
                    materialEditor.ShaderProperty(_ScaleWithLightSensitivity, new GUIContent("Scaling Sensitivity", "How agressively the emission should scale with the light."));
            }
        }

        private void DrawRimlightSettings(MaterialEditor materialEditor)
        {
            showRimlight = XSStyles.ShurikenFoldout("Rimlight", showRimlight);
            if (showRimlight)
            {
                materialEditor.ShaderProperty(_RimColor, new GUIContent("Rimlight Tint", "The Tint of the Rimlight."));
                materialEditor.ShaderProperty(_RimAlbedoTint, new GUIContent("Rim Albedo Tint", "How much the Albedo texture should effect the rimlight color."));
                materialEditor.ShaderProperty(_RimCubemapTint, new GUIContent("Rim Environment Tint", "How much the Environment cubemap should effect the rimlight color."));
                materialEditor.ShaderProperty(_RimAttenEffect, new GUIContent("Rim Attenuation Effect", "How much should realtime shadows mask out the rimlight?"));
                materialEditor.ShaderProperty(_RimIntensity, new GUIContent("Rimlight Intensity", "Strength of the Rimlight."));
                materialEditor.ShaderProperty(_RimRange, new GUIContent("Range", "Range of the Rim"), 2);
                materialEditor.ShaderProperty(_RimThreshold, new GUIContent("Threshold", "Threshold of the Rim"), 2);
                materialEditor.ShaderProperty(_RimSharpness, new GUIContent("Sharpness", "Sharpness of the Rim"), 2);
            }
        }

        private void DrawHalfToneSettings(MaterialEditor materialEditor)
        {
            showHalftones = XSStyles.ShurikenFoldout("Halftones", showHalftones);
            if (showHalftones)
            {
                materialEditor.ShaderProperty(_HalftoneType, new GUIContent("Halftone Style", "Controls where halftone and stippling effects are drawn."));

                if (_HalftoneType.floatValue == 1 || _HalftoneType.floatValue == 2)
                {
                    materialEditor.ShaderProperty(_HalftoneDotSize, new GUIContent("Stippling Scale", "How large should the stippling pattern be?"));
                    materialEditor.ShaderProperty(_HalftoneDotAmount, new GUIContent("Stippling Density", "How dense is the stippling effect?"));
                }

                if (_HalftoneType.floatValue == 0 || _HalftoneType.floatValue == 2)
                {
                    materialEditor.ShaderProperty(_HalftoneLineAmount, new GUIContent("Halftone Line Count", "How many lines should the halftone shadows have?"));
                    materialEditor.ShaderProperty(_HalftoneLineIntensity, new GUIContent("Halftone Line Intensity", "How dark should the halftone lines be?"));
                }
            }
        }

        private void DrawTransmissionSettings(MaterialEditor materialEditor)
        {
            showSubsurface = XSStyles.ShurikenFoldout("Transmission", showSubsurface);
            if (showSubsurface)
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Thickness Map", "Thickness Map, used to mask areas where transmission can happen"), _ThicknessMap);
                materialEditor.TextureScaleOffsetProperty(_ThicknessMap);
                materialEditor.ShaderProperty(_UVSetThickness, new GUIContent("UV Set", "The UV set to use for the Thickness Map"), 2);
                XSStyles.constrainedShaderProperty(materialEditor, _SSColor, new GUIContent("Transmission Color", "Transmission Color"), 2);
                materialEditor.ShaderProperty(_SSDistortion, new GUIContent("Transmission Distortion", "How much the Transmission should follow the normals of the mesh and/or normal map."), 2);
                materialEditor.ShaderProperty(_SSPower, new GUIContent("Transmission Power", "Subsurface Power"), 2);
                materialEditor.ShaderProperty(_SSScale, new GUIContent("Transmission Scale", "Subsurface Scale"), 2);
            }
        }

        private void DrawRefractionSettings(MaterialEditor materialEditor)
        {
            if (isRefractive)
            {
                showRefractionSettings = XSStyles.ShurikenFoldout("Refraction", showRefractionSettings);
                if (showRefractionSettings)
                {
                    materialEditor.ShaderProperty(_RefractionModel, new GUIContent("Refraction Model", "Refraction technique"));
                    materialEditor.ShaderProperty(_IOR, new GUIContent("Index of Refraction", "The index of refraction of the material. Glass: 1.5, Crystal: 2.0, Ice: 1.309, Water: 1.325"));
                }
            }
        }

        private void DrawAdvancedSettings(MaterialEditor materialEditor)
        {
            if (_AdvMode.floatValue == 1)
            {
                showAdvanced = XSStyles.ShurikenFoldout("Advanced Settings", showAdvanced);
                if (showAdvanced)
                {
                    materialEditor.ShaderProperty(_VertexColorAlbedo, new GUIContent("Vertex Color Albedo", "Multiplies the vertex color of the mesh by the Albedo texture to derive the final Albedo color."));
                    if (isDithered || isCutout)
                    {
                        materialEditor.TexturePropertySingleLine(new GUIContent("Clip Map (RGBA)", "Used to control clipping in an advanced manner, read tooltip for Clip Mask Vectors below."), _ClipMap);
                        materialEditor.TextureScaleOffsetProperty(_ClipMap);
                        materialEditor.ShaderProperty(_UVSetClipMap, new GUIContent("UV Set", "The UV set to use for the Clip Map"), 2);
                        materialEditor.ShaderProperty(_ClipAgainstVertexColorGreaterZeroFive, new GUIContent("Clip Mask > 0.5 Opacity", "Uses the Clip Map RGBA channels as a multiplier for clipping."));
                        _ClipAgainstVertexColorGreaterZeroFive.vectorValue = ClampVec4(_ClipAgainstVertexColorGreaterZeroFive.vectorValue);
                        materialEditor.ShaderProperty(_ClipAgainstVertexColorLessZeroFive, new GUIContent("Clip Mask Color < 0.5 Opacity", "Uses the Clip Map RGBA channels as a multiplier for clipping."));
                        _ClipAgainstVertexColorLessZeroFive.vectorValue = ClampVec4(_ClipAgainstVertexColorLessZeroFive.vectorValue);
                    }

                    materialEditor.ShaderProperty(_Stencil, _Stencil.displayName);
                    materialEditor.ShaderProperty(_StencilComp, _StencilComp.displayName);
                    materialEditor.ShaderProperty(_StencilOp, _StencilOp.displayName);

                    materialEditor.RenderQueueField();
                }
            }
        }

        private void DrawPatreonSettings(MaterialEditor materialEditor)
        {
            //Plugins for Patreon releases
            if (isPatreonShader)
            {
                if (isEyeTracking)
                {
                    showEyeTracking = XSStyles.ShurikenFoldout("Eye Tracking Settings", showEyeTracking);
                    if (showEyeTracking)
                    {
                        materialEditor.ShaderProperty(_LeftRightPan, new GUIContent("Left Right Adj.", "Adjusts the eyes manually left or right."));
                        materialEditor.ShaderProperty(_UpDownPan, new GUIContent("Up Down Adj.", "Adjusts the eyes manually up or down."));

                        XSStyles.SeparatorThin();
                        materialEditor.ShaderProperty(_AttentionSpan, new GUIContent("Attention Span", "How often should the eyes look at the target; 0 = never, 1 = always, 0.5 = half of the time."));
                        materialEditor.ShaderProperty(_FollowPower, new GUIContent("Follow Power", "The influence the target has on the eye"));
                        materialEditor.ShaderProperty(_LookSpeed, new GUIContent("Look Speed", "How fast the eye transitions to looking at the target"));
                        materialEditor.ShaderProperty(_Twitchyness, new GUIContent("Refocus Frequency", "How much should the eyes look around near the target?"));

                        XSStyles.SeparatorThin();
                        materialEditor.ShaderProperty(_IrisSize, new GUIContent("Iris Size", "Size of the iris"));
                        materialEditor.ShaderProperty(_FollowLimit, new GUIContent("Follow Limit", "Limits the angle from the front of the face on how far the eyes can track/rotate."));
                        materialEditor.ShaderProperty(_EyeOffsetLimit, new GUIContent("Offset Limit", "Limit for how far the eyes can turn"));
                    }
                }
            }
            //
        }

        private Vector4 ClampVec4(Vector4 vec)
        {
            Vector4 value = vec;
            value.x = Mathf.Clamp01(value.x);
            value.y = Mathf.Clamp01(value.y);
            value.z = Mathf.Clamp01(value.z);
            value.w = Mathf.Clamp01(value.w);
            return value;
        }
    }
}