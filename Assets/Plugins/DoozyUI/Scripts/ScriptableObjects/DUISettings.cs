// Copyright (c) 2015 - 2018 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using QuickEngine.Core;
using System;
using UnityEngine;

namespace DoozyUI
{
    [Serializable]
    public class DUISettings : ScriptableObject
    {
        private static DUISettings _instance;
        public static DUISettings Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Q.GetResource<DUISettings>(DUI.RESOURCES_PATH_SETTINGS, DUI.SETTINGS_FILENAME);

#if UNITY_EDITOR
                    if(_instance == null)
                    {
                        _instance = Q.CreateAsset<DUISettings>(DUI.RELATIVE_PATH_SETTINGS, DUI.SETTINGS_FILENAME);
                    }
#endif
                }
                return _instance;
            }
        }

        //General Settings
        public bool autoExpandEnabledFeatures = false;

        public bool UIButton_allowMultipleClicks = true;
        public float UIButton_disableButtonInterval = UIButton.BETWEEN_CLICKS_DISABLE_INTERVAL;
        public bool UIButton_deselectButtonOnClick = true;
        
        public bool UIButton_Inspector_HideOnClick = false;
        public bool UIButton_useOnClickAnimations = true;
        public bool UIButton_waitForOnClickAnimation = true;
        public UIButton.SingleClickMode UIButton_singleClickMode = UIButton.SingleClickMode.Instant;
        public bool UIButton_customOnClickSound = false;
        public UIButton.ButtonAnimationType UIButton_onClickAnimationType = UIButton.ButtonAnimationType.Punch;
        public string UIButton_onClickPunchPresetCategory = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
        public string UIButton_onClickPunchPresetName = UIAnimatorUtil.DEFAULT_PRESET_NAME;
        public bool UIButton_loadOnClickPunchPresetAtRuntime = false;
        public string UIButton_onClickStatePresetCategory = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
        public string UIButton_onClickStatePresetName = UIAnimatorUtil.DEFAULT_PRESET_NAME;
        public bool UIButton_loadOnClickStatePresetAtRuntime = false;

        public bool UIButton_Inspector_HideNormalLoop = false;
        public string UIButton_normalLoopPresetCategory = UIAnimatorUtil.UNCATEGORIZED_CATEGORY_NAME;
        public string UIButton_normalLoopPresetName = UIAnimatorUtil.DEFAULT_PRESET_NAME;
        public bool UIButton_loadNormalLoopPresetAtRuntime = false;

    }
}
