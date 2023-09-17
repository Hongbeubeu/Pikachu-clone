using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;

namespace SuperAshley.GoogleSpreadSheet
{
    public class SAGoogleSpreadsheetLoader : EditorWindow
    {
        private const string ApiKey = "AIzaSyBedkOU_m9PDCihUS5j_rIECQuOuqFIXX4";

        public enum GenerateState
        {
            Script,
            Asset
        }

        private const string AppName = "Google-Sheets";
        private string _spreadsheetId = string.Empty;
        private List<ValueRange> _listSheetData;
        private List<string> _sheets;
        private Vector2 _worksheetScroll;
        private Vector2 _cellScroll;
        private Vector2 _cellTitleScroll;

        private string _sheetTitle = string.Empty;

        private string _selectedWorksheet = string.Empty;
        private string _saveScriptPath = string.Empty;
        private string _saveAssetPath = string.Empty;


        private void OnEnable()
        {
            _spreadsheetId = SASettings.SpreadsheetID;

            _selectedWorksheet = SASettings.SelectedWorksheet;
            _saveScriptPath = SASettings.ScriptFolder;
            _saveAssetPath = SASettings.AssetFolder;
        }

        #region UI

        /// <summary>
        /// Draw the UI for this tool.
        /// </summary>s
        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80f;
            GUILayout.Space(3f);

            DrawSpreadsheetSettings();

            DrawSpreadsheetInfo();
        }

        private void DrawSpreadsheetSettings()
        {
            // spreadsheet settings
            SAEditorHelper.DrawHeader("Spreadsheet Settings");
            SAEditorHelper.BeginContents();

            // spreadsheet ID
            GUI.changed = false;
            _spreadsheetId = EditorGUILayout.TextField("Sheet ID", _spreadsheetId);
            if (GUI.changed)
            {
                SASettings.SpreadsheetID = _spreadsheetId;

                SASettings.WorksheetJSON = string.Empty;
                SASettings.CellsJSON = string.Empty;
                SASettings.SelectedWorksheet = string.Empty;
            }

            // load spread sheet button
            if (GUILayout.Button("Load", "LargeButtonMid"))
            {
                _selectedWorksheet = string.Empty;

                SASettings.WorksheetJSON = string.Empty;
                SASettings.CellsJSON = string.Empty;
                SASettings.SelectedWorksheet = string.Empty;

                LoadSpreadsheetData();
            }

            SAEditorHelper.EndContents();
        }

        private void DrawSpreadsheetInfo()
        {
            //if (worksheetDict != null && worksheetDict.Count > 0)
            // if (ValueRanges != null && ValueRanges.Count > 0)
            {
                // spreadsheet info
                if (string.IsNullOrEmpty(_sheetTitle)) return;
                SAEditorHelper.DrawHeader(_sheetTitle);
                SAEditorHelper.BeginContents();

                DrawWorksheetTab();

                DrawWorksheetCells();

                SAEditorHelper.EndContents();
            }
        }

        private void DrawWorksheetTab()
        {
            if (_sheets == null || _sheets.Count == 0) return;
            SAEditorHelper.DrawSubHeader("Worksheets");
            _worksheetScroll = EditorGUILayout.BeginScrollView(_worksheetScroll, GUILayout.Height(40f));

            EditorGUILayout.BeginHorizontal();
            foreach (var item in _sheets)
            {
                if (item == _selectedWorksheet)
                    GUI.color = Color.blue;
                if (GUILayout.Button(item, "U2D.createRect"))
                {
                    _selectedWorksheet = item;
                    SASettings.SelectedWorksheet = _selectedWorksheet;
                    _cellScroll = Vector2.zero;

                    //if (cellList != null)
                    //{
                    //    cellList = null;

                    //    SASettings.CellsJSON = string.Empty;
                    //}
                }

                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void DrawWorksheetCells()
        {
            if (string.IsNullOrEmpty(_selectedWorksheet)) return;
            if (_sheets == null || _sheets.Count == 0) return;
            var idSheet = _sheets.IndexOf(_selectedWorksheet);

            if (_listSheetData == null || _listSheetData.Count <= 0) return;
            var sheetData = _listSheetData[idSheet];
            SAEditorHelper.DrawSubHeader("Cells Preview");
            _cellTitleScroll.x = _cellScroll.x;
            _cellTitleScroll.y = 0f;

            var transparent = new Color(0, 0, 0, 0);
            GUI.color = transparent;

            var scrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
            scrollbarStyle.fixedHeight = scrollbarStyle.fixedWidth = 0;

            EditorGUILayout.BeginScrollView(_cellTitleScroll, false, true, scrollbarStyle,
                GUI.skin.verticalScrollbar, GUI.skin.scrollView, GUILayout.Height(20f));

            GUI.color = Color.white;

            GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginHorizontal();


            for (var col = 0; col < sheetData.Values[1].Count; col++)
            {
                GUILayout.Label(sheetData.Values[1][col].ToString(), "TextArea", GUILayout.Width(100f),
                    GUILayout.Height(20f));
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndScrollView();

            _cellScroll = EditorGUILayout.BeginScrollView(_cellScroll, true, true, GUILayout.Height(200f));
            for (var row = 2; row < sheetData.Values.Count; row++)
            {
                EditorGUILayout.BeginHorizontal();
                if (row == 0)
                    GUI.backgroundColor = Color.green;
                for (var col = 0; col < sheetData.Values[row].Count; col++)
                {
                    GUILayout.Label(sheetData.Values[row][col].ToString(), "TextArea", GUILayout.Width(100f),
                        GUILayout.Height(20f));
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            DrawGenerateButtons();
        }

        private void DrawGenerateButtons()
        {
            GUILayout.Space(3f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Script", "LargeButtonMid", GUILayout.Width(150f)))
            {
                var idSheet = _sheets.IndexOf(_selectedWorksheet);
                var sheetData = _listSheetData[idSheet];
                SASheetDataClassGenerator.GenerateClass(_selectedWorksheet, sheetData);
            }

            GUILayout.Space(3f);
            if (GUILayout.Button("Choose Script Folder", "LargeButtonMid", GUILayout.Width(150f)))
            {
                var path = EditorUtility.OpenFolderPanel("Choose Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _saveScriptPath = path.Replace(Application.dataPath, "Assets");
                    SASettings.ScriptFolder = _saveScriptPath;
                }
            }

            GUILayout.Space(3f);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(4f);
            GUILayout.Label(_saveScriptPath, "TextArea", GUILayout.Height(20f));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Asset", "LargeButtonMid", GUILayout.Width(150f)))
            {
                var idSheet = _sheets.IndexOf(_selectedWorksheet);
                var sheetData = _listSheetData[idSheet];
                SASheetDataClassGenerator.CreateAsset(_selectedWorksheet, sheetData);
            }

            GUILayout.Space(3f);
            if (GUILayout.Button("Choose Asset Folder", "LargeButtonMid", GUILayout.Width(150f)))
            {
                var path = EditorUtility.OpenFolderPanel("Choose Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _saveAssetPath = path.Replace(Application.dataPath, "Assets");
                    SASettings.AssetFolder = _saveAssetPath;
                }
            }

            GUILayout.Space(3f);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(4f);
            GUILayout.Label(_saveAssetPath, "TextArea", GUILayout.Height(20f));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        private void LoadSpreadsheetData()
        {
            _listSheetData = new List<ValueRange>();
            GetDataSheets();
        }

        private void GetDataSheets()
        {
            //Validate input
            if (string.IsNullOrEmpty(_spreadsheetId))
            {
                Debug.LogError("spreadSheetKey can not be null!");
                return;
            }

            Debug.Log("Start downloading from key: " + _spreadsheetId);


            var service = new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = ApiKey,
                ApplicationName = AppName,
            });
            Debug.Log("Executing a list request...");

            var spreadSheetData = service.Spreadsheets.Get(_spreadsheetId).Execute();
            _sheetTitle = spreadSheetData.Properties.Title;
            var sheets = spreadSheetData.Sheets;
            if (sheets == null || sheets.Count <= 0)
            {
                Console.WriteLine("Not found any data!");
                return;
            }

            _sheets = new List<string>();
            foreach (var item in sheets)
            {
                _sheets.Add(item.Properties.Title);
                Debug.Log(item.Properties.GridProperties);
            }

            SpreadsheetsResource.ValuesResource.BatchGetRequest request =
                service.Spreadsheets.Values.BatchGet(_spreadsheetId);
            request.Ranges = _sheets;
            BatchGetValuesResponse response = request.Execute();
            _listSheetData = new List<ValueRange>();
            foreach (var valueRange in response.ValueRanges)
            {
                _listSheetData.Add(valueRange);
            }

            Debug.Log("Done!");

            AssetDatabase.Refresh();

            Debug.Log("Download completed.");
        }
    }
}