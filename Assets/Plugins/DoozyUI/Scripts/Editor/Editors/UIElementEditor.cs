using DoozyUI.Internal;
using QuickEditor;
using QuickEngine.Extensions;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace DoozyUI
{
    [CustomEditor(typeof(UIElement), true)]
    [DisallowMultipleComponent]
    [CanEditMultipleObjects]
    public class UIElementEditor : QEditor
    {
        UIElement uiElement { get { return (UIElement)target; } }
        DUIData.AnimDatabase DatabaseInAnimations { get { return DUIData.Instance.DatabaseInAnimations; } }
        DUIData.AnimDatabase DatabaseOutAnimations { get { return DUIData.Instance.DatabaseOutAnimations; } }

#pragma warning disable 0414

        SerializedProperty
            startHidden, animateAtStart,
            useCustomStartAnchoredPosition, customStartAnchoredPosition, useBackground,


            inAnimations,
            OnInAnimationsStart, OnInAnimationsFinish,
            inAnimationsPresetCategoryName, inAnimationsPresetName, loadInAnimationsPresetAtRuntime,

            outAnimations,
            OnOutAnimationsStart, OnOutAnimationsFinish,
            outAnimationsPresetCategoryName, outAnimationsPresetName, loadOutAnimationsPresetAtRuntime;

        AnimBool
            showAutoHideDelay,
            showCustomStartPosition,
            showInAnimations, showInAnimationsEvents,
            showOutAnimations, showOutAnimationsEvents,
            showLoopAnimations, showLoopAnimationsPreset;

#pragma warning restore 0414

        bool autoExpandEnabledFeatures = false;

        //bool localShowHide = false;

        string newPresetCategoryName = "";
        string newPresetName = "";

        AnimBool createNewCategoryName;

        Index inAnimationsPresetCategoryNameIndex = new Index();
        Index inAnimationsPresetNameIndex = new Index();
        AnimBool inAnimationsNewPreset;

        Index outAnimationsPresetCategoryNameIndex = new Index();
        Index outAnimationsPresetNameIndex = new Index();
        AnimBool outAnimationsNewPreset;

        Index loopAnimationsPresetCategoryNameIndex = new Index();
        Index loopAnimationsPresetNameIndex = new Index();
        AnimBool loopAnimationsNewPreset;

        //bool currentCategoryNameIsCustomName = false;

        float GlobalWidth { get { return DUI.GLOBAL_EDITOR_WIDTH; } }
        int BarHeight { get { return DUI.BAR_HEIGHT; } }
        int MiniBarHeight { get { return DUI.MINI_BAR_HEIGHT; } }


        float tempFloat = 0;
        bool tempBool = false;

        enum AnimType { In, Out, Loop }

        protected override void SerializedObjectFindProperties()
        {
            base.SerializedObjectFindProperties();

            useBackground = serializedObject.FindProperty("useBackground");
            startHidden = serializedObject.FindProperty("startHidden");
            animateAtStart = serializedObject.FindProperty("animateAtStart");

            useCustomStartAnchoredPosition = serializedObject.FindProperty("useCustomStartAnchoredPosition");
            customStartAnchoredPosition = serializedObject.FindProperty("customStartAnchoredPosition");

            inAnimations = serializedObject.FindProperty("inAnimations");

            OnInAnimationsStart = serializedObject.FindProperty("OnInAnimationsStart");
            OnInAnimationsFinish = serializedObject.FindProperty("OnInAnimationsFinish");


            inAnimationsPresetCategoryName = serializedObject.FindProperty("inAnimationsPresetCategoryName");
            inAnimationsPresetName = serializedObject.FindProperty("inAnimationsPresetName");
            loadInAnimationsPresetAtRuntime = serializedObject.FindProperty("loadInAnimationsPresetAtRuntime");

            outAnimations = serializedObject.FindProperty("outAnimations");

            OnOutAnimationsStart = serializedObject.FindProperty("OnOutAnimationsStart");
            OnOutAnimationsFinish = serializedObject.FindProperty("OnOutAnimationsFinish");

            outAnimationsPresetCategoryName = serializedObject.FindProperty("outAnimationsPresetCategoryName");
            outAnimationsPresetName = serializedObject.FindProperty("outAnimationsPresetName");
            loadOutAnimationsPresetAtRuntime = serializedObject.FindProperty("loadOutAnimationsPresetAtRuntime");
        }

        protected override void GenerateInfoMessages()
        {
            base.GenerateInfoMessages();

            infoMessage.Add("LocalShowHide",
                            new InfoMessage()
                            {
                                title = "Local Show/Hide",
                                message = "Only this UIElement will get shown/hidden when using the SHOW/HIDE buttons. Any other UIElements with the same element category and element name will not be animated.",
                                type = InfoMessageType.Info,
                                show = new AnimBool(false, Repaint)
                            });

            infoMessage.Add("GlobalShowHide",
                           new InfoMessage()
                           {
                               title = "Global Show/Hide",
                               message = "All the UIElements with the same element category and element name will get shown/hidden when using the SHOW/HIDE buttons.",
                               type = InfoMessageType.Info,
                               show = new AnimBool(false, Repaint)
                           });

            infoMessage.Add("InAnimationsDisabled",
                            new InfoMessage()
                            {
                                title = "Enable at least one In Animation for SHOW to work.",
                                message = "",
                                type = InfoMessageType.Warning,
                                show = new AnimBool(false, Repaint)
                            });

            infoMessage.Add("OutAnimationsDisabled",
                            new InfoMessage()
                            {
                                title = "Enable at least one Out Animation for HIDE to work.",
                                message = "",
                                type = InfoMessageType.Warning,
                                show = new AnimBool(false, Repaint)
                            });

            infoMessage.Add("InAnimationsLoadPresetAtRuntime",
                            new InfoMessage()
                            {
                                title = "Runtime Preset: " + inAnimationsPresetCategoryName.stringValue + " / " + inAnimationsPresetName.stringValue,
                                message = "",
                                type = InfoMessageType.Info,
                                show = new AnimBool(loadInAnimationsPresetAtRuntime.boolValue, Repaint)
                            });

            infoMessage.Add("OutAnimationsLoadPresetAtRuntime",
                            new InfoMessage()
                            {
                                title = "Runtime Preset: " + outAnimationsPresetCategoryName.stringValue + " / " + outAnimationsPresetName.stringValue,
                                message = "",
                                type = InfoMessageType.Info,
                                show = new AnimBool(loadOutAnimationsPresetAtRuntime.boolValue, Repaint)
                            });

        }

        protected override void InitAnimBools()
        {
            base.InitAnimBools();



            showCustomStartPosition = new AnimBool(useCustomStartAnchoredPosition.boolValue, Repaint);

            showInAnimations = new AnimBool(autoExpandEnabledFeatures ? uiElement.InAnimationsEnabled || loadInAnimationsPresetAtRuntime.boolValue : false, Repaint);
            showInAnimationsEvents = new AnimBool(autoExpandEnabledFeatures ? uiElement.OnInAnimationsStart.GetPersistentEventCount() > 0 || uiElement.OnInAnimationsFinish.GetPersistentEventCount() > 0 : false, Repaint);

            showOutAnimations = new AnimBool(autoExpandEnabledFeatures ? uiElement.OutAnimationsEnabled || loadOutAnimationsPresetAtRuntime.boolValue : false, Repaint);
            showOutAnimationsEvents = new AnimBool(autoExpandEnabledFeatures ? uiElement.OnOutAnimationsStart.GetPersistentEventCount() > 0 || uiElement.OnOutAnimationsFinish.GetPersistentEventCount() > 0 : false, Repaint);

            createNewCategoryName = new AnimBool(false, Repaint);
            inAnimationsNewPreset = new AnimBool(false, Repaint);
            outAnimationsNewPreset = new AnimBool(false, Repaint);
            loopAnimationsNewPreset = new AnimBool(false, Repaint);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            requiresContantRepaint = true;

            AddMissingComponents();
            SyncData();
        }

        void AddMissingComponents()
        {
            if (uiElement.GetComponent<Canvas>() == null) { uiElement.gameObject.AddComponent<Canvas>(); }
            if (uiElement.GetComponent<CanvasGroup>() == null) { uiElement.gameObject.AddComponent<CanvasGroup>(); }
            if (uiElement.GetComponent<GraphicRaycaster>() == null) { uiElement.gameObject.AddComponent<GraphicRaycaster>(); }
        }

        void SyncData()
        {
            DUIData.Instance.ValidateInAnimations(); //validate IN Animations
            DUIData.Instance.ValidateOutAnimations(); //validate OUT Animations
        }


        public override void OnInspectorGUI()
        {
            if (IsEditorLocked) { return; }

            serializedObject.Update();

            DrawSettings(GlobalWidth);

            QUI.Space(SPACE_8);

            DrawInAnimations(GlobalWidth);

            //DrawSpecialAnimationsButtons(globalWidth);

            DrawOutAnimations(GlobalWidth);

            serializedObject.ApplyModifiedProperties();

            QUI.Space(SPACE_4);
        }

        void DrawSettings(float width)
        {
            QUI.BeginHorizontal(width);
            {
                if (animateAtStart.boolValue) { GUI.enabled = false; }
                QUI.QToggle("hide @START", startHidden);
                GUI.enabled = true;
                QUI.Space(SPACE_8);

                if (startHidden.boolValue) { GUI.enabled = false; }
                QUI.QToggle("animate @START", animateAtStart);
                GUI.enabled = true;

                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();

            QUI.Space(SPACE_4);

            #region CUSTOM START POSITION
            QUI.BeginHorizontal(width);
            {
                //CUSTOM START POSITION
                QLabel.text = "custom start position";
                QLabel.style = Style.Text.Normal;
                tempFloat = width - QLabel.x - 16 - 12; //extra space after the custom start position label
                QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, useCustomStartAnchoredPosition.boolValue ? QColors.Color.Blue : QColors.Color.Gray), QLabel.x + 16 + 12 + tempFloat * showCustomStartPosition.faded, 18 + 24 * showCustomStartPosition.faded);
                QUI.Space(-QLabel.x - 12 - 12 - tempFloat * showCustomStartPosition.faded);

                QUI.Toggle(useCustomStartAnchoredPosition);
                QUI.BeginVertical(QLabel.x + 8, QUI.SingleLineHeight);
                {
                    QUI.Label(QLabel);
                    QUI.Space(SPACE_2);
                }
                QUI.EndVertical();

                if (showCustomStartPosition.faded > 0.4f)
                {
                    QUI.PropertyField(customStartAnchoredPosition, (tempFloat - 4) * showCustomStartPosition.faded);
                }
            }
            QUI.EndHorizontal();

            showCustomStartPosition.target = useCustomStartAnchoredPosition.boolValue;

            QUI.Space(-20 * showCustomStartPosition.faded); //lift the buttons on the background

            if (showCustomStartPosition.faded > 0.4f)
            {
                tempFloat = (width - 16 - 16) / 3; //button width (3 buttons) that takes into account spaces
                QUI.BeginHorizontal(width);
                {
                    QUI.Space(20 * showCustomStartPosition.faded);
                    if (QUI.GhostButton("Get Position", QColors.Color.Blue, tempFloat * showCustomStartPosition.faded, 16 * showCustomStartPosition.faded))
                    {
                        customStartAnchoredPosition.vector3Value = uiElement.RectTransform.anchoredPosition3D;
                    }

                    QUI.Space(SPACE_4);

                    if (QUI.GhostButton("Set Position", QColors.Color.Blue, tempFloat * showCustomStartPosition.faded, 16 * showCustomStartPosition.faded))
                    {
                        Undo.RecordObject(uiElement.RectTransform, "SetPosition");
                        uiElement.RectTransform.anchoredPosition3D = customStartAnchoredPosition.vector3Value;
                    }

                    QUI.Space(SPACE_4);

                    if (QUI.GhostButton("Reset Position", QColors.Color.Blue, tempFloat * showCustomStartPosition.faded, 16 * showCustomStartPosition.faded))
                    {
                        customStartAnchoredPosition.vector3Value = Vector3.zero;
                    }

                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
            }

            QUI.Space(SPACE_8 * showCustomStartPosition.faded);

            if (useBackground.boolValue) { GUI.enabled = true; }
            QUI.QToggle("use @Background", useBackground);
            GUI.enabled = true;

            #endregion

            QUI.Space(SPACE_4);

        }

        void DrawInAnimations(float width)
        {
            QUI.BeginHorizontal(width);
            {
                DrawMainBar("In Animations", uiElement.InAnimationsEnabled, loadInAnimationsPresetAtRuntime, showInAnimations, inAnimationsNewPreset, width, BarHeight);
                DrawMainGhostButtons(AnimType.In, BarHeight, BarHeight);
            }
            QUI.EndHorizontal();

            DrawDisabledInfoMessages("InAnimationsDisabled", uiElement.InAnimationsEnabled, loadInAnimationsPresetAtRuntime, showInAnimations, width);
            DrawLoadPresetInfoMessage("InAnimationsLoadPresetAtRuntime", loadInAnimationsPresetAtRuntime, inAnimationsPresetCategoryName.stringValue, inAnimationsPresetName.stringValue, showInAnimations, width);

            QUI.BeginHorizontal(width);
            {
                QUI.Space(SPACE_8 * showInAnimations.faded);
                if (QUI.BeginFadeGroup(showInAnimations.faded))
                {
                    QUI.BeginVertical(width - SPACE_8);
                    {
                        QUI.Space(SPACE_2 * showInAnimations.faded);
                        QUI.Space(SPACE_2 * showInAnimations.faded);
                        DUIUtils.DrawAnim(uiElement.inAnimations, uiElement, width - SPACE_8); //draw in animations - generic method
                        QUI.Space(SPACE_8 * showInAnimations.faded);
                        DrawInAnimationsEvents(width - SPACE_8);
                        QUI.Space(SPACE_16 * showInAnimations.faded);
                    }
                    QUI.EndVertical();
                }
                QUI.EndFadeGroup();
            }
            QUI.EndHorizontal();
            QUI.Space(SPACE_8 * (1 - showInAnimations.faded));
        }

        void DrawInAnimationsEvents(float width)
        {
            DrawUnityEvents((uiElement.InAnimationsEnabled || loadInAnimationsPresetAtRuntime.boolValue) && (uiElement.OnInAnimationsStart.GetPersistentEventCount() > 0 || uiElement.OnInAnimationsFinish.GetPersistentEventCount() > 0),
                            showInAnimationsEvents,
                            OnInAnimationsStart,
                            "OnInAnimationsStart",
                            OnInAnimationsFinish,
                            "OnInAnimationsFinish",
                            width);
        }

        void DrawOutAnimations(float width)
        {
            QUI.BeginHorizontal(width);
            {
                DrawMainBar("Out Animations", uiElement.OutAnimationsEnabled, loadOutAnimationsPresetAtRuntime, showOutAnimations, outAnimationsNewPreset, width, BarHeight);
                DrawMainGhostButtons(AnimType.Out, BarHeight, BarHeight);
            }
            QUI.EndHorizontal();

            DrawDisabledInfoMessages("OutAnimationsDisabled", uiElement.OutAnimationsEnabled, loadOutAnimationsPresetAtRuntime, showOutAnimations, width);
            DrawLoadPresetInfoMessage("OutAnimationsLoadPresetAtRuntime", loadOutAnimationsPresetAtRuntime, outAnimationsPresetCategoryName.stringValue, outAnimationsPresetName.stringValue, showOutAnimations, width);

            QUI.BeginHorizontal(width);
            {
                QUI.Space(SPACE_8 * showOutAnimations.faded);
                if (QUI.BeginFadeGroup(showOutAnimations.faded))
                {
                    QUI.BeginVertical(width - SPACE_8);
                    {
                        QUI.Space(SPACE_2 * showOutAnimations.faded);
                        QUI.Space(SPACE_2 * showOutAnimations.faded);
                        DUIUtils.DrawAnim(uiElement.outAnimations, uiElement, width - SPACE_8); //draw out animations - generic method
                        QUI.Space(SPACE_8 * showOutAnimations.faded);
                        DrawOutAnimationsEvents(width - SPACE_8);
                        QUI.Space(SPACE_16 * showOutAnimations.faded);
                    }
                    QUI.EndVertical();
                }
                QUI.EndFadeGroup();
            }
            QUI.EndHorizontal();
            QUI.Space(SPACE_8 * (1 - showOutAnimations.faded));
        }

        void DrawOutAnimationsEvents(float width)
        {
            DrawUnityEvents((uiElement.OutAnimationsEnabled || loadOutAnimationsPresetAtRuntime.boolValue) && (uiElement.OnOutAnimationsStart.GetPersistentEventCount() > 0 || uiElement.OnOutAnimationsFinish.GetPersistentEventCount() > 0),
                            showOutAnimationsEvents,
                            OnOutAnimationsStart,
                            "OnOutAnimationsStart",
                            OnOutAnimationsFinish,
                            "OnOutAnimationsFinish",
                            width);
        }

        void DrawDisabledInfoMessages(string disabledInfoMessageTag, bool enabled, SerializedProperty loadPresetAtRuntime, AnimBool show, float width)
        {
            infoMessage[disabledInfoMessageTag].show.target = !enabled && !loadPresetAtRuntime.boolValue; //check if the animations are disabled
            DrawInfoMessage(disabledInfoMessageTag, width); //draw warning if the animations are disabled
            QUI.Space(SPACE_4 * (1 - show.faded) * infoMessage[disabledInfoMessageTag].show.faded); //space added if the animations are disabled
        }
        void DrawLoadPresetInfoMessage(string loadPresetInfoMessageTag, SerializedProperty loadPresetAtRuntime, string categoryName, string presetName, AnimBool show, float width)
        {
            infoMessage[loadPresetInfoMessageTag].show.target = loadPresetAtRuntime.boolValue; //check if a preset is set to load at runtime
            infoMessage[loadPresetInfoMessageTag].title = "Runtime Preset: " + categoryName + " / " + presetName; //set the preset category and name that are set to load at runtime
            DrawInfoMessage(loadPresetInfoMessageTag, width); //draw info if a preset is set to load at runtime
            QUI.Space(SPACE_4 * (1 - show.faded) * infoMessage[loadPresetInfoMessageTag].show.faded); //space added if a preset is set to load at runtime
        }

        void DrawPresetNormalView(AnimType animType, float width)
        {
            tempFloat = (width - 6) / 2 - 5; //dropdown lists width
            QUI.BeginHorizontal(width);
            {
                switch (animType)
                {
                    case AnimType.In:
                        //SELECT PRESET CATEGORY
                        if (!DatabaseInAnimations.ContainsCategoryName(inAnimationsPresetCategoryName.stringValue)) //if the preset category does not exist -> set it to default
                        {
                            inAnimationsPresetCategoryName.stringValue = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
                        }
                        inAnimationsPresetCategoryNameIndex.index = DatabaseInAnimations.CategoryNameIndex(inAnimationsPresetCategoryName.stringValue); //update the category index
                        QUI.BeginChangeCheck();
                        inAnimationsPresetCategoryNameIndex.index = EditorGUILayout.Popup(inAnimationsPresetCategoryNameIndex.index, DatabaseInAnimations.categoryNames.ToArray(), GUILayout.Width(tempFloat));
                        if (QUI.EndChangeCheck())
                        {
                            Undo.RecordObject(uiElement, "ChangeCategory");
                            inAnimationsPresetCategoryName.stringValue = DatabaseInAnimations.categoryNames[inAnimationsPresetCategoryNameIndex.index]; //set category naame
                            inAnimationsPresetNameIndex.index = 0; //set name index to 0
                            inAnimationsPresetName.stringValue = DatabaseInAnimations.GetCategory(inAnimationsPresetCategoryName.stringValue).presetNames[inAnimationsPresetNameIndex.index]; //set preset name according to the new index
                        }

                        QUI.Space(SPACE_4);

                        //SELECT PRESET NAME
                        if (!DatabaseInAnimations.Contains(inAnimationsPresetCategoryName.stringValue, inAnimationsPresetName.stringValue)) //if the preset name does not exist in the set category -> set it to index 0 (first item in the category)
                        {
                            inAnimationsPresetNameIndex.index = 0; //update the index
                            inAnimationsPresetName.stringValue = DatabaseInAnimations.GetCategory(inAnimationsPresetCategoryName.stringValue).presetNames[inAnimationsPresetNameIndex.index]; //update the preset name value
                        }
                        else
                        {
                            inAnimationsPresetNameIndex.index = DatabaseInAnimations.ItemNameIndex(inAnimationsPresetCategoryName.stringValue, inAnimationsPresetName.stringValue); //update the item index
                        }
                        QUI.BeginChangeCheck();
                        inAnimationsPresetNameIndex.index = EditorGUILayout.Popup(inAnimationsPresetNameIndex.index, DatabaseInAnimations.GetCategory(inAnimationsPresetCategoryName.stringValue).presetNames.ToArray(), GUILayout.Width(tempFloat * (1 - inAnimationsNewPreset.faded)));
                        if (QUI.EndChangeCheck())
                        {
                            Undo.RecordObject(uiElement, "ChangePreset");
                            inAnimationsPresetName.stringValue = DatabaseInAnimations.GetCategory(inAnimationsPresetCategoryName.stringValue).presetNames[inAnimationsPresetNameIndex.index]; //set preset name according to the new index
                        }
                        break;
                    case AnimType.Out:
                        //SELECT PRESET CATEGORY
                        if (!DatabaseOutAnimations.ContainsCategoryName(outAnimationsPresetCategoryName.stringValue)) //if the preset category does not exist -> set it to default
                        {
                            outAnimationsPresetCategoryName.stringValue = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
                        }
                        outAnimationsPresetCategoryNameIndex.index = DatabaseOutAnimations.CategoryNameIndex(outAnimationsPresetCategoryName.stringValue); //update the category index
                        QUI.BeginChangeCheck();
                        outAnimationsPresetCategoryNameIndex.index = EditorGUILayout.Popup(outAnimationsPresetCategoryNameIndex.index, DatabaseOutAnimations.categoryNames.ToArray(), GUILayout.Width(tempFloat));
                        if (QUI.EndChangeCheck())
                        {
                            Undo.RecordObject(uiElement, "ChangeCategory");
                            outAnimationsPresetCategoryName.stringValue = DatabaseOutAnimations.categoryNames[outAnimationsPresetCategoryNameIndex.index]; //set category naame
                            outAnimationsPresetNameIndex.index = 0; //set name index to 0
                            outAnimationsPresetName.stringValue = DatabaseOutAnimations.GetCategory(outAnimationsPresetCategoryName.stringValue).presetNames[outAnimationsPresetNameIndex.index]; //set preset name according to the new index
                        }

                        QUI.Space(SPACE_4);

                        //SELECT PRESET NAME
                        if (!DatabaseOutAnimations.Contains(outAnimationsPresetCategoryName.stringValue, outAnimationsPresetName.stringValue)) //if the preset name does not exist in the set category -> set it to index 0 (first item in the category)
                        {
                            outAnimationsPresetNameIndex.index = 0; //update the index
                            outAnimationsPresetName.stringValue = DatabaseOutAnimations.GetCategory(outAnimationsPresetCategoryName.stringValue).presetNames[outAnimationsPresetNameIndex.index]; //update the preset name value
                        }
                        else
                        {
                            outAnimationsPresetNameIndex.index = DatabaseOutAnimations.ItemNameIndex(outAnimationsPresetCategoryName.stringValue, outAnimationsPresetName.stringValue); //update the item index
                        }
                        QUI.BeginChangeCheck();
                        outAnimationsPresetNameIndex.index = EditorGUILayout.Popup(outAnimationsPresetNameIndex.index, DatabaseOutAnimations.GetCategory(outAnimationsPresetCategoryName.stringValue).presetNames.ToArray(), GUILayout.Width(tempFloat * (1 - outAnimationsNewPreset.faded)));
                        if (QUI.EndChangeCheck())
                        {
                            Undo.RecordObject(uiElement, "ChangePreset");
                            outAnimationsPresetName.stringValue = DatabaseOutAnimations.GetCategory(outAnimationsPresetCategoryName.stringValue).presetNames[outAnimationsPresetNameIndex.index]; //set preset name according to the new index
                        }
                        break;
                }
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();
        }
        void DrawPresetNormalViewPresetButtons(AnimType animType, AnimBool newPreset, float width)
        {
            QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Gray), width, 22);

            QUI.Space(-20);

            tempFloat = (width - SPACE_16) / 3; //button width
            QUI.BeginHorizontal(width);
            {
                QUI.Space(SPACE_4);
                if (QUI.GhostButton("Load Preset", QColors.Color.Blue, tempFloat * (1 - newPreset.faded), MiniBarHeight))
                {
                    LoadPreset(animType);
                }
                QUI.Space(SPACE_4);
                if (newPreset.faded > 0)
                {
                    QUI.FlexibleSpace();
                }
                if (QUI.GhostButton("New Preset", QColors.Color.Green, tempFloat * (1 - newPreset.faded), MiniBarHeight))
                {
                    NewPreset(animType);
                }
                if (newPreset.faded > 0)
                {
                    QUI.FlexibleSpace();
                }
                QUI.Space(SPACE_4);
                if (QUI.GhostButton("Delete Preset", QColors.Color.Red, tempFloat * (1 - newPreset.faded), MiniBarHeight))
                {
                    DeletePreset(animType);
                }
                QUI.Space(SPACE_4);
            }
            QUI.EndHorizontal();
        }

        void DrawPresetNewPresetView(AnimType animType, AnimBool newPreset, float width)
        {
            tempFloat = (width - 6) / 2 - 5; //dropdown lists width
            QUI.BeginHorizontal(width);
            {
                if (createNewCategoryName.faded < 0.5f)
                {
                    switch (animType)
                    {
                        case AnimType.In:
                            //SELECT PRESET CATEGORY
                            QUI.BeginChangeCheck();
                            inAnimationsPresetCategoryNameIndex.index = EditorGUILayout.Popup(inAnimationsPresetCategoryNameIndex.index, DatabaseInAnimations.categoryNames.ToArray(), GUILayout.Width(tempFloat * (1 - createNewCategoryName.faded)));
                            if (QUI.EndChangeCheck())
                            {
                                Undo.RecordObject(uiElement, "ChangeCategory");
                                inAnimationsPresetCategoryName.stringValue = DatabaseInAnimations.categoryNames[inAnimationsPresetCategoryNameIndex.index]; //set category naame
                            }
                            break;
                        case AnimType.Out:
                            //SELECT PRESET CATEGORY
                            QUI.BeginChangeCheck();
                            outAnimationsPresetCategoryNameIndex.index = EditorGUILayout.Popup(outAnimationsPresetCategoryNameIndex.index, DatabaseOutAnimations.categoryNames.ToArray(), GUILayout.Width(tempFloat * (1 - createNewCategoryName.faded)));
                            if (QUI.EndChangeCheck())
                            {
                                Undo.RecordObject(uiElement, "ChangeCategory");
                                outAnimationsPresetCategoryName.stringValue = DatabaseOutAnimations.categoryNames[outAnimationsPresetCategoryNameIndex.index]; //set category naame
                            }
                            break;
                    }

                    QUI.Space(tempFloat * createNewCategoryName.faded);
                }
                else
                {
                    //CREATE NEW CATEGORY
                    QUI.SetNextControlName("newPresetCategoryName");
                    newPresetCategoryName = EditorGUILayout.TextField(newPresetCategoryName, GUILayout.Width(tempFloat * createNewCategoryName.faded));

                    if (createNewCategoryName.isAnimating && createNewCategoryName.target && !QUI.GetNameOfFocusedControl().Equals("newPresetCategoryName")) //select the new category name text field
                    {
                        QUI.FocusTextInControl("newPresetCategoryName");
                    }

                    QUI.Space(tempFloat * (1 - createNewCategoryName.faded));
                }

                QUI.Space(SPACE_4);

                //ENTER A NEW PRESET NAME
                QUI.SetNextControlName("newPresetName");
                newPresetName = EditorGUILayout.TextField(newPresetName, GUILayout.Width(tempFloat * newPreset.faded));

                if ((newPreset.isAnimating && newPreset.target && !QUI.GetNameOfFocusedControl().Equals("newPresetName"))
                   || (createNewCategoryName.isAnimating && !createNewCategoryName.target && !QUI.GetNameOfFocusedControl().Equals("newPresetName"))) //select the new preset name text field
                {
                    QUI.FocusTextInControl("newPresetName");
                }

                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();
        }
        void DrawPresetNewPresetViewPresetButtons(AnimType animType, AnimBool newPreset, SerializedProperty presetCategoryName, SerializedProperty presetName, float width)
        {
            QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Gray), width, 22);

            QUI.Space(-20);

            QUI.BeginHorizontal(width);
            {
                QUI.Space(SPACE_4);
                QUI.BeginChangeCheck();
                createNewCategoryName.target = QUI.Toggle(createNewCategoryName.target);
                if (QUI.EndChangeCheck())
                {
                    if (createNewCategoryName.target == false) //if the dev decided not to create a new category name, restore the selected new category name
                    {
                        newPresetCategoryName = presetCategoryName.stringValue;
                    }
                }
                QLabel.text = "Create a new category";
                QLabel.style = Style.Text.Normal;
                QUI.BeginVertical(QLabel.x, QUI.SingleLineHeight);
                {
                    QUI.Label(QLabel);
                    QUI.Space(SPACE_2);
                }
                QUI.EndVertical();

                QUI.FlexibleSpace();

                tempFloat = (width - 24) / 4; //button width
                if (QUI.GhostButton("Save Preset", QColors.Color.Green, tempFloat * newPreset.faded, MiniBarHeight)
                    || (QUI.DetectKeyUp(Event.current, KeyCode.Return) && (QUI.GetNameOfFocusedControl().Equals("newPresetName") || QUI.GetNameOfFocusedControl().Equals("newPresetCategoryName"))))
                {
                    if (SavePreset(animType)) //save the new preset -> if a new preset was saved -> update the indexes
                    {
                        switch (animType)
                        {
                            case AnimType.In:
                                inAnimationsPresetCategoryNameIndex.index = DatabaseInAnimations.CategoryNameIndex(presetCategoryName.stringValue); //update the index
                                inAnimationsPresetNameIndex.index = DatabaseInAnimations.ItemNameIndex(presetCategoryName.stringValue, presetName.stringValue); //update the index
                                break;
                            case AnimType.Out:
                                outAnimationsPresetCategoryNameIndex.index = DatabaseOutAnimations.CategoryNameIndex(presetCategoryName.stringValue); //update the index
                                outAnimationsPresetNameIndex.index = DatabaseOutAnimations.ItemNameIndex(presetCategoryName.stringValue, presetName.stringValue); //update the index
                                break;
                        }
                    }
                }

                QUI.Space(SPACE_4);

                if (QUI.GhostButton("Cancel", QColors.Color.Red, tempFloat * newPreset.faded, MiniBarHeight)
                     || QUI.DetectKeyUp(Event.current, KeyCode.Escape))
                {
                    ResetNewPresetState();
                }

                QUI.Space(SPACE_4);

                QUI.Space(tempFloat * (1 - newPreset.faded));
            }
            QUI.EndHorizontal();

            QUI.Space(-SPACE_4);
        }

        void DrawUnityEvents(bool enabled, AnimBool showEvents, SerializedProperty OnStart, string OnStartTitle, SerializedProperty OnFinish, string OnFinishTitle, float width)
        {
            if (QUI.GhostBar("Unity Events", enabled ? QColors.Color.Blue : QColors.Color.Gray, showEvents, width, MiniBarHeight))
            {
                showEvents.target = !showEvents.target;
            }
            QUI.BeginHorizontal(width);
            {
                QUI.Space(SPACE_8 * showEvents.faded);
                if (QUI.BeginFadeGroup(showEvents.faded))
                {
                    QUI.SetGUIBackgroundColor(enabled ? QUI.AccentColorBlue : QUI.AccentColorGray);
                    QUI.BeginVertical(width - SPACE_16);
                    {
                        QUI.Space(SPACE_2 * showEvents.faded);
                        QUI.PropertyField(OnStart, new GUIContent() { text = OnStartTitle }, width - 8);
                        QUI.Space(SPACE_2 * showEvents.faded);
                        QUI.PropertyField(OnFinish, new GUIContent() { text = OnFinishTitle }, width - 8);
                        QUI.Space(SPACE_2 * showEvents.faded);
                    }
                    QUI.EndVertical();
                    QUI.ResetColors();
                }
                QUI.EndFadeGroup();
            }
            QUI.EndHorizontal();
        }

        void LoadPreset(AnimType animType)
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                Undo.RecordObjects(targets, "LoadPreset");
                switch (animType)
                {
                    case AnimType.In:
                        Anim inAnim = UIAnimatorUtil.GetInAnim(inAnimationsPresetCategoryName.stringValue, inAnimationsPresetName.stringValue);
                        for (int i = 0; i < targets.Length; i++) { UIElement iTarget = (UIElement)targets[i]; iTarget.inAnimations = inAnim.Copy(); }
                        break;
                    case AnimType.Out:
                        Anim outAnim = UIAnimatorUtil.GetOutAnim(outAnimationsPresetCategoryName.stringValue, outAnimationsPresetName.stringValue);
                        for (int i = 0; i < targets.Length; i++) { UIElement iTarget = (UIElement)targets[i]; iTarget.outAnimations = outAnim.Copy(); }
                        break;
                }
            }
            else
            {
                Undo.RecordObject(uiElement, "LoadPreset");
                switch (animType)
                {
                    case AnimType.In: uiElement.inAnimations = UIAnimatorUtil.GetInAnim(inAnimationsPresetCategoryName.stringValue, inAnimationsPresetName.stringValue).Copy(); break;
                    case AnimType.Out: uiElement.outAnimations = UIAnimatorUtil.GetOutAnim(outAnimationsPresetCategoryName.stringValue, outAnimationsPresetName.stringValue).Copy(); break;
                }
            }
            QUI.ExitGUI();
        }
        void NewPreset(AnimType animType)
        {
            ResetNewPresetState();
            switch (animType)
            {
                case AnimType.In:
                    inAnimationsNewPreset.target = true;
                    newPresetCategoryName = inAnimationsPresetCategoryName.stringValue;
                    break;
                case AnimType.Out:
                    outAnimationsNewPreset.target = true;
                    newPresetCategoryName = outAnimationsPresetCategoryName.stringValue;
                    break;
            }
        }
        void DeletePreset(AnimType animType)
        {
            string categoryName = "";
            string presetName = "";
            switch (animType)
            {
                case AnimType.In:
                    categoryName = inAnimationsPresetCategoryName.stringValue;
                    presetName = inAnimationsPresetName.stringValue;
                    break;
                case AnimType.Out:
                    categoryName = outAnimationsPresetCategoryName.stringValue;
                    presetName = outAnimationsPresetName.stringValue;
                    break;
            }

            if (QUI.DisplayDialog("Delete Preset",
                                            "Are you sure you want to delete the '" + presetName + "' preset from the '" + categoryName + "' preset category?",
                                            "Yes",
                                            "No"))
            {
                switch (animType)
                {
                    case AnimType.In:
                        DUIData.Instance.DatabaseInAnimations.GetCategory(inAnimationsPresetCategoryName.stringValue).DeleteAnimData(inAnimationsPresetName.stringValue, UIAnimatorUtil.RELATIVE_PATH_IN_ANIM_DATA);
                        if (DatabaseInAnimations.GetCategory(inAnimationsPresetCategoryName.stringValue).IsEmpty()) //category is empty -> remove it from the database (sanity check)
                        {
                            DatabaseInAnimations.RemoveCategory(inAnimationsPresetCategoryName.stringValue, UIAnimatorUtil.RELATIVE_PATH_IN_ANIM_DATA, true);
                        }
                        if (serializedObject.isEditingMultipleObjects)
                        {
                            for (int i = 0; i < targets.Length; i++)
                            {
                                UIElement iTarget = (UIElement)targets[i];
                                if (iTarget.inAnimationsPresetCategoryName.Equals(inAnimationsPresetCategoryName.stringValue) ||
                                    iTarget.inAnimationsPresetName.Equals(inAnimationsPresetName.stringValue))
                                {
                                    iTarget.inAnimationsPresetCategoryName = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
                                    iTarget.inAnimationsPresetName = UIAnimatorUtil.DEFAULT_PRESET_NAME;
                                }
                            }
                        }
                        inAnimationsPresetCategoryName.stringValue = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
                        inAnimationsPresetName.stringValue = UIAnimatorUtil.DEFAULT_PRESET_NAME;
                        break;
                    case AnimType.Out:
                        DUIData.Instance.DatabaseOutAnimations.GetCategory(outAnimationsPresetCategoryName.stringValue).DeleteAnimData(outAnimationsPresetName.stringValue, UIAnimatorUtil.RELATIVE_PATH_OUT_ANIM_DATA);
                        if (DatabaseOutAnimations.GetCategory(outAnimationsPresetCategoryName.stringValue).IsEmpty()) //category is empty -> remove it from the database (sanity check)
                        {
                            DatabaseOutAnimations.RemoveCategory(outAnimationsPresetCategoryName.stringValue, UIAnimatorUtil.RELATIVE_PATH_OUT_ANIM_DATA, true);
                        }
                        if (serializedObject.isEditingMultipleObjects)
                        {
                            for (int i = 0; i < targets.Length; i++)
                            {
                                UIElement iTarget = (UIElement)targets[i];
                                if (iTarget.outAnimationsPresetCategoryName.Equals(outAnimationsPresetCategoryName.stringValue) ||
                                    iTarget.outAnimationsPresetName.Equals(outAnimationsPresetName.stringValue))
                                {
                                    iTarget.outAnimationsPresetCategoryName = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
                                    iTarget.outAnimationsPresetName = UIAnimatorUtil.DEFAULT_PRESET_NAME;
                                }
                            }
                        }
                        outAnimationsPresetCategoryName.stringValue = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
                        outAnimationsPresetName.stringValue = UIAnimatorUtil.DEFAULT_PRESET_NAME;
                        break;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
        bool SavePreset(AnimType animType)
        {
            if (createNewCategoryName.target && string.IsNullOrEmpty(newPresetCategoryName.Trim()))
            {
                QUI.DisplayDialog("Info",
                                  "The new preset category name cannot be an empty string.",
                                  "Ok");
                return false; //return false that a new preset was not created
            }

            if (string.IsNullOrEmpty(newPresetName.Trim()))
            {
                QUI.DisplayDialog("Info",
                                  "The new preset name cannot be an empty string.",
                                  "Ok");
                return false; //return false that a new preset was not created
            }

            tempBool = false; //test if the database contains the new preset name
            switch (animType)
            {
                case AnimType.In: tempBool = DatabaseInAnimations.Contains(newPresetCategoryName, newPresetName); break;
                case AnimType.Out: tempBool = DatabaseOutAnimations.Contains(newPresetCategoryName, newPresetName); break;
            }

            if (tempBool)
            {
                QUI.DisplayDialog("Info",
                                  "There is another preset with the '" + newPresetName + "' preset name in the '" + newPresetCategoryName + "' preset category." +
                                    "\n\n" +
                                    "Try a different preset name maybe?",
                                  "Ok");
                return false; //return false that a new preset was not created
            }

            tempBool = false; //test if the database contains the new category
            switch (animType)
            {
                case AnimType.In: tempBool = !DatabaseInAnimations.ContainsCategory(newPresetCategoryName); break;
                case AnimType.Out: tempBool = !DatabaseOutAnimations.ContainsCategory(newPresetCategoryName); break;
            }

            if (tempBool) //if creating a new category, check that it does not already exist
            {
                switch (animType) //create the new category 
                {
                    case AnimType.In: DatabaseInAnimations.AddCategory(newPresetCategoryName, UIAnimatorUtil.RELATIVE_PATH_IN_ANIM_DATA, true); break;
                    case AnimType.Out: DatabaseOutAnimations.AddCategory(newPresetCategoryName, UIAnimatorUtil.RELATIVE_PATH_OUT_ANIM_DATA, true); break;
                }
            }

            switch (animType) //create the new preset
            {
                case AnimType.In: DatabaseInAnimations.GetCategory(newPresetCategoryName).AddAnimData(UIAnimatorUtil.CreateInAnimPreset(newPresetCategoryName, newPresetName, uiElement.inAnimations.Copy())); break;
                case AnimType.Out: DatabaseOutAnimations.GetCategory(newPresetCategoryName).AddAnimData(UIAnimatorUtil.CreateOutAnimPreset(newPresetCategoryName, newPresetName, uiElement.outAnimations.Copy())); break;
            }

            switch (animType) //set the values
            {
                case AnimType.In:
                    if (serializedObject.isEditingMultipleObjects)
                    {
                        for (int i = 0; i < targets.Length; i++)
                        {
                            UIElement iTarget = (UIElement)targets[i];
                            iTarget.inAnimationsPresetCategoryName = newPresetCategoryName;
                            iTarget.inAnimationsPresetName = newPresetName;
                        }
                    }
                    else
                    {
                        inAnimationsPresetCategoryName.stringValue = newPresetCategoryName;
                        inAnimationsPresetName.stringValue = newPresetName;
                    }
                    break;
                case AnimType.Out:
                    if (serializedObject.isEditingMultipleObjects)
                    {
                        for (int i = 0; i < targets.Length; i++)
                        {
                            UIElement iTarget = (UIElement)targets[i];
                            iTarget.outAnimationsPresetCategoryName = newPresetCategoryName;
                            iTarget.outAnimationsPresetName = newPresetName;
                        }
                    }
                    else
                    {
                        outAnimationsPresetCategoryName.stringValue = newPresetCategoryName;
                        outAnimationsPresetName.stringValue = newPresetName;
                    }
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            ResetNewPresetState();
            return true; //return true that a new preset has been created
        }
        void ResetNewPresetState()
        {
            inAnimationsNewPreset.target = false;
            outAnimationsNewPreset.target = false;
            loopAnimationsNewPreset.target = false;

            createNewCategoryName.target = false;
            newPresetCategoryName = "";
            newPresetName = "";

            QUI.ResetKeyboardFocus();
        }

        void DrawSpecialAnimationsButtons(float width)
        {
            QUI.BeginHorizontal(width);
            {
                if (QUI.GhostButton("IN -> OUT", QColors.Color.Orange))
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        UIElement iTarget = (UIElement)targets[i];
                        iTarget.outAnimations = iTarget.inAnimations.Copy();
                        iTarget.outAnimations.animationType = Anim.AnimationType.Out;
                        iTarget.outAnimations.move.animationType = Anim.AnimationType.Out;
                        iTarget.outAnimations.rotate.animationType = Anim.AnimationType.Out;
                        iTarget.outAnimations.scale.animationType = Anim.AnimationType.Out;
                        iTarget.outAnimations.fade.animationType = Anim.AnimationType.Out;
                    }
                }
                if (QUI.GhostButton("IN <- OUT", QColors.Color.Orange))
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        UIElement iTarget = (UIElement)targets[i];
                        iTarget.inAnimations = iTarget.outAnimations.Copy();
                        iTarget.inAnimations.animationType = Anim.AnimationType.In;
                        iTarget.inAnimations.move.animationType = Anim.AnimationType.In;
                        iTarget.inAnimations.rotate.animationType = Anim.AnimationType.In;
                        iTarget.inAnimations.scale.animationType = Anim.AnimationType.In;
                        iTarget.inAnimations.fade.animationType = Anim.AnimationType.In;
                    }
                }
            }
            QUI.EndHorizontal();
            QUI.Space(SPACE_8);
        }

        void DrawMainBar(string title, bool enabled, SerializedProperty loadPresetAtRuntime, AnimBool show, AnimBool newPreset, float width, float height)
        {
            if (QUI.GhostBar(title, !enabled && !loadPresetAtRuntime.boolValue ? QColors.Color.Gray : QColors.Color.Blue, show, width - height * 4, height))
            {
                show.target = !show.target;
                if (!show.target) //if closing -> reset any new preset settings
                {
                    if (newPreset.target)
                    {
                        ResetNewPresetState();
                        QUI.ExitGUI();
                    }
                }
            }
        }
        void DrawMainGhostButtons(AnimType animType, float width, float height)
        {
            switch (animType)
            {
                case AnimType.In:
                    if (QUI.GhostButton("M", uiElement.inAnimations.move.enabled ? QColors.Color.Green : QColors.Color.Gray, BarHeight, BarHeight, showInAnimations.target))
                    {
                        Undo.RecordObject(uiElement, "ToggleMove" + animType);
                        uiElement.inAnimations.move.enabled = !uiElement.inAnimations.move.enabled;
                        if (uiElement.inAnimations.move.enabled) { showInAnimations.target = true; }
                    }
                    if (QUI.GhostButton("R", uiElement.inAnimations.rotate.enabled ? QColors.Color.Orange : QColors.Color.Gray, BarHeight, BarHeight, showInAnimations.target))
                    {
                        Undo.RecordObject(uiElement, "ToggleRotate" + animType);
                        uiElement.inAnimations.rotate.enabled = !uiElement.inAnimations.rotate.enabled;
                        if (uiElement.inAnimations.rotate.enabled) { showInAnimations.target = true; }
                    }
                    if (QUI.GhostButton("S", uiElement.inAnimations.scale.enabled ? QColors.Color.Red : QColors.Color.Gray, BarHeight, BarHeight, showInAnimations.target))
                    {
                        Undo.RecordObject(uiElement, "ToggleScale" + animType);
                        uiElement.inAnimations.scale.enabled = !uiElement.inAnimations.scale.enabled;
                        if (uiElement.inAnimations.scale.enabled) { showInAnimations.target = true; }
                    }
                    if (QUI.GhostButton("F", uiElement.inAnimations.fade.enabled ? QColors.Color.Purple : QColors.Color.Gray, BarHeight, BarHeight, showInAnimations.target))
                    {
                        Undo.RecordObject(uiElement, "ToggleFade" + animType);
                        uiElement.inAnimations.fade.enabled = !uiElement.inAnimations.fade.enabled;
                        if (uiElement.inAnimations.fade.enabled) { showInAnimations.target = true; }
                    }
                    break;
                case AnimType.Out:
                    if (QUI.GhostButton("M", uiElement.outAnimations.move.enabled ? QColors.Color.Green : QColors.Color.Gray, BarHeight, BarHeight, showOutAnimations.target))
                    {
                        Undo.RecordObject(uiElement, "ToggleMove" + animType);
                        uiElement.outAnimations.move.enabled = !uiElement.outAnimations.move.enabled;
                        if (uiElement.outAnimations.move.enabled) { showOutAnimations.target = true; }
                    }
                    if (QUI.GhostButton("R", uiElement.outAnimations.rotate.enabled ? QColors.Color.Orange : QColors.Color.Gray, BarHeight, BarHeight, showOutAnimations.target))
                    {
                        Undo.RecordObject(uiElement, "ToggleRotate" + animType);
                        uiElement.outAnimations.rotate.enabled = !uiElement.outAnimations.rotate.enabled;
                        if (uiElement.outAnimations.rotate.enabled) { showOutAnimations.target = true; }
                    }
                    if (QUI.GhostButton("S", uiElement.outAnimations.scale.enabled ? QColors.Color.Red : QColors.Color.Gray, BarHeight, BarHeight, showOutAnimations.target))
                    {
                        Undo.RecordObject(uiElement, "ToggleScale" + animType);
                        uiElement.outAnimations.scale.enabled = !uiElement.outAnimations.scale.enabled;
                        if (uiElement.outAnimations.scale.enabled) { showOutAnimations.target = true; }
                    }
                    if (QUI.GhostButton("F", uiElement.outAnimations.fade.enabled ? QColors.Color.Purple : QColors.Color.Gray, BarHeight, BarHeight, showOutAnimations.target))
                    {
                        Undo.RecordObject(uiElement, "ToggleFade" + animType);
                        uiElement.outAnimations.fade.enabled = !uiElement.outAnimations.fade.enabled;
                        if (uiElement.outAnimations.fade.enabled) { showOutAnimations.target = true; }
                    }
                    break;             
            }

        }
    }
}
