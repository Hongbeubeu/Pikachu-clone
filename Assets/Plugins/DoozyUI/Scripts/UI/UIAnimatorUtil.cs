// Copyright (c) 2015 - 2018 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using UnityEngine;
using System;
using QuickEngine.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DoozyUI
{
    [Serializable]
    public class UIAnimatorUtil
    {
        public const string UNCATEGORIZED_CATEGORY_NAME = "Uncategorized";
        public const string DEFAULT_PRESET_NAME = "DefaultPreset";

        public const string FOLDER_NAME_IN = "In/";
        public const string FOLDER_NAME_OUT = "Out/";
        public const string FOLDER_NAME_STATE = "State/";
        public const string FOLDER_NAME_LOOP = "Loop/";
        public const string FOLDER_NAME_PUNCH = "Punch/";

        public const string RESOURCES_PATH_ANIMATIONS = "DUI/Animations/";
        public const string RESOURCES_PATH_IN_ANIM_DATA = RESOURCES_PATH_ANIMATIONS + FOLDER_NAME_IN;
        public const string RESOURCES_PATH_OUT_ANIM_DATA = RESOURCES_PATH_ANIMATIONS + FOLDER_NAME_OUT;
        public const string RESOURCES_PATH_STATE_ANIM_DATA = RESOURCES_PATH_ANIMATIONS + FOLDER_NAME_STATE;
        public const string RESOURCES_PATH_LOOP_DATA = RESOURCES_PATH_ANIMATIONS + FOLDER_NAME_LOOP;
        public const string RESOURCES_PATH_PUNCH_DATA = RESOURCES_PATH_ANIMATIONS + FOLDER_NAME_PUNCH;

        public static string RELATIVE_PATH_ANIMATIONS { get { return DUI.PATH + "/Resources/DUI/Animations/"; } }
        public static string RELATIVE_PATH_IN_ANIM_DATA { get { return RELATIVE_PATH_ANIMATIONS + FOLDER_NAME_IN; } }
        public static string RELATIVE_PATH_OUT_ANIM_DATA { get { return RELATIVE_PATH_ANIMATIONS + FOLDER_NAME_OUT; } }
        public static string RELATIVE_PATH_STATE_ANIM_DATA { get { return RELATIVE_PATH_ANIMATIONS + FOLDER_NAME_STATE; } }
        public static string RELATIVE_PATH_LOOP_DATA { get { return RELATIVE_PATH_ANIMATIONS + FOLDER_NAME_LOOP; } }
        public static string RELATIVE_PATH_PUNCH_DATA { get { return RELATIVE_PATH_ANIMATIONS + FOLDER_NAME_PUNCH; } }

        private static string[] GetInAnimPresetsDirectories { get { return QuickEngine.IO.File.GetDirectoriesNames(RELATIVE_PATH_IN_ANIM_DATA); } }
        private static string[] GetOutAnimPresetsDirectories { get { return QuickEngine.IO.File.GetDirectoriesNames(RELATIVE_PATH_OUT_ANIM_DATA); } }


        private static string[] GetInAnimPresetsNamesForCategory(string presetCategory) { return QuickEngine.IO.File.GetFilesNames(RELATIVE_PATH_IN_ANIM_DATA + presetCategory + "/", "asset"); }
        private static string[] GetOutAnimPresetsNamesForCategory(string presetCategory) { return QuickEngine.IO.File.GetFilesNames(RELATIVE_PATH_OUT_ANIM_DATA + presetCategory + "/", "asset"); }

        public static T GetResource<T>(string resourcesPath, string fileName) where T : ScriptableObject
        {
            return (T)Resources.Load(resourcesPath + fileName, typeof(T));
        }

        public static Anim GetInAnim(string presetCategory, string presetName)
        {
            return Q.GetResource<AnimData>(RESOURCES_PATH_IN_ANIM_DATA + presetCategory + "/", presetName).data.Copy();
        }
        public static Anim GetOutAnim(string presetCategory, string presetName)
        {
            return Q.GetResource<AnimData>(RESOURCES_PATH_OUT_ANIM_DATA + presetCategory + "/", presetName).data.Copy();
        }
        public static Anim GetStateAnim(string presetCategory, string presetName)
        {
            return Q.GetResource<AnimData>(RESOURCES_PATH_STATE_ANIM_DATA + presetCategory + "/", presetName).data.Copy();
        }
        public static Loop GetLoop(string presetCategory, string presetName)
        {
            return Q.GetResource<LoopData>(RESOURCES_PATH_LOOP_DATA + presetCategory + "/", presetName).data.Copy();
        }
        public static Punch GetPunch(string presetCategory, string presetName)
        {
            return Q.GetResource<PunchData>(RESOURCES_PATH_PUNCH_DATA + presetCategory + "/", presetName).data.Copy();
        }

#if UNITY_EDITOR
        public static Dictionary<string, List<AnimData>> InAnimDataPresetsDatabase;
        public static Dictionary<string, List<AnimData>> OutAnimDataPresetsDatabase;

        private static List<string> directories;
        private static string[] fileNames;

        private static int count;
        private static int len;


        public static void RefreshInAnimDataPresetsDatabase()
        {
            EditorUtility.DisplayProgressBar("Refreshing In Animations Database", "", 0f);
            if (InAnimDataPresetsDatabase == null) { InAnimDataPresetsDatabase = new Dictionary<string, List<AnimData>>(); }
            InAnimDataPresetsDatabase.Clear();
            if (directories == null) { directories = new List<string>(); }
            directories.Clear();
            directories = RemoveEmptyPresetFolders(RELATIVE_PATH_IN_ANIM_DATA, GetInAnimPresetsDirectories);
            count = directories != null ? directories.Count : 0;
            for (int directoryIndex = 0; directoryIndex < count; directoryIndex++)
            {
                EditorUtility.DisplayProgressBar("Refreshing In Animations Database", directories[directoryIndex], ((directoryIndex + 1) / (count + 2)));
                fileNames = GetInAnimPresetsNamesForCategory(directories[directoryIndex]);
                len = fileNames != null ? fileNames.Length : 0;
                if (len == 0) { continue; } //empty folder
                InAnimDataPresetsDatabase.Add(directories[directoryIndex], new List<AnimData>());
                for (int fileIndex = 0; fileIndex < len; fileIndex++)
                {
                    AnimData asset = GetResource<AnimData>(RESOURCES_PATH_IN_ANIM_DATA + directories[directoryIndex] + "/", fileNames[fileIndex]);
                    if (asset == null) { continue; }
                    EditorUtility.DisplayProgressBar("Refreshing In Animations Database", directories[directoryIndex] + " / " + asset.presetName, ((directoryIndex + 1) / (count + 2)));
                    InAnimDataPresetsDatabase[directories[directoryIndex]].Add(asset);
                }
            }
            EditorUtility.DisplayProgressBar("Refreshing In Animations Database", "Creating Categories List...", 0.9f);
            RefreshInAnimPresetCategories();
            EditorUtility.DisplayProgressBar("Refreshing In Animations Database", "Validating...", 1f);
            ValidateInAnimPresets();
            EditorUtility.ClearProgressBar();
        }
        public static void RefreshOutAnimDataPresetsDatabase()
        {
            EditorUtility.DisplayProgressBar("Refreshing Out Animations Database", "", 0f);
            if (OutAnimDataPresetsDatabase == null) { OutAnimDataPresetsDatabase = new Dictionary<string, List<AnimData>>(); }
            OutAnimDataPresetsDatabase.Clear();
            if (directories == null) { directories = new List<string>(); }
            directories.Clear();
            directories = RemoveEmptyPresetFolders(RELATIVE_PATH_OUT_ANIM_DATA, GetOutAnimPresetsDirectories);
            count = directories != null ? directories.Count : 0;
            for (int directoryIndex = 0; directoryIndex < count; directoryIndex++)
            {
                EditorUtility.DisplayProgressBar("Refreshing Out Animations Database", directories[directoryIndex], ((directoryIndex + 1) / (count + 2)));
                fileNames = GetOutAnimPresetsNamesForCategory(directories[directoryIndex]);
                len = fileNames != null ? fileNames.Length : 0;
                if (len == 0) { continue; } //empty folder
                OutAnimDataPresetsDatabase.Add(directories[directoryIndex], new List<AnimData>());
                for (int fileIndex = 0; fileIndex < len; fileIndex++)
                {
                    AnimData asset = GetResource<AnimData>(RESOURCES_PATH_OUT_ANIM_DATA + directories[directoryIndex] + "/", fileNames[fileIndex]);
                    if (asset == null) { continue; }
                    EditorUtility.DisplayProgressBar("Refreshing Out Animations Database", directories[directoryIndex] + " / " + asset.presetName, ((directoryIndex + 1) / (count + 2)));
                    OutAnimDataPresetsDatabase[directories[directoryIndex]].Add(asset);
                }
            }
            EditorUtility.DisplayProgressBar("Refreshing Out Animations Database", "Creating Categories List...", 0.9f);
            RefreshOutAnimPresetCategories();
            EditorUtility.DisplayProgressBar("Refreshing Out Animations Database", "Validating...", 1f);
            ValidateOutAnimPresets();
            EditorUtility.ClearProgressBar();
        }

        private static List<string> m_InAnimPresetCategories;
        public static List<string> InAnimPresetCategories { get { if (m_InAnimPresetCategories == null) { RefreshInAnimPresetCategories(); } return m_InAnimPresetCategories; } }
        private static void RefreshInAnimPresetCategories()
        {
            if (m_InAnimPresetCategories == null) { m_InAnimPresetCategories = new List<string>(); }
            m_InAnimPresetCategories.Clear();
            m_InAnimPresetCategories.AddRange(GetInAnimPresetCategories());
        }
        private static List<string> m_OutAnimPresetCategories;
        public static List<string> OutAnimPresetCategories { get { if (m_OutAnimPresetCategories == null) { RefreshOutAnimPresetCategories(); } return m_OutAnimPresetCategories; } }
        private static void RefreshOutAnimPresetCategories()
        {
            if (m_OutAnimPresetCategories == null) { m_OutAnimPresetCategories = new List<string>(); }
            m_OutAnimPresetCategories.Clear();
            m_OutAnimPresetCategories.AddRange(GetOutAnimPresetCategories());
        }

        private static List<string> GetInAnimPresetCategories()
        {
            if (InAnimDataPresetsDatabase == null) { RefreshInAnimDataPresetsDatabase(); }
            return new List<string>(InAnimDataPresetsDatabase.Keys);
        }
        private static List<string> GetOutAnimPresetCategories()
        {
            if (OutAnimDataPresetsDatabase == null) { RefreshOutAnimDataPresetsDatabase(); }
            return new List<string>(OutAnimDataPresetsDatabase.Keys);
        }

        public static List<string> GetInAnimPresetNames(string presetCategory)
        {
            if (InAnimDataPresetsDatabase == null) { RefreshInAnimDataPresetsDatabase(); }
            if (!InAnimPresetCategoryExists(presetCategory)) { return null; }
            List<string> presetNames = new List<string>();
            foreach (var presetData in InAnimDataPresetsDatabase[presetCategory]) { presetNames.Add(presetData.presetName); }
            return presetNames;
        }
        public static List<string> GetOutAnimPresetNames(string presetCategory)
        {
            if (OutAnimDataPresetsDatabase == null) { RefreshOutAnimDataPresetsDatabase(); }
            if (!OutAnimPresetCategoryExists(presetCategory)) { return null; }
            List<string> presetNames = new List<string>();
            foreach (var presetData in OutAnimDataPresetsDatabase[presetCategory]) { presetNames.Add(presetData.presetName); }
            return presetNames;
        }

        public static bool InAnimPresetCategoryExists(string presetCategory)
        {
            return InAnimPresetCategories.Contains(presetCategory);
        }
        public static bool OutAnimPresetCategoryExists(string presetCategory)
        {
            return OutAnimPresetCategories.Contains(presetCategory);
        }

        public static bool InAnimPresetExists(string presetCategory, string presetName)
        {
            if (!InAnimPresetCategoryExists(presetCategory)) { return false; }
            return GetInAnimPresetNames(presetCategory).Contains(presetName);
        }
        public static bool OutAnimPresetExists(string presetCategory, string presetName)
        {
            if (!OutAnimPresetCategoryExists(presetCategory)) { return false; }
            return GetOutAnimPresetNames(presetCategory).Contains(presetName);
        }

        private static AnimData CreateAnimDataAsset(string relativePath, string presetCategory, string presetName, Anim anim)
        {
            AnimData asset = ScriptableObject.CreateInstance<AnimData>();
            asset.presetName = presetName;
            asset.presetCategory = presetCategory;
            asset.data = anim;
            if (!QuickEngine.IO.File.Exists(relativePath + presetCategory + "/"))
            {
                QuickEngine.IO.File.CreateDirectory(relativePath + presetCategory + "/");
            }
            AssetDatabase.CreateAsset(asset, relativePath + presetCategory + "/" + presetName + ".asset");
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }


        public static AnimData CreateInAnimPreset(string presetCategory, string presetName, Anim anim)
        {
            return CreateAnimDataAsset(RELATIVE_PATH_IN_ANIM_DATA, presetCategory, presetName, anim);
        }
        public static AnimData CreateOutAnimPreset(string presetCategory, string presetName, Anim anim)
        {
            return CreateAnimDataAsset(RELATIVE_PATH_OUT_ANIM_DATA, presetCategory, presetName, anim);
        }

        private static List<string> RemoveEmptyPresetFolders(string relativePath, string[] directories)
        {
            List<string> list = new List<string>();
            list.AddRange(directories);
            bool refreshAssetsDatabase = false;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (QuickEngine.IO.File.GetFilesNames(relativePath + list[i] + "/", "asset").Length == 0) //this is an empty folder -> delete it
                {
                    FileUtil.DeleteFileOrDirectory(relativePath + list[i]);
                    list.RemoveAt(i);
                    refreshAssetsDatabase = true;
                }
            }
            if (refreshAssetsDatabase) { AssetDatabase.Refresh(); }
            return list;
        }

        /// <summary>
        /// Checks that there are not null presets (animation data), that the preset category is matched to the directory structure and that the preset name is matched to the file name
        /// </summary>
        private static void ValidateInAnimPresets()
        {
            bool refreshDatabase = false;
            bool assetDatabaseSaveAssets = false;
            bool assetDatabaseRefresh = false;
            directories = new List<string>();
            directories = RemoveEmptyPresetFolders(RELATIVE_PATH_IN_ANIM_DATA, GetInAnimPresetsDirectories);
            if (!directories.Contains(UNCATEGORIZED_CATEGORY_NAME) || !InAnimPresetExists(UNCATEGORIZED_CATEGORY_NAME, DEFAULT_PRESET_NAME)) //the default preset category does not exist in the database or the default preset name does not exist in the database-> create it
            {
                refreshDatabase = true;
                CreateInAnimPreset(UNCATEGORIZED_CATEGORY_NAME, DEFAULT_PRESET_NAME, new Anim(Anim.AnimationType.In));
            }
            for (int directoryIndex = 0; directoryIndex < directories.Count; directoryIndex++)
            {
                fileNames = GetInAnimPresetsNamesForCategory(directories[directoryIndex]);
                if (fileNames.Length == 0) { continue; } //empty folder
                for (int fileIndex = 0; fileIndex < fileNames.Length; fileIndex++)
                {
                    AnimData asset = GetResource<AnimData>(RESOURCES_PATH_IN_ANIM_DATA + directories[directoryIndex] + "/", fileNames[fileIndex]);
                    if (asset == null) { continue; }
                    if (asset.data == null) //preset data is null (this is the animation data) -> this should not happen -> delete the preset to avoid corruption
                    {
                        refreshDatabase = true;
                        if (AssetDatabase.MoveAssetToTrash(RELATIVE_PATH_IN_ANIM_DATA + directories[directoryIndex] + "/" + fileNames[fileIndex] + ".asset")) //move asset file to trash
                        {
                            assetDatabaseSaveAssets = true;
                            assetDatabaseRefresh = true;
                        }
                    }
                    if (string.IsNullOrEmpty(asset.presetName) || !asset.presetName.Equals(fileNames[fileIndex])) //preset name is empty or preset name does not match the file name -> set the preset name as the file name
                    {
                        refreshDatabase = true;
                        asset.presetName = fileNames[fileIndex];
                        assetDatabaseSaveAssets = true;
                    }
                    if (string.IsNullOrEmpty(asset.presetCategory) || !asset.presetCategory.Equals(directories[directoryIndex])) //preset category is empty or preset category does not match the directory name-> set the preset category as the directory name 
                    {
                        refreshDatabase = true;
                        asset.presetCategory = directories[directoryIndex];
                        assetDatabaseSaveAssets = true;
                    }
                }
            }
            if (assetDatabaseSaveAssets) { AssetDatabase.SaveAssets(); }
            if (assetDatabaseRefresh) { AssetDatabase.Refresh(); }
            if (refreshDatabase) { RefreshInAnimDataPresetsDatabase(); }
        }
        /// <summary>
        /// Checks that there are not null presets (animation data), that the preset category is matched to the directory structure and that the preset name is matched to the file name
        /// </summary>
        private static void ValidateOutAnimPresets()
        {
            bool refreshDatabase = false;
            bool assetDatabaseSaveAssets = false;
            bool assetDatabaseRefresh = false;
            directories = new List<string>();
            directories = RemoveEmptyPresetFolders(RELATIVE_PATH_OUT_ANIM_DATA, GetOutAnimPresetsDirectories);
            if (!directories.Contains(UNCATEGORIZED_CATEGORY_NAME) || !OutAnimPresetExists(UNCATEGORIZED_CATEGORY_NAME, DEFAULT_PRESET_NAME)) //the default preset category does not exist in the database or the default preset name does not exist in the database-> create it
            {
                refreshDatabase = true;
                CreateOutAnimPreset(UNCATEGORIZED_CATEGORY_NAME, DEFAULT_PRESET_NAME, new Anim(Anim.AnimationType.Out));
            }
            for (int directoryIndex = 0; directoryIndex < directories.Count; directoryIndex++)
            {
                fileNames = GetOutAnimPresetsNamesForCategory(directories[directoryIndex]);
                if (fileNames.Length == 0) { continue; } //empty folder
                for (int fileIndex = 0; fileIndex < fileNames.Length; fileIndex++)
                {
                    AnimData asset = GetResource<AnimData>(RESOURCES_PATH_OUT_ANIM_DATA + directories[directoryIndex] + "/", fileNames[fileIndex]);
                    if (asset == null) { continue; }
                    if (asset.data == null) //preset data is null (this is the animation data) -> this should not happen -> delete the preset to avoid corruption
                    {
                        refreshDatabase = true;
                        if (AssetDatabase.MoveAssetToTrash(RELATIVE_PATH_OUT_ANIM_DATA + directories[directoryIndex] + "/" + fileNames[fileIndex] + ".asset")) //move asset file to trash
                        {
                            assetDatabaseSaveAssets = true;
                            assetDatabaseRefresh = true;
                        }
                    }
                    if (string.IsNullOrEmpty(asset.presetName) || !asset.presetName.Equals(fileNames[fileIndex])) //preset name is empty or preset name does not match the file name -> set the preset name as the file name
                    {
                        refreshDatabase = true;
                        asset.presetName = fileNames[fileIndex];
                        assetDatabaseSaveAssets = true;
                    }
                    if (string.IsNullOrEmpty(asset.presetCategory) || !asset.presetCategory.Equals(directories[directoryIndex])) //preset category is empty or preset category does not match the directory name-> set the preset category as the directory name 
                    {
                        refreshDatabase = true;
                        asset.presetCategory = directories[directoryIndex];
                        assetDatabaseSaveAssets = true;
                    }
                }
            }
            if (assetDatabaseSaveAssets) { AssetDatabase.SaveAssets(); }
            if (assetDatabaseRefresh) { AssetDatabase.Refresh(); }
            if (refreshDatabase) { RefreshOutAnimDataPresetsDatabase(); }
        }

#endif
    }
}
