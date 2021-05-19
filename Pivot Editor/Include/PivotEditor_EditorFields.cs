#if UNITY_EDITOR

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 날짜 : 2021-05-16 PM 3:34:18
// 작성자 : Rito

namespace Rito.EditorPlugins
{
    public partial class PivotEditor : MonoBehaviour
    {
        [CustomEditor(typeof(PivotEditor))]
        private partial class Custom : UnityEditor.Editor
        {
            private static readonly Color HandleColor = new Color(0.6f, 0.4f, 1.0f, 1.0f);
            private static readonly Color BoundsColor = new Color(0.7f, 0.5f, 1.0f, 1.0f);
            private static readonly Color ConfinedColor = new Color(1.0f, 1.0f, 0.1f, 1.0f);

            private static readonly Color DarkButtonColor   = new Color(0.5f, 0.3f, 0.7f, 1.0f);
            private static readonly Color DarkButtonColor2  = new Color(0.4f, 0.1f, 0.5f, 1.0f);
            private static readonly Color LightButtonColor  = new Color(0.9f, 0.7f, 1.4f, 1.0f);
            private static readonly Color LightButtonColor2 = new Color(0.6f, 0.4f, 1.0f, 1.0f);
            private static readonly Color ContentColor    = new Color(1.4f, 1.1f, 1.8f, 1.0f);
            private static readonly Color BackgroundColor = new Color(0.3f, 0.1f, 0.6f, 0.3f);

            // 1. 기본(버튼 한개)
            private const float HeaderButtonHeight = 48f;

            // 2. Edit 버튼을 누른 경우
            private const float ContentHeight = HeaderButtonHeight + 292f;

            // 3. Edit Pivot 토글을 활성화한 경우
            private const float FullContentHeight = ContentHeight + 72f;

            private PivotEditor me;
            private float viewWidth;
            private float safeViewWidth;
            private GUILayoutOption safeViewWidthOption;
            private GUILayoutOption safeViewWidthHalfOption;  // 1/2
            private GUILayoutOption safeViewWidthThirdOption; // 1/3
            private GUILayoutOption safeViewWidthTwoThirdOption; // 2/3

            private static readonly GUILayoutOption ApplyButtonHeightOption 
                = GUILayout.Height(24f);

            private Vector3 prevPivotPos;
            private Vector3 prevPosition;
            private Quaternion prevRotation;
            private Vector3 prevScale;
            private bool IsTransformChanged
            {
                get
                {
                    return
                        me.transform.position != prevPosition ||
                        me.transform.rotation != prevRotation ||
                        me.transform.localScale != prevScale;
                }
            }
            private Vector3 BoundsCenter => (me.minBounds + me.maxBounds) * 0.5f;
            private Vector3 BoundsSize => (me.maxBounds - me.minBounds);

            private void OnEnable()
            {
                me = target as PivotEditor;

                if (me.meshFilter == null)
                {
                    me.meshFilter = me.GetComponent<MeshFilter>();
                }

                if (me.meshRenderer == null)
                {
                    me.meshRenderer = me.GetComponent<MeshRenderer>();
                }

                RecordTransform();
            }

            public override void OnInspectorGUI()
            {
                if (DrawWarnings()) return;

                // Remember Old Styles
                var oldColor = GUI.color;
                var oldBG = GUI.backgroundColor;
                var oldButtonFontStyle = GUI.skin.button.fontStyle;
                var oldLabelFontStyle = GUI.skin.label.fontStyle;

                InitValues();

                EditorGUILayout.Space(4f);
                DrawBackgroundBox();
                DrawEditOrCancleButton();

                if (me.editMode)
                {
                    if(IsTransformChanged)
                        RecalculateMeshBounds();

                    EditorGUILayout.Space(12f);
                    DrawEditPivotToggle();
                    DrawEditModeFields();

                    EditorGUILayout.Space(12f);
                    DrawSetPivotPosButtons();

                    EditorGUILayout.Space(12f);
                    DrawResetTransformButtons();

                    EditorGUILayout.Space(12f);
                    DrawApplyButtons();

                    RecordTransform();
                }

                EditorGUILayout.Space(4f);

                // Restore Styles
                GUI.color = oldColor;
                GUI.backgroundColor = oldBG;
                GUI.skin.button.fontStyle = oldButtonFontStyle;
                GUI.skin.label.fontStyle = oldLabelFontStyle;

                SceneView.RepaintAll();
            }

            public void OnSceneGUI()
            {
                if (me.editMode)
                {
                    Tools.pivotMode = PivotMode.Pivot;

                    DrawPivotPointGizmo();

                    if (me.pivotEditMode)
                    {
                        DrawPivotHandle();

                        Handles.BeginGUI();
                        DrawSceneGUI();
                        Handles.EndGUI();
                    }

                    if (me.showBounds)
                    {
                        DrawBounds();
                    }
                }
            }
        }
    }
}

#endif