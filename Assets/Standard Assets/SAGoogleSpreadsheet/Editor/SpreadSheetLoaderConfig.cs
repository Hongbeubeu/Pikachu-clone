using UnityEditor;
using UnityEngine;

namespace SuperAshley.GoogleSpreadSheet
{
    public class SpreadSheetLoaderConfig : ScriptableObject
    {
        public string SpreadsheetID;
        public string ScriptFolder;
        public string AssetFolder;
        public string SpriteAssetFolder;
        public string SkeletonDataFolder;
        public string PrefabFolder;
        static Sprite defaultSprite;
        public static SpreadSheetLoaderConfig Instance {get => GetInstance();}

        private static SpreadSheetLoaderConfig GetInstance()
        {
            SpreadSheetLoaderConfig value = AssetDatabase.LoadAssetAtPath<SpreadSheetLoaderConfig>("Assets/Standard Assets/SAGoogleSpreadsheet/Editor/SpreadSheetLoaderConfig.asset");
            if (value == null)
            {
                value = CreateInstance<SpreadSheetLoaderConfig>();
                AssetDatabase.CreateAsset(value, "Assets/Standard Assets/SAGoogleSpreadsheet/Editor/SpreadSheetLoaderConfig.asset");
            }

            return value;
        }
    }
}