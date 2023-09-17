//----------------------------------------------
// SA: Google Spreadsheet Loader
// Copyright © 2014 SuperAshley Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SuperAshley.GoogleSpreadSheet
{
    public class SASettings
    {
        public static string SpreadsheetID
        {
            get { return SpreadSheetLoaderConfig.Instance.SpreadsheetID; }
            set
            {
                SpreadSheetLoaderConfig.Instance.SpreadsheetID = value;
                EditorUtility.SetDirty(SpreadSheetLoaderConfig.Instance);
            }
        }

        public static string WorksheetJSON
        {
            get => EditorPrefs.GetString("SAWorksheetJSON", string.Empty);
            set => EditorPrefs.SetString("SAWorksheetJSON", value);
        }

        public static string CellsJSON
        {
            get => EditorPrefs.GetString("SACellsJSON", string.Empty);
            set => EditorPrefs.SetString("SACellsJSON", value);
        }

        public static string SelectedWorksheet
        {
            get => EditorPrefs.GetString("SASelectedWorksheet", string.Empty);
            set => EditorPrefs.SetString("SASelectedWorksheet", value);
        }

        public static string ScriptFolder
        {
            get => SpreadSheetLoaderConfig.Instance.ScriptFolder;
            set
            {
                SpreadSheetLoaderConfig.Instance.ScriptFolder = value;
                EditorUtility.SetDirty(SpreadSheetLoaderConfig.Instance);
            }
        }

        public static string AssetFolder
        {
            get => SpreadSheetLoaderConfig.Instance.AssetFolder;
            set
            {
                SpreadSheetLoaderConfig.Instance.AssetFolder = value;
                EditorUtility.SetDirty(SpreadSheetLoaderConfig.Instance);
            }
        }
    }
}