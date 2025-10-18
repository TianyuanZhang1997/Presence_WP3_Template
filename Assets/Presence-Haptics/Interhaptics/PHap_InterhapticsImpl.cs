/*
 * Interhaptics implementation of the Presence Haptics API. Contains stubs for Interhaptics Devs to implement.
 * 
 * author:
 * max@senseglove.com
 */


// Define to enable / disable anything to do with the Interhaptics Package. Comment / Uncomment to disable / enable
// Intherhaptics Core SDK Crashes if you run the project without a Meta Quest attached via Oculus Link.
// Remove the package if you want to test without needing a Quest attached to my system. In that case, uncomment this define.
#define IH_PACKAGE_PRESENT 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if IH_PACKAGE_PRESENT
using Interhaptics.Core;
using Interhaptics.HapticBodyMapping; // Added for BodyPartE
using Interhaptics;
using Interhaptics.Platforms.Sensa;
#endif

namespace Presence
{
    //[CreateAssetMenu(fileName = "PHap_Interhaptics", menuName = "Presence/Implementations/Interhaptics", order = 1)]
    public class PHap_InterhapticsImpl : PHap_DeviceImplementationHelper
    {
        //--------------------------------------------------------------------------------------------------------------------------------------------------------
        // Constants and Variables

        private static string exePath = "Assets/Presence-Haptics/Interhaptics/Editor/Transcoder.exe";





        //--------------------------------------------------------------------------------------------------------------------------------------------------------
        // PHap_Device Implementation


        public override string GetImplementationID()
        {
            return "PHap_Interhaptics";
        }

        public override bool SupportsHapticsType(PHap_HapticModality effectType)
        {
            switch (effectType)
            {
                case PHap_HapticModality.Vibrotactile:
                    return true;
                //TODO: Add additional supported format(s).
                default:
                    return false;
            }
        }

        public override bool SupportsHapticLocation(PHap_BodyPart onBodyPart)
        {
            switch (onBodyPart)
            {
                case PHap_BodyPart.LeftHead:
                case PHap_BodyPart.RightHead:
                case PHap_BodyPart.LeftHandPalm:
                case PHap_BodyPart.RightHandPalm:
                case PHap_BodyPart.RightChest:
                case PHap_BodyPart.LeftChest:
                case PHap_BodyPart.RightWaist:
                case PHap_BodyPart.LeftWaist:
                case PHap_BodyPart.RightUpperLeg:
                case PHap_BodyPart.LeftUpperLeg:
                    return true;
                default:
                    return false;
            }
        }

        public override bool SupportsHaptics(PHap_HapticModality effectType, PHap_BodyPart bodyPart)
        {
            return SupportsHapticsType(effectType) && SupportsHapticLocation(bodyPart); //Note: This may not give the intended result if you have mixed effects (eg force and vibrotactile on different fingers)
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------
        // Runtime Implementation


        //Note: I've noticed that the Interhaptics API Calls do not work, unless I the Sample first. this indicates that something is loaded within that sample...



        public override bool Initialize()
        {
#if IH_PACKAGE_PRESENT
            HAR.Init();
#else
            Debug.LogError("Interhaptics Pacake not included in this project! Make sure it is included, and you've uncommented the IH_PACKAGE_PRESENT define at the top of this file", this);
#endif
            return true;
            //return base.Initialize();
        }

        public override bool Deinitialize()
        {
#if IH_PACKAGE_PRESENT
            HAR.Quit();
#endif
            return true;
            //return base.Deinitialize();
        }

        public override bool DeviceConnected()
        {
            
            //TODO: Find out how to check for valid interhaptics device(s). For now, since we're using it for controller vibration, we simply check for those. I can use the SenseGlove plugin for a cheesy method
            if (SG.SG_XR_Devices.GetHandDevice(true, out UnityEngine.XR.InputDevice rightController))
                return true;
            if (SG.SG_XR_Devices.GetHandDevice(false, out UnityEngine.XR.InputDevice leftController))
                return true;
            // For Freyja, we might need a more specific check if available from Interhaptics or RazerSensaProvider
#if IH_PACKAGE_PRESENT
            var provider = new Interhaptics.Platforms.Sensa.RazerSensaProvider();
            if (provider.IsPresent()) { 
                //Debug.Log("Interhaptics Device is present.");
                return true;
            }
#endif
            return false;
        }



        public override bool PlayHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
#if IH_PACKAGE_PRESENT
            //Debug.Log($"PHap_InterhapticsImpl: Received request for {location.BodyPart}. Mapping to Interhaptics.");
            if (!m_loadedMaterials.ContainsKey(effect))
            {
                Debug.LogWarning($"Haptic material for effect '{effect.name}' not loaded.", effect);
                return false;
            }

            HapticMaterial IHMat = m_loadedMaterials[effect];

            GroupID targetBodyPart;
            LateralFlag l;
            switch (location.BodyPart)
            {
                case PHap_BodyPart.RightHead:
                    targetBodyPart = GroupID.Head;
                    l = LateralFlag.Right;
                    break;
                case PHap_BodyPart.LeftHead:
                    targetBodyPart = GroupID.Head;
                    l = LateralFlag.Left;
                    break;
                case PHap_BodyPart.RightHandPalm:
                    targetBodyPart = GroupID.Palm;
                    l = LateralFlag.Right; 
                    break;
                case PHap_BodyPart.LeftHandPalm:
                    targetBodyPart = GroupID.Palm; ;
                    l = LateralFlag.Left;
                    break;
                case PHap_BodyPart.RightChest:
                    targetBodyPart = GroupID.Chest;
                    l = LateralFlag.Right;
                    break;
                case PHap_BodyPart.LeftChest:
                    targetBodyPart = GroupID.Chest;
                    l = LateralFlag.Left;
                    break;
                case PHap_BodyPart.RightWaist:
                    targetBodyPart = GroupID.Waist;
                    l = LateralFlag.Right;
                    break;
                case PHap_BodyPart.LeftWaist:
                    targetBodyPart = GroupID.Waist;
                    l = LateralFlag.Left;
                    break;
                case PHap_BodyPart.RightUpperLeg:
                    targetBodyPart = GroupID.Upper_leg;
                    l = LateralFlag.Right;
                    break;
                case PHap_BodyPart.LeftUpperLeg:
                    targetBodyPart = GroupID.Upper_leg;
                    l = LateralFlag.Left;
                    break;
                default:
                    Debug.LogWarning($"Unsupported body part for Interhaptics playback: {location.BodyPart}");
                    return false;
            }
            
            Debug.Log($"PHap_InterhapticsImpl: Playing Interhaptics effect '{effect.name}' on {location.BodyPart} (mapped to IH GroupID: {targetBodyPart}, Lateral: {l})");
            HAR.PlayHapticEffect(IHMat, effect.Intensity, effect.RepeatAmount, 0f, l, targetBodyPart);
            return true;
#else
            return false;
#endif
        }


        public override bool StopHapticEffect(PHap_HapticEffect effect, PHap_EffectLocation location)
        {
            if (effect.BaseEffect.GetEffectType() != PHap_HapticModality.Vibrotactile)
                return false;

#if IH_PACKAGE_PRESENT
            if ( !m_loadedMaterials.ContainsKey(effect) )
                return false;

            // Stopping might need to be body-part specific if HAR.StopCurrentHapticEffect(HmatID) is too general
            // or if multiple instances of the same material can play on different body parts.
            // For now, using the existing logic.
            int HmatID = HAR.AddHM(m_loadedMaterials[effect]) -1; // This re-adds the material, which is likely not intended for stopping.
                                                                  // Consider storing the ID when playing or using a method that stops based on material and/or bodypart.
            HAR.StopCurrentHapticEffect(HmatID); // This stops based on an ID. If the ID is not correctly managed, stopping might fail.
            // A more robust stop might be HAR.StopFx(m_loadedMaterials[effect]); if such an API exists and is suitable.
            Debug.LogWarning("Interhaptics StopHapticEffect might need review for robustly stopping effects, especially for specific body parts or managed IDs.");
            return true;
#else
            return false;
#endif
        }



        //--------------------------------------------------------------------------------------------------------------------------------------------------------
        // Loading / Unloading materials


        public override bool LoadEffect(PHap_HapticEffect effect)
        {
#if IH_PACKAGE_PRESENT
            if (m_loadedMaterials.ContainsKey(effect)) //already loaded
            {
                //Debug.LogError("Material has no key"); // This log seems incorrect for "already loaded"
                Debug.Log($"Effect '{effect.name}' already loaded for Interhaptics.");
                return true; // Already loaded, so success.
            }
                

            if (effect.BaseEffect.GetEffectType() != PHap_HapticModality.Vibrotactile)
            {
               // Debug.Log("Wrong effect type, expecting Vibrotictile but it is " + effect.BaseEffect.GetEffectType());
                return false;
            }

            string filePath;
            if (!effect.BaseEffect.GetEffectPath(this, out filePath))
            {
                Debug.Log("Invalid filePath");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("Invalid filePath! Empty or NULL");
                return false;
            }
#if UNITY_EDITOR //outside of the editor, this filepath should be loaded from Resources, which I do from TryLoadScriptableObject()
            string directoryPath = System.IO.Path.GetDirectoryName(filePath);
            if (!System.IO.Directory.Exists(directoryPath))
            {
                Debug.Log("The Directory " + directoryPath + " does not exist!");
                return false;
            }
#endif
            if (!filePath.ToLower().EndsWith(".haps"))
            {
                Debug.Log(filePath + ": Invalid FileType! Expected .haps but got " + System.IO.Path.GetExtension(filePath));
                return false;
            }
            //HapticMaterial extands off ScriptableObject, so it must be located in the Resources folder (at Build time)
            HapticMaterial IHmat;
            if (!PHap_Util.TryLoadScriptableObject(filePath, out IHmat))
            {
                Debug.LogError("Unable to load " + filePath + " as HapticMaterial!");
                return false; //false if we can't load the material
            }
            m_loadedMaterials.Add(effect, IHmat);
            Debug.Log("Loaded " + effect.name + " as HapticMaterial");
            return true;
#else
            return false;
#endif
        }


        public override bool UnloadEffect(PHap_HapticEffect effect)
        {
#if IH_PACKAGE_PRESENT
            return m_loadedMaterials.Remove(effect);
#else
            return false;
#endif
        }

#if IH_PACKAGE_PRESENT
        private Dictionary<PHap_HapticEffect, HapticMaterial> m_loadedMaterials = new Dictionary<PHap_HapticEffect, HapticMaterial>();
#endif


        //--------------------------------------------------------------------------------------------------------------------------------------------------------
        // Transcoding (Editor Only)


        public override bool IsMyCustomFormat(string customFilePath)
        {
            return customFilePath.EndsWith(".haps");
        }


        public override bool CustomFormat_ToHjif(string customFilePath, string hjifOutputDir, string fileName, out string hjifFilePath)
        {
            //Debug.Log(this.GetImplementationID() + ": CustomFormat_ToHjif(" + customFilePath + ", " + hjifOutputDir + ", " + fileName + "out string hjifFilePath);", this);
            string outputFilePath_hjif = System.IO.Path.Combine(hjifOutputDir, fileName + ".hjif");
            hjifFilePath = Haps_to_Hjif(customFilePath, outputFilePath_hjif);
            return hjifFilePath.Length > 0;
        }

        public override bool Hjif_ToCustomFormat(string hjifFilePath, PHap_BaseEffectMetaData metaData, string effectName, string targetDirectory, out string decodedPath, out string additionalData)
        {
            //Debug.Log(this.name + ": Hjif_ToCustomFormat");

            //Since we load .haps files as scriptableObjects, they should be in the Resources folder...
            PHap_Transcoding.CheckOutputFolder(PHap_Transcoding.TranscodeOutputFolder_Resources);
            string outputFilePath_haps = System.IO.Path.Combine(PHap_Transcoding.TranscodeOutputFolder_Resources, effectName + ".haps");
            decodedPath = Hjif_to_Haps(hjifFilePath, outputFilePath_haps);
            additionalData = "";
            return decodedPath.Length > 0;
        }


        /// <summary>  </summary>
        /// <param name="inputFilePath_hjif"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string Hjif_to_Haps(string inputFilePath_hjif, string outputFilePath_haps)
        {
            if (!outputFilePath_haps.ToLower().EndsWith(".haps"))
            {
                return "";
            }
            try
            {
                // Create the process start info
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = exePath;
                startInfo.Arguments = $"-f ./{inputFilePath_hjif} -o ./{outputFilePath_haps}";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                // Start the process
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();

                    // Read the output (optional)
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    // Log the output and error (optional)
                    if (!string.IsNullOrEmpty(output))
                    {
                        UnityEngine.Debug.Log("PHap_InterhapticsImpl: " + output);
                    }
                    if (!string.IsNullOrEmpty(error))
                    {
                        UnityEngine.Debug.LogError("PHap_InterhapticsImpl: " + error);
                    }
                }

                //UnityEngine.Debug.Log("Should now have an asset called " + outputFilePath_haps);
                if (System.IO.File.Exists(outputFilePath_haps))
                    return outputFilePath_haps;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
            }
            return "";
        }



        /// <summary>  </summary>
        /// <param name="inputFilePath_haps"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string Haps_to_Hjif(string inputFilePath_haps, string outputFilePath_hjif) //TODO: Move OutputPath to Resources instead of StreamingAssets?
        {
            if (!outputFilePath_hjif.ToLower().EndsWith(".hjif"))
            {
                return "";
            }
            //if (inputFilePath_haps.Contains(" "))
            //{
            //    Debug.LogError("Interhaptics Implementation cannot currently transcode files containing a SPACE \" \". Please rename your file / folder(s) until Interhaptics resolves this issue.");
            //    //return "";
            //}
            try
            {
                string inp = "\"" + inputFilePath_haps + "\"";
                string outp = "\"" + outputFilePath_hjif + "\"";

                // Create the process start info
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = exePath;
                startInfo.Arguments = $"-f ./{inp} -o ./{outp}";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                // Start the process
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();

                    // Read the output (optional)
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    // Log the output and error (optional)
                    if (!string.IsNullOrEmpty(output))
                    {
                        UnityEngine.Debug.Log(output);
                    }
                    if (!string.IsNullOrEmpty(error))
                    {
                        UnityEngine.Debug.LogError(error);
                    }
                }

                //UnityEngine.Debug.Log("Should now have an asset called " + outputFilePath_hjif);
                if (System.IO.File.Exists(outputFilePath_hjif))
                    return outputFilePath_hjif;
                //else something died.
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
            }
            return "";
        }

        public override PHap_BaseEffectMetaData FallbackGetEffectParameters(string customFilePath)
        {
            PHap_BaseEffectMetaData metaData = new PHap_BaseEffectMetaData();
            metaData.SetModality(PHap_HapticModality.Vibrotactile);
            metaData.Duration = 1.0f;
            return metaData;
        }
    }
}