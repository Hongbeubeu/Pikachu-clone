// Copyright (c) 2015 - 2018 Doozy Entertainment / Marlink Trading SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoozyUI
{
    public class UINavigation
    {
        public static Queue<UIElement> QueueHistory { get; set; }

        private static void InitHistory()
        {
            QueueHistory = new Queue<UIElement>(1);
        }
        public static void ClearHistory()
        {
            QueueHistory = null;
        }

        public static void AddItemToHistory(UIElement element)
        {
            if (QueueHistory == null)
            {
                InitHistory();
            }

            if (QueueHistory != null) QueueHistory.Enqueue(element);
        }

        private static UIElement GetLastItemFromNavigationHistory()
        {
            if (QueueHistory.Count > 0)
            {
                return QueueHistory.Dequeue();
            }

            return null;
        }

        public static void ShowHistory()
        {
            UIElement element = GetLastItemFromNavigationHistory();
            if (element == null) {  return;  }
            element.Show(false);
        }
    }

}
