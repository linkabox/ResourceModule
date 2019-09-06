
#if USE_FMOD
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceModule
{
    public class FMODLoader
    {
		public static string FMODResDir = Application.persistentDataPath + "/fmodres/";

        public delegate void OnLoadBank(bool isOk);

        public static bool LoadBank(string bankName, bool loadSamples = false)
        {
            return LoadAllBank(new[] { bankName }, loadSamples);
        }

        public static bool LoadAllBank(string[] banks, bool loadSamples = false)
        {
            if (_LoadBank(banks, loadSamples))
            {
                FMODUnity.RuntimeManager.WaitForAllLoads();
                return true;
            }
            return false;
        }

        public static void LoadBankAsync(string bankName, bool loadSamples = false, OnLoadBank callback = null)
        {
            LoadAllBankAsync(new[] { bankName }, loadSamples, callback);
        }

        public static void LoadAllBankAsync(string[] banks, bool loadSamples = false, OnLoadBank onFinish = null)
        {
            if (_LoadBank(banks, loadSamples))
            {
                ResManager.Instance.StartCoroutine(_LoadCoroutine(onFinish));
            }
            else
            {
                if (onFinish != null) onFinish(false);
            }
        }

        private static bool _LoadBank(string[] banks, bool loadSamples)
        {
            try
            {
                if (ResManager.IsEdiotrMode && Application.isEditor)
                {
#if UNITY_EDITOR
                    foreach (var bank in banks)
                    {
                        FMODUnity.RuntimeManager.LoadBank(bank, loadSamples);
                    }
#else
                    Debug.LogErrorFormat("`IsEditorLoadAsset` is Unity Editor only");
#endif
                }
                else
                {
                    foreach (var bank in banks)
                    {
						string bankFile = FMODResDir + bank + ".bank";
                        FMODUnity.RuntimeManager.LoadBankFile(bank, bankFile, loadSamples);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        private static IEnumerator _LoadCoroutine(OnLoadBank onFinish)
        {
            // Keep yielding the co-routine until all the Bank loading is done
            while (FMODUnity.RuntimeManager.AnyBankLoading())
            {
                yield return null;
            }

            if (onFinish != null) onFinish(true);
        }

        public static void UnloadAllBank(string[] banks)
        {
            foreach (var bankName in banks)
            {
                UnloadBank(bankName);
            }
        }

        public static void UnloadBank(string bankName)
        {
            FMODUnity.RuntimeManager.UnloadBank(bankName);
        }
    }
}
#endif