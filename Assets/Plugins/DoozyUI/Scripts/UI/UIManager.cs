// Copyright (c) 2015 - 2018 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoozyUI
{
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        protected UIManager()
        {

        }

        /// <summary>
        /// The instance
        /// </summary>
        private static UIManager _instance;
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static UIManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    if(applicationIsQuitting)
                    {
                        Debug.LogWarning("[Singleton] Instance '" + typeof(UIManager) + "' already destroyed on application quit. Won't create again - returning null.");
                        return null;
                    }

                    _instance = FindObjectOfType<UIManager>();

                    if(_instance == null)
                    {
                        GameObject singleton = new GameObject("UIManager");
                        _instance = singleton.AddComponent<UIManager>();
                        DontDestroyOnLoad(singleton);
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// The application is quitting.
        /// </summary>
        private static bool applicationIsQuitting = false;
        /// <summary>
        /// Called when [application quit].
        /// </summary>
        private void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        #region Context Menu
#if UNITY_EDITOR
        static void CreateUIManager(UnityEditor.MenuCommand menuCommand)
        {
            if(FindObjectOfType<UIManager>() != null)
            {
                Debug.Log("[UI Manager] Cannot add another UIManager to this Scene because you don't need and should not have more than one.");
                UnityEditor.Selection.activeObject = FindObjectOfType<UIManager>();
                return;
            }

            GameObject go = new GameObject("UIManager", typeof(UIManager));
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            UnityEditor.Selection.activeObject = go;
        }
#endif
        #endregion

        /// <summary>
        /// Prints debug messages related to UIButtons at runtime.
        /// </summary>
        private bool debugUIButtons = false;
        /// <summary>
        /// Internal variable used to keep track if button clicks are disabled or not. This affects all the UIButtons.
        /// <para>This is an additive bool so if == 0 --> false (button clicks are NOT disabled) and if > 0 --> true (button clicks are disabled).</para>
        /// </summary>
        private int buttonClicksDisableLevel = 0;
        /// <summary>
        /// Returns true if button clicks are disabled and false otherwise. This is mosty used when an UIElement is in transition and, in order to prevent accidental clicks, the buttons need to be disabled.
        /// </summary>
        public bool ButtonClicksDisabled
        {
            get
            {
                if(buttonClicksDisableLevel < 0) { buttonClicksDisableLevel = 0; }
                return buttonClicksDisableLevel == 0 ? false : true;
            }
        }

        /// <summary>
        /// Internal variable used to keep track if the 'Back' button is disabled or not.
        /// <para>This is an additive bool so if == 0 --> false (the 'Back' button is NOT disabled) and if > 0 --> true (the 'Back' button is disabled).</para>
        /// </summary>
        private int backButtonDisableLevel = 0;
        /// <summary>
        /// Returns true if the 'Back' button is disabled and false otherwise.
        /// </summary>
        public bool BackButtonDisabled
        {
            get
            {
                if(backButtonDisableLevel < 0) { backButtonDisableLevel = 0; }
                return backButtonDisableLevel == 0 ? false : true;
            }
        }
        /// <summary>
        /// Every time the user pauses the game, this variable stores the current Time.timeScale value. This is needed so that when the game needs to get unpaused, UIManager will know at what timescale should the game return to.
        /// </summary>
        private float currentGameTimeScale = 1;

        /// <summary>
        /// Global static variable that determines if the UINotification look for TextMeshProUGUI component instead of a Text componenet when looking for text.
        /// <para>TextMeshPro support is currently in limbo as we wait to see what Unity does with it.</para>
        /// </summary>
        private static bool usesTMPro = false;
        private bool useTextMeshPro = false;                         //Used to change in the inspector the settings for the static variable

        private static int len;
        private static int count;

        private Transform canvasTransform;
        private UIElement background;
        public Queue<UIElement> backgroundQueue;

        void Awake()
        {
            if(_instance != null && _instance != this)
            {
                Debug.Log("[DoozyUI] There cannot be two UIManagers active at the same time. Destryoing this one!");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);


            DOTween.Init();
          
            usesTMPro = useTextMeshPro;

        }

        void Start()
        {
            currentGameTimeScale = Time.timeScale;
        }

        void Update()
        {
            ListenForBackButton();
        }

        public UIElement GetBackgroundPanel(bool instantAction)
        {
            UIElement bg = null;
            if (backgroundQueue.Count < 1)
            {
                UIElement newBg = Instantiate<UIElement>(background, transform);
                newBg.transform.SetParent(canvasTransform, false);
                backgroundQueue.Enqueue(newBg);
            }

            if (backgroundQueue.Count > 0)
            {
                bg = backgroundQueue.Dequeue();
            }

            bg.Show(instantAction);
            return bg;
        }

        public void HideBackgroundPanel(UIElement bg, bool instantAction)
        {
            bg.Hide(instantAction);
            backgroundQueue.Enqueue(bg);
        }

        public void InitBackgroundQueue(UIElement _background,Transform _canvasTranform)
        {
            this.background = _background;
            this.canvasTransform = _canvasTranform;
            backgroundQueue = new Queue<UIElement>(3);
            if (background == null)
            {
                return;
            }
            
            UIElement newBg = Instantiate<UIElement>(background, _canvasTranform);
            UIElement newBg2 = Instantiate<UIElement>(background, _canvasTranform);
            UIElement newBg3 = Instantiate<UIElement>(background, _canvasTranform);
            //newBg.transform.SetParent(canvasTransform, false);
            backgroundQueue.Enqueue(newBg);
            backgroundQueue.Enqueue(newBg2);
            backgroundQueue.Enqueue(newBg3);
        }


        /// <summary>
        /// This is the main Button Action trigger (previously known as the Button Click trigger).
        /// <para>Note: Only OnClick will the button name be taken into account. All the other actionTypes are used only for navigation purposes.</para>
        /// </summary>
        private void OnButtonAction(string buttonName, UIButton uiButton = null, UIButton.ButtonActionType actionType = UIButton.ButtonActionType.OnClick)
        {
            if(Instance.debugUIButtons) { Debug.Log("[DoozyUI] [UIManager] [OnButtonAction] [" + actionType + "] ['" + buttonName + "' button name]"); }

            if(actionType == UIButton.ButtonActionType.OnClick)
            {
                if(BackButtonDisabled && buttonName.Equals(DUI.BACK_BUTTON_NAME)) { return; } //if the back button is disabled and the user presses the 'Back' button, then we do not send the event further
            }

            switch(actionType)
            {
                case UIButton.ButtonActionType.OnClick:
                    Instance.TriggerTheTriggers(buttonName, DUI.EventType.ButtonClick);
                    break;
            }

        }


        #region Button Actions
        /// <summary>
        /// Sends a button action with a reference to the UIButton that sent it and what type of action it is.
        /// </summary>
        public void SendButtonAction(UIButton uiButton, UIButton.ButtonActionType actionType)
        {
            OnButtonAction(uiButton.buttonName, uiButton, actionType);
        }

        #endregion

        #region The 'Back' button
        /// <summary>
        /// Listener for the 'Back' button.
        /// </summary>
        void ListenForBackButton()
        {
            if(BackButtonDisabled) { return; }
        }
        /// <summary>
        /// Disables the 'Back' button functionality
        /// </summary>
        public static void DisableBackButton()
        {
            Instance.backButtonDisableLevel++; //if == 0 --> false (back button is not disabled) if > 0 --> true (back button is disabled)
        }
        /// <summary>
        /// Enables the 'Back' button functionality
        /// </summary>
        public static void EnableBackButton()
        {
            Instance.backButtonDisableLevel--; //if == 0 --> false (back button is not disabled) if > 0 --> true (back button is disabled)
            if(Instance.backButtonDisableLevel < 0) { Instance.backButtonDisableLevel = 0; } //Check so that the backButtonDisableLevel does not go below zero
        }
       
        #endregion


        #region UITrigger
     
        /// <summary>
        /// Triggers all the UITriggers that are listening for the given triggerValue and are of the given triggerType.
        /// </summary>
        public void TriggerTheTriggers(string triggerValue, DUI.EventType triggerType)
        {
            StartCoroutine(ExecuteTriggerTheTriggersInTheNextFrame(triggerValue, triggerType));
        }

        IEnumerator ExecuteTriggerTheTriggersInTheNextFrame(string triggerValue, DUI.EventType triggerType)
        {
            yield return null;
           
        }
        #endregion


        #region Game Management
       
        public static void ApplicationQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

    }
}
