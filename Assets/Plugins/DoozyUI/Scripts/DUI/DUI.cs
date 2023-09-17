// Copyright (c) 2015 - 2018 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections.Generic;
using System;
using QuickEngine.Extensions;
using QuickEngine.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DoozyUI
{
    [Serializable]
    public partial class DUI
    {
        #region Context Menu Settings

        public const int MENU_PRIORITY_UIELEMENT = 1;
        public const string COMPONENT_MENU_UIELEMENT = "DoozyUI/UIElement";
        public const string GAMEOBJECT_MENU_UIELEMENT = "GameObject/DoozyUI/UIElement";

        public const int MENU_PRIORITY_UIBUTTON = 2;
        public const string COMPONENT_MENU_UIBUTTON = "DoozyUI/UIButton";
        public const string GAMEOBJECT_MENU_UIBUTTON = "GameObject/DoozyUI/UIButton";

        #endregion

        public const string UNCATEGORIZED_CATEGORY_NAME = "Uncategorized";
        public const string DEFAULT_ELEMENT_NAME = "~Element Name~";

        public const string DEFAULT_BUTTON_NAME = "~Button Name~";

        public const string BACK_BUTTON_NAME = "Back";
        public const string CUSTOM_NAME = "~Custom Name~";

        public const int GLOBAL_EDITOR_WIDTH = 420;
        public const int BAR_HEIGHT = 20;
        public const int MINI_BAR_HEIGHT = 18;
        public enum EventType { ButtonClick }
        public const string CANVAS_DATABASE_FILENAME = "CanvasDatabase";

        private static string _DOOZYUI_PATH = "";
        public static string PATH
        {
            get
            {
                if(_DOOZYUI_PATH.IsNullOrEmpty())
                {
                    _DOOZYUI_PATH = QuickEngine.IO.File.GetRelativeDirectoryPath("DoozyUI");
                }
                return _DOOZYUI_PATH;
            }
        }

        public const string RESOURCES_PATH_DUIDATA = "";
        public static string RELATIVE_PATH_DUIDATA { get { return PATH + "/Resources/" + RESOURCES_PATH_DUIDATA; } }

        public const string RESOURCES_PATH_UIELEMENTS = "DUI/UIElements/";
        public const string RESOURCES_PATH_UIBUTTONS = "DUI/UIButtons/";
        public const string RESOURCES_PATH_CANVAS_DATABASE = "DUI/Canvases/";

        public static string RELATIVE_PATH_UIELEMENTS { get { return PATH + "/Resources/" + RESOURCES_PATH_UIELEMENTS; } }
        public static string RELATIVE_PATH_UIBUTTONS { get { return PATH + "/Resources/" + RESOURCES_PATH_UIBUTTONS; } }
        public static string RELATIVE_PATH_CANVAS_DATABASE { get { return PATH + "/Resources/" + RESOURCES_PATH_CANVAS_DATABASE; } }

        public const string RESOURCES_PATH_SETTINGS = "DUI/Settings/";
        public static string RELATIVE_PATH_SETTINGS { get { return PATH + "/Resources/" + RESOURCES_PATH_SETTINGS; } }
        public const string SETTINGS_FILENAME = "DUISettings";

        private static string[] GetUIButtonCategoriesFileNames { get { return QuickEngine.IO.File.GetFilesNames(RELATIVE_PATH_UIBUTTONS, "asset"); } }

        public static T GetResource<T>(string resourcesPath, string fileName) where T : ScriptableObject
        {
            return (T)Resources.Load(resourcesPath + fileName, typeof(T));
        }

        private static DUISettings _DUISettings;
        public static DUISettings DUISettings
        {
            get
            {
                if (_DUISettings == null)
                {
#if UNITY_EDITOR
                    if (!AssetDatabase.IsValidFolder(DUI.PATH + "/Resources")) { AssetDatabase.CreateFolder(DUI.PATH, "Resources"); }
                    if (!AssetDatabase.IsValidFolder(DUI.PATH + "/Resources/DUI")) { AssetDatabase.CreateFolder(DUI.PATH + "/Resources", "DUI"); }
                    if (!AssetDatabase.IsValidFolder(DUI.PATH + "/Resources/DUI/Settings")) { AssetDatabase.CreateFolder(DUI.PATH + "/Resources/DUI", "Settings"); }
#endif
                    _DUISettings = Q.GetResource<DUISettings>(RESOURCES_PATH_SETTINGS, SETTINGS_FILENAME);
                }

#if UNITY_EDITOR && !dUI_SOURCE
                if (_DUISettings == null)
                {
                    _DUISettings = Q.CreateAsset<DUISettings>(RELATIVE_PATH_SETTINGS, SETTINGS_FILENAME);
                }
#endif
                return _DUISettings;
            }
        }

#if UNITY_EDITOR
        public static T CreateAsset<T>(string relativePath, string fileName, string extension = ".asset") where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, relativePath + fileName + extension);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        public static Dictionary<string, NamesDatabase> UIButtonsDatabase;
        public static NamesDatabase CanvasNamesDatabase;

        public static NamesDatabase GetUIButtonsDatabase(string category)
        {
            return GetResource<NamesDatabase>(RESOURCES_PATH_UIBUTTONS, category);
        }
        public static NamesDatabase GetCanvasNamesDatabase()
        {
            return GetResource<NamesDatabase>(RESOURCES_PATH_CANVAS_DATABASE, CANVAS_DATABASE_FILENAME);
        }

        private static string[] fileNames;

        public static void RefreshUIButtonsDatabase()
        {
            EditorUtility.DisplayProgressBar("Refreshing UIButtons Database", "", 0f);
            if(UIButtonsDatabase == null) { UIButtonsDatabase = new Dictionary<string, NamesDatabase>(); }
            UIButtonsDatabase.Clear();
            fileNames = null;
            fileNames = GetUIButtonCategoriesFileNames;
            int fileNamesLength = fileNames != null ? fileNames.Length : 0;
            for(int fileIndex = 0; fileIndex < fileNamesLength; fileIndex++)
            {
                EditorUtility.DisplayProgressBar("Refreshing UIButtons Database", fileNames[fileIndex], ((fileIndex + 1) / (fileNamesLength + 2)));
                UIButtonsDatabase.Add(fileNames[fileIndex], GetUIButtonsDatabase(fileNames[fileIndex]));
            }
            EditorUtility.DisplayProgressBar("Refreshing UIButtons Database", "Creating Categories List...", 0.9f);
            RefreshUIButtonCategories();
            EditorUtility.DisplayProgressBar("Refreshing UIButtons Database", "Validating...", 1f);
            ValidateUIButtonsDatabase();
            EditorUtility.ClearProgressBar();
        }
        
        public static void RefreshCanvasNamesDatabase()
        {
            EditorUtility.DisplayProgressBar("Refreshing Canvas Names Database", "", 0f);
            CanvasNamesDatabase = GetCanvasNamesDatabase();
            if(CanvasNamesDatabase == null) { CreateCanvasDatabase(); } //the canvas names database asset file does not exist -> create it
            EditorUtility.DisplayProgressBar("Refreshing Canvas Names Database", "Creating Canvas Names List...", 0.9f);
            RefreshCanvasNames();
            EditorUtility.DisplayProgressBar("Refreshing Canvas Names Database", "Validating...", 1f);
            ValidateCanvasNamesDatabase();
            EditorUtility.ClearProgressBar();
        }


        private static List<string> m_UIButtonCategories;
        public static List<string> UIButtonCategories { get { if(m_UIButtonCategories == null) { RefreshUIButtonCategories(); } return m_UIButtonCategories; } }
        public static void RefreshUIButtonCategories()
        {

            if(m_UIButtonCategories == null) { m_UIButtonCategories = new List<string>(); }
            m_UIButtonCategories.Clear();
            m_UIButtonCategories.Add(CUSTOM_NAME);
            m_UIButtonCategories.AddRange(GetUIButtonCategories());
        }

        private static List<string> m_CanvasNames;
        public static List<string> CanvasNames { get { if(m_CanvasNames == null) { RefreshCanvasNames(); } return m_CanvasNames; } }
        public static void RefreshCanvasNames()
        {
            if(m_CanvasNames == null) { m_CanvasNames = new List<string>(); }
            m_CanvasNames.Clear();
            m_CanvasNames.AddRange(GetCanvasNames());
        }

        public static bool UIButtonCategoryExists(string categoryName)
        {
            return UIButtonCategories.Contains(categoryName);
        }
        public static bool UIButtonNameExists(string categoryName, string elementName)
        {
            if(!UIButtonCategoryExists(categoryName)) { return false; }
            return GetUIButtonNames(categoryName).Contains(elementName);
        }

        private static NamesDatabase UIButtonCategoryDatabase(string categoryName)
        {
            NamesDatabase db = GetResource<NamesDatabase>(RESOURCES_PATH_UIBUTTONS, categoryName);
            if(db != null)
            {
                return db;
            }
            return CreateAsset<NamesDatabase>(RELATIVE_PATH_UIBUTTONS, categoryName);
        }

        private static string[] GetUIButtonCategories()
        {
            if(UIButtonsDatabase == null) { RefreshUIButtonsDatabase(); }
            return new List<string>(UIButtonsDatabase.Keys).ToArray();
        }
        private static string[] GetCanvasNames()
        {
            if(CanvasNamesDatabase == null) { RefreshCanvasNames(); }
            return CanvasNamesDatabase.ToArray();
        }

        public static List<string> GetUIButtonNames(string categoryName)
        {
            if(UIButtonsDatabase == null) { RefreshUIButtonsDatabase(); }
            if(!UIButtonsDatabase.ContainsKey(categoryName)) { return null; }
            return UIButtonsDatabase[categoryName].data;
        }

        private static void AddName(NamesDatabase targetDatabase, string name)
        {
            if(targetDatabase == null) { return; }
            targetDatabase.Add(name);
            EditorUtility.SetDirty(targetDatabase);
            AssetDatabase.SaveAssets();
        }
      
        public static void RenameName(NamesDatabase targetDatabase, string oldName, string newName)
        {
            if(oldName.Equals(newName) || string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName)) { return; }
            if(targetDatabase == null || !targetDatabase.Contains(oldName)) { return; }
            if(targetDatabase.Contains(newName)) { targetDatabase.Remove(newName); }
            targetDatabase.data[targetDatabase.IndexOf(oldName)] = newName;
            targetDatabase.Sort();
            EditorUtility.SetDirty(targetDatabase);
            AssetDatabase.SaveAssets();
        }

        private static void CreateCanvasDatabase()
        {
            NamesDatabase canvasDatabase = CreateAsset<NamesDatabase>(RELATIVE_PATH_CANVAS_DATABASE, CANVAS_DATABASE_FILENAME);
            canvasDatabase.Init();
            EditorUtility.SetDirty(canvasDatabase);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshCanvasNamesDatabase();
        }
     
       
        private static void ValidateUIButtonsDatabase()
        {
            bool refreshDatabase = false;
            if(UIButtonsDatabase == null) { RefreshUIButtonsDatabase(); return; }
            if(!UIButtonCategoryExists(UNCATEGORIZED_CATEGORY_NAME))
            {
                refreshDatabase = true;
                NamesDatabase newCategory = CreateAsset<NamesDatabase>(RELATIVE_PATH_UIBUTTONS, UNCATEGORIZED_CATEGORY_NAME);
                newCategory.Init();
                EditorUtility.SetDirty(newCategory);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else if(!UIButtonNameExists(UNCATEGORIZED_CATEGORY_NAME, DEFAULT_BUTTON_NAME))
            {
                refreshDatabase = true;
            }
            if(refreshDatabase)
            {
                AddName(UIButtonCategoryDatabase(UNCATEGORIZED_CATEGORY_NAME), DEFAULT_BUTTON_NAME);
                RefreshUIButtonsDatabase();
            }
        }
       
        private static void ValidateCanvasNamesDatabase()
        {
            bool refreshDatabase = false;
            if(CanvasNamesDatabase == null) { RefreshCanvasNamesDatabase(); return; }
            CanvasNamesDatabase.RemoveEmpty();
            CanvasNamesDatabase.Sort();
            if(refreshDatabase) { RefreshCanvasNamesDatabase(); }
        }

#region UI built-in sprites
        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpriteResourcePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";

        public static Sprite UISprite { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath); ; } }
        public static Sprite Background { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpriteResourcePath); ; } }
        public static Sprite FieldBackground { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath); ; } }
        public static Sprite Knob { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath); ; } }
        public static Sprite Checkmark { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath); ; } }
#endregion
#endif
    }
}
