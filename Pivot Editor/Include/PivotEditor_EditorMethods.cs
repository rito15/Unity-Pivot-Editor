#if UNITY_EDITOR

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 날짜 : 2021-05-16 PM 3:34:18
// 작성자 : Rito

namespace Rito.EditorUtilities
{
    public partial class PivotEditor : MonoBehaviour
    {
        private partial class Custom : UnityEditor.Editor
        {
            /// <summary> OnInspectorGUI에서 매 에디터 프레임마다 초기화 </summary>
            private void InitValues()
            {
                viewWidth = EditorGUIUtility.currentViewWidth;
                safeViewWidth = viewWidth - 48f;
                boxHeaderWidth = safeViewWidth + 8f;

                safeViewWidthOption = GUILayout.Width(safeViewWidth);
                safeViewWidthHalfOption = GUILayout.Width(safeViewWidth * 0.5f - 1f);
                safeViewWidthThirdOption = GUILayout.Width(safeViewWidth / 3f - 2f);
                safeViewWidthTwoThirdOption = GUILayout.Width(safeViewWidth * 2f/ 3f);

                // Init Styles
                if (headerBoxLabelStyle == null)
                {
                    headerBoxLabelStyleState = new GUIStyleState()
                    {
                        textColor = BoxHeaderTextColor,
                    };
                    headerBoxLabelStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontStyle = FontStyle.Bold,
                        normal = headerBoxLabelStyleState
                    };
                }
            }

            private void RecordTransform()
            {
                prevPosition = me.transform.position;
                prevRotation = me.transform.rotation;
                prevScale = me.transform.localScale;
            }

            private bool DrawWarnings()
            {
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Cannot Edit in Playmode", MessageType.Warning);
                    return true;
                }

                if (me.meshFilter == null)
                {
                    EditorGUILayout.HelpBox("Mesh Filter Does Not Exist", MessageType.Error);
                    return true;
                }

                if (me.meshFilter.sharedMesh == null)
                {
                    EditorGUILayout.HelpBox("Mesh is Null", MessageType.Error);
                    return true;
                }

                return false;
            }

            private void DrawBackgroundBox()
            {
                DrawOutlinedBox(10f, 36f);

                if (me.editMode)
                {
                    const float lineMarginY = 38f;

                    currentBoxY = 56f;
                    float optionHeight = me.pivotEditMode ? 112f : 44f;
                    if(!me.foldOutOptions) optionHeight = HeaderBoxOutlineWidth;
                    DrawOutlinedHeaderBox(" Options", currentBoxY, optionHeight, ref me.foldOutOptions);

                    currentBoxY += optionHeight + lineMarginY;
                    float boundsHeight = (me.showBounds && me.confineInBounds) ? 106f : 44f;
                    if(!me.foldOutBounds) boundsHeight = HeaderBoxOutlineWidth;
                    DrawOutlinedHeaderBox(" Bounds", currentBoxY, boundsHeight, ref me.foldOutBounds);

                    currentBoxY += boundsHeight + lineMarginY;
                    float bottonBoxHeight1 = 60f;
                    if (!me.foldOutPivotPos) bottonBoxHeight1 = HeaderBoxOutlineWidth * 3f;
                    DrawOutlinedHeaderBox(" Set Pivot", currentBoxY, bottonBoxHeight1, ref me.foldOutPivotPos, 1);

                    currentBoxY += bottonBoxHeight1 + lineMarginY;
                    float bottonBoxHeight2 = 60f;
                    if (!me.foldOutTransform) bottonBoxHeight2 = HeaderBoxOutlineWidth * 3;
                    DrawOutlinedHeaderBox(" Reset Transform", currentBoxY, bottonBoxHeight2, ref me.foldOutTransform, 1);

                    currentBoxY += bottonBoxHeight2 + lineMarginY;
                    float saveHeight = string.IsNullOrWhiteSpace(me.meshName) ? 68f : 56f;
                    if (!me.foldOutSave) saveHeight = HeaderBoxOutlineWidth;
                    DrawOutlinedHeaderBox(" Save", currentBoxY, saveHeight, ref me.foldOutSave, 2);
                }
            }

            private void DrawEditOrCancleButton()
            {
                Color oldBgColor = GUI.backgroundColor;
                Color oldcntColor = GUI.contentColor;
                int oldFontSize = GUI.skin.button.fontSize;
                FontStyle oldFontStyle = GUI.skin.button.fontStyle;

                GUI.backgroundColor = LightButtonColor;
                GUI.contentColor = Color.white * 3f;
                GUI.skin.button.fontSize = 16;
                GUI.skin.button.fontStyle = FontStyle.Bold;

                string buttonText = me.editMode ? "CANCEL" : "EDIT";
                bool editOrCancleButton = GUILayout.Button(buttonText, safeViewWidthOption, GUILayout.Height(28f));
                if (editOrCancleButton)
                {
                    me.editMode = !me.editMode;

                    // Click : Edit
                    if (me.editMode)
                    {
                        RecalculateMeshBounds();

                        me.pivotPos = me.transform.position;

                        if (string.IsNullOrWhiteSpace(me.meshName))
                            me.meshName = me.meshFilter.sharedMesh.name;
                    }
                    // Click : Cancel
                    else
                    {
                        if(me.hideTransformTool)
                            Tools.current = Tool.Move;
                    }
                }

                GUI.backgroundColor = oldBgColor;
                GUI.contentColor = oldcntColor;
                GUI.skin.button.fontSize = oldFontSize;
                GUI.skin.button.fontStyle = oldFontStyle;
            }

            private void DrawOptionsFields()
            {
                GUI.color = ContentColor;
                if (!me.foldOutOptions) return;

                // 1. Hide Transform Tool Toggle
                using (var cs = new EditorGUI.ChangeCheckScope())
                {
                    Undo.RecordObject(me, "Hide Transform Tool Toggle");
                    me.hideTransformTool = EditorGUILayout.Toggle("Hide Transform Tool", me.hideTransformTool);

                    if (cs.changed && !me.hideTransformTool)
                        Tools.current = Tool.Move;
                }
                if (me.hideTransformTool)
                    Tools.current = Tool.None;

                // 2. Edit Pivot Toggle
                using (var cs = new EditorGUI.ChangeCheckScope())
                {
                    Undo.RecordObject(me, "Change Edit Pivot Toggle");
                    me.pivotEditMode = EditorGUILayout.Toggle("Edit Pivot", me.pivotEditMode);

                    if (cs.changed && me.pivotEditMode && me.showBounds)
                        RecalculateMeshBounds();
                }
            }

            private void DrawEditPivotFields()
            {
                if(!me.foldOutOptions) return;

                // 1. Pivot Position Field
                Undo.RecordObject(me, "Change Pivot Position");
                Vector3 pivotPos = EditorGUILayout.Vector3Field("Pivot Position", me.pivotPos, safeViewWidthOption);

                if (me.snapMode)
                {
                    me.pivotPos = SnapVector3(pivotPos, me.snapValue);
                }
                else
                {
                    me.pivotPos = SnapVector3(pivotPos, 0.0001f);
                }

                // Check Pivot Pos Changed
                if (me.pivotPos != prevPivotPos && me.showBounds && me.confineInBounds)
                    RecalculateNormalizedPivotPoint();
                prevPivotPos = me.pivotPos;

                EditorGUILayout.Space(4f);

                // 2. Snap Toogle
                Undo.RecordObject(me, "Change Snap");
                me.snapMode = EditorGUILayout.Toggle("Snap", me.snapMode);

                // 3. Snap Value Slider
                using (new EditorGUI.DisabledGroupScope(!me.snapMode))
                {
                    Undo.RecordObject(me, "Change Snap Value");
                    float snap = EditorGUILayout.Slider("", me.snapValue, 0f, 1f, safeViewWidthOption);
                    me.snapValue = Mathf.Round(snap / 0.05f) * 0.05f;
                }
            }

            private void DrawBoundsFields()
            {
                if(!me.foldOutBounds) return;

                // 1. Bounds Toggle
                using (var cc = new EditorGUI.ChangeCheckScope())
                {
                    Undo.RecordObject(me, "Change Show Bounds");
                    me.showBounds = EditorGUILayout.Toggle("Show Bounds", me.showBounds);

                    if (cc.changed && me.showBounds)
                    {
                        RecalculateMeshBounds();

                        if (me.confineInBounds)
                            RecalculateNormalizedPivotPoint();
                    }
                }

                // 2. Bounds - Confine Toggle
                using (new EditorGUI.DisabledGroupScope(!me.showBounds))
                using (var cc = new EditorGUI.ChangeCheckScope())
                {
                    Undo.RecordObject(me, "Change Confine In Bounds");
                    me.confineInBounds = EditorGUILayout.Toggle("Confine Pivot In Bounds", me.confineInBounds);

                    if (cc.changed && me.confineInBounds)
                        RecalculateNormalizedPivotPoint();
                }

                // 3. Normalized X, Y, Z Pivot Point Slider
                if (me.showBounds && me.confineInBounds)
                {
                    me.pivotPos = ClampVector3(me.pivotPos, me.minBounds, me.maxBounds);

                    var oldBGColor = GUI.backgroundColor;
                    GUI.backgroundColor = DarkButtonColor;

                    using (var cc = new EditorGUI.ChangeCheckScope())
                    {
                        float x, y, z;
                        const float labelWidth = 16f;
                        const float smallButtonWidth = 20f;
                        const float margin = 12f;

                        var labelWidthOption = GUILayout.Width(labelWidth);
                        var sliderWidthOption = GUILayout.Width(safeViewWidth - labelWidth - smallButtonWidth * 3f - margin);
                        var smallButtonWidthOption = GUILayout.Width(smallButtonWidth);

                        // X
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("X", labelWidthOption);
                            x = EditorGUILayout.Slider(me.normalizedPivotPoint.x, 0f, 1f, sliderWidthOption);

                            if (GUILayout.Button("<", smallButtonWidthOption)) x = 0.0f;
                            if (GUILayout.Button("-", smallButtonWidthOption)) x = 0.5f;
                            if (GUILayout.Button(">", smallButtonWidthOption)) x = 1.0f;
                        }

                        // Y
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Y", labelWidthOption);
                            y = EditorGUILayout.Slider(me.normalizedPivotPoint.y, 0f, 1f, sliderWidthOption);

                            if (GUILayout.Button("<", smallButtonWidthOption)) y = 0.0f;
                            if (GUILayout.Button("-", smallButtonWidthOption)) y = 0.5f;
                            if (GUILayout.Button(">", smallButtonWidthOption)) y = 1.0f;
                        }

                        // Z
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Z", labelWidthOption);
                            z = EditorGUILayout.Slider(me.normalizedPivotPoint.z, 0f, 1f, sliderWidthOption);

                            if (GUILayout.Button("<", smallButtonWidthOption)) z = 0.0f;
                            if (GUILayout.Button("-", smallButtonWidthOption)) z = 0.5f;
                            if (GUILayout.Button(">", smallButtonWidthOption)) z = 1.0f;
                        }

                        // 소수점 넷째 자리까지 반올림
                        me.normalizedPivotPoint.x = Mathf.Round(x * 10000f) * 0.0001f;
                        me.normalizedPivotPoint.y = Mathf.Round(y * 10000f) * 0.0001f;
                        me.normalizedPivotPoint.z = Mathf.Round(z * 10000f) * 0.0001f;

                        if (cc.changed)
                        {
                            MovePivotPointByNormalizedBounds();
                        }
                    }

                    GUI.backgroundColor = oldBGColor;
                }
            }

            void RecalculateNormalizedPivotPoint()
            {
                me.pivotPos = ClampVector3(me.pivotPos, me.minBounds, me.maxBounds);

                me.normalizedPivotPoint.x = (me.pivotPos.x - me.minBounds.x) / (BoundsSize.x);
                me.normalizedPivotPoint.y = (me.pivotPos.y - me.minBounds.y) / (BoundsSize.y);
                me.normalizedPivotPoint.z = (me.pivotPos.z - me.minBounds.z) / (BoundsSize.z);
            }

            private void DrawSetPivotPosButtons()
            {
                GUI.backgroundColor = LightButtonColor2;
                GUI.skin.button.fontStyle = FontStyle.Bold;

                if(!me.foldOutPivotPos) return;

                if (GUILayout.Button("Reset", safeViewWidthOption, ApplyButtonHeightOption))
                {
                    Undo.RecordObject(me, "Reset Pivot Position");
                    me.pivotPos = me.transform.position;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Bottom Center", safeViewWidthThirdOption, ApplyButtonHeightOption))
                    {
                        Undo.RecordObject(me, "Bottom Center Pivot");
                        me.normalizedPivotPoint = new Vector3(0.5f, 0f, 0.5f);
                        MovePivotPointByNormalizedBounds();
                    }

                    if (GUILayout.Button("Center", safeViewWidthThirdOption, ApplyButtonHeightOption))
                    {
                        Undo.RecordObject(me, "Center Pivot");
                        me.pivotPos = me.transform.position;
                        me.normalizedPivotPoint = new Vector3(0.5f, 0.5f, 0.5f);
                        MovePivotPointByNormalizedBounds();
                    }

                    if (GUILayout.Button("Top Center", safeViewWidthThirdOption, ApplyButtonHeightOption))
                    {
                        Undo.RecordObject(me, "Top Center Pivot");
                        me.pivotPos = me.transform.position;
                        me.normalizedPivotPoint = new Vector3(0.5f, 1.0f, 0.5f);
                        MovePivotPointByNormalizedBounds();
                    }
                }
            }

            private void DrawResetTransformButtons()
            {
                GUI.backgroundColor = LightButtonColor2;
                GUI.skin.button.fontStyle = FontStyle.Bold;

                if (!me.foldOutTransform) return;

                if (GUILayout.Button("All", safeViewWidthOption, ApplyButtonHeightOption))
                {
                    Undo.RecordObject(me.transform, "Reset Transform");
                    me.transform.localPosition = Vector3.zero;
                    me.transform.localRotation = Quaternion.identity;
                    me.transform.localScale = Vector3.one;

                    // 피벗 위치도 함께 이동
                    //Undo.RecordObject(me, "Reset Transform");
                    //me.pivotPos = me.transform.position;

                    RecalculateMeshBounds();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Position", safeViewWidthThirdOption, ApplyButtonHeightOption))
                    {
                        Undo.RecordObject(me.transform, "Reset Position");
                        me.transform.localPosition = Vector3.zero;

                        // 피벗 위치도 함께 이동
                        //Undo.RecordObject(me, "Reset Position");
                        //me.pivotPos = me.transform.position;

                        RecalculateMeshBounds();
                    }

                    if (GUILayout.Button("Rotation", safeViewWidthThirdOption, ApplyButtonHeightOption))
                    {
                        Undo.RecordObject(me.transform, "Reset Rotation");
                        me.transform.localRotation = Quaternion.identity;

                        RecalculateMeshBounds();
                    }

                    if (GUILayout.Button("Scale", safeViewWidthThirdOption, ApplyButtonHeightOption))
                    {
                        Undo.RecordObject(me.transform, "Reset Scale");
                        me.transform.localScale = Vector3.one;

                        RecalculateMeshBounds();
                    }
                }
            }

            private void DrawSaveButtons()
            {
                GUI.backgroundColor = DarkButtonColor2;
                GUI.skin.button.fontStyle = FontStyle.Bold;

                if (!me.foldOutSave) return;

                Undo.RecordObject(me, "Change Mesh Name");
                me.meshName = EditorGUILayout.TextField("Mesh Name", me.meshName, safeViewWidthOption);

                if (string.IsNullOrWhiteSpace(me.meshName))
                {
                    EditorGUILayout.HelpBox("Input Mesh Name", MessageType.Error);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Apply", safeViewWidthHalfOption, ApplyButtonHeightOption))
                    {
                        Undo.RecordObject(me.transform, "Transform");
                        Undo.RecordObject(me, "Click Edit Mesh Button");
                        ApplyToCurrentMesh();
                    }

                    if (GUILayout.Button("Save As Obj File", safeViewWidthHalfOption, ApplyButtonHeightOption))
                    {
                        SaveAsObjFile();
                    }
                }
            }

#pragma warning disable CS0618
            private void DrawPivotPointGizmo()
            {
                Handles.color = HandleColor;
                // Handles.DrawSphere(0, me.pivotPos, Quaternion.identity, HandleUtility.GetHandleSize(me.pivotPos) * 0.12f);
                Handles.SphereHandleCap(
                0,
                me.pivotPos,
                Quaternion.identity,
                HandleUtility.GetHandleSize(me.pivotPos) * 0.12f,
                EventType.Repaint
                );
            }

            private void DrawPivotHandle()
            {
                float size = HandleUtility.GetHandleSize(me.pivotPos) * 0.6f;
                Handles.color = HandleColor;

                Undo.RecordObject(me, "Move Pivot Position");
                me.pivotPos = Handles.Slider(me.pivotPos, Vector3.right,   size, Handles.ArrowHandleCap, 1f); // +X
                me.pivotPos = Handles.Slider(me.pivotPos, Vector3.left,    size, Handles.ArrowHandleCap, 1f); // -X
                me.pivotPos = Handles.Slider(me.pivotPos, Vector3.up,      size, Handles.ArrowHandleCap, 1f); // +Y
                me.pivotPos = Handles.Slider(me.pivotPos, Vector3.down,    size, Handles.ArrowHandleCap, 1f); // -Y
                me.pivotPos = Handles.Slider(me.pivotPos, Vector3.forward, size, Handles.ArrowHandleCap, 1f); // +Z
                me.pivotPos = Handles.Slider(me.pivotPos, Vector3.back,    size, Handles.ArrowHandleCap, 1f); // -Z

                // Snap 
                if (me.snapMode && me.snapValue > 0f)
                {
                    me.pivotPos = SnapVector3(me.pivotPos, me.snapValue);
                }
            }
#pragma warning restore CS0618

            private void DrawBounds()
            {
                if (me.confineInBounds)
                    Handles.color = ConfinedColor;
                else
                    Handles.color = BoundsColor;

                Handles.DrawWireCube(BoundsCenter, BoundsSize);
            }

            private int windowID = 99;
            private Rect windowRect;
            private void DrawSceneGUI()
            {
                const float width = 160f;
                const float height = 100f;
                const float paddingX = 70f;
                const float paddingY = 30f;

                windowRect = new Rect
                (
                    Screen.width  - width  - paddingX, 
                    Screen.height - height - paddingY, 
                    width,
                    height
                );

                windowRect = GUILayout.Window(windowID, windowRect, (id) => {

                    EditorGUILayout.Space(4f);

                    Undo.RecordObject(me, "Move Pivot Position");
                    Vector3 pivotPos = EditorGUILayout.Vector3Field("", me.pivotPos);

                    if (me.snapMode && me.snapValue > 0f)
                    {
                        me.pivotPos = SnapVector3(pivotPos, me.snapValue);
                    }
                    else
                    {
                        me.pivotPos = SnapVector3(pivotPos, 0.0001f);
                    }

                    EditorGUILayout.Space(4f);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Snap", GUILayout.Width(32f));
                        me.snapMode = EditorGUILayout.ToggleLeft("", me.snapMode, GUILayout.Width(16f));
                    }

                    using (new EditorGUI.DisabledGroupScope(!me.snapMode))
                    {
                        float snap = EditorGUILayout.Slider("", me.snapValue, 0f, 1f);
                        me.snapValue = Mathf.Round(snap / 0.05f) * 0.05f;
                    }

                    //GUI.DragWindow();

                }, "Pivot Position");
            }

            /// <summary> 변경사항 적용한 새로운 메시 생성 </summary>
            private Mesh EditMesh()
            {
                Matrix4x4 mat = me.transform.localToWorldMatrix;

                Mesh srMesh = me.meshFilter.sharedMesh;
                Mesh newMesh = new Mesh();

                int vertCount = srMesh.vertexCount;
                int trisCount = srMesh.triangles.Length;

                Vector3[] verts = new Vector3[vertCount];
                Vector3[] normals = new Vector3[vertCount];
                Vector4[] tangents = new Vector4[vertCount];
                Vector2[] uvs = new Vector2[vertCount];
                int[] tris = new int[trisCount];

                for (int i = 0; i < vertCount; i++)
                {
                    verts[i] = mat.MultiplyPoint(srMesh.vertices[i]) - me.pivotPos;
                    normals[i] = mat.MultiplyVector(srMesh.normals[i]);
                    tangents[i] = mat.MultiplyVector(srMesh.tangents[i]);
                    uvs[i] = srMesh.uv[i];
                }
                for (int i = 0; i < trisCount; i++)
                {
                    tris[i] = srMesh.triangles[i];
                }

                newMesh.vertices = verts;
                newMesh.normals = normals;
                newMesh.tangents = tangents;
                newMesh.triangles = tris;
                newMesh.uv = uvs;

                // UV2
                int uv2Len = srMesh.uv2.Length;
                if (uv2Len > 0)
                {
                    Vector2[] uv2 = new Vector2[uv2Len];
                    for (int i = 0; i < uv2Len; i++)
                        uv2[i] = srMesh.uv2[i];
                    newMesh.uv2 = uv2;
                }

                newMesh.RecalculateBounds();

                me.transform.localPosition = me.pivotPos;
                me.transform.localRotation = Quaternion.identity;
                me.transform.localScale = Vector3.one;

                me.editMode = false;
                me.pivotEditMode = false;
                Tools.current = Tool.Move;

                return newMesh;
            }

            private void ApplyToCurrentMesh()
            {
                Mesh newMesh = EditMesh();
                newMesh.name = me.meshName;

                Undo.RecordObject(me.meshFilter, "Edit Mesh");
                me.meshFilter.sharedMesh = newMesh;
            }

            private void SaveAsObjFile()
            {
                string meshName = me.meshName;
                string path =
                    UnityEditor.EditorUtility.SaveFilePanelInProject("Save Mesh As Obj File", meshName, "obj", "");

                if(string.IsNullOrWhiteSpace(path))
                    return;

                Mesh newMesh = EditMesh();
                ObjExporter.SaveMeshToFile(newMesh, me.meshRenderer, meshName, path);
                AssetDatabase.Refresh();
            }

            /// <summary> 벡터를 일정 단위로 끊기 </summary>
            private Vector3 SnapVector3(Vector3 vec, float snapValue)
            {
                if(snapValue <= 0f) return vec;

                vec.x = Mathf.Round(vec.x / snapValue) * snapValue;
                vec.y = Mathf.Round(vec.y / snapValue) * snapValue;
                vec.z = Mathf.Round(vec.z / snapValue) * snapValue;
                return vec;
            }

            private Vector3 ClampVector3(Vector3 vec, in Vector3 min, in Vector3 max)
            {
                vec.x = Mathf.Clamp(vec.x, min.x, max.x);
                vec.y = Mathf.Clamp(vec.y, min.y, max.y);
                vec.z = Mathf.Clamp(vec.z, min.z, max.z);
                return vec;
            }

            private void RecalculateMeshBounds()
            {
                Mesh mesh = me.meshFilter.sharedMesh;

                me.minBounds = Vector3.positiveInfinity;
                me.maxBounds = Vector3.negativeInfinity;

                foreach (var vert in mesh.vertices)
                {
                    Vector3 v = me.transform.TransformPoint(vert);

                    if (v.x > me.maxBounds.x) me.maxBounds.x = v.x;
                    else if (v.x < me.minBounds.x) me.minBounds.x = v.x;

                    if (v.y > me.maxBounds.y) me.maxBounds.y = v.y;
                    else if (v.y < me.minBounds.y) me.minBounds.y = v.y;

                    if (v.z > me.maxBounds.z) me.maxBounds.z = v.z;
                    else if (v.z < me.minBounds.z) me.minBounds.z = v.z;
                }
            }

            private void MovePivotPointByNormalizedBounds()
            {
                me.pivotPos.x = Mathf.Lerp(me.minBounds.x, me.maxBounds.x, me.normalizedPivotPoint.x);
                me.pivotPos.y = Mathf.Lerp(me.minBounds.y, me.maxBounds.y, me.normalizedPivotPoint.y);
                me.pivotPos.z = Mathf.Lerp(me.minBounds.z, me.maxBounds.z, me.normalizedPivotPoint.z);
            }

            private void DrawOutlinedBox(float y, float contentHeight)
            {
                const float x = HeaderBoxPosX;
                ref float width = ref boxHeaderWidth;

                const float ow = HeaderBoxOutlineWidth;
                float ow2 = ow * 2f;

                Rect outRect = new Rect(x - ow, y - ow, width + ow2, contentHeight + ow2);
                Rect inRect  = new Rect(x, y, width, contentHeight);

                EditorGUI.DrawRect(outRect, MainBoxOutlineColor);
                EditorGUI.DrawRect(inRect, MainBoxContentColor);
            }

            /// <summary> 헤더가 존재하는 박스 그리기 </summary>
            private void DrawOutlinedHeaderBox(in string headerText, float y, float contentHeight, ref bool foldOut,
                int colorType = 0)
            {
                const float x = HeaderBoxPosX;
                const float headerHeight = HeaderBoxHeaderHeight;
                ref float width = ref boxHeaderWidth;

                const float ow = HeaderBoxOutlineWidth;
                float ow2 = ow * 2f;
                float ow3 = ow * 3f;
                float height = headerHeight + contentHeight;

                Rect headRect = new Rect(x, y, width, headerHeight);

                // Foldout Button
                var cc = GUI.color;
                GUI.color = new Color(0,0,0,0);
                if(GUI.Button(headRect, "")) foldOut = !foldOut;
                if(!foldOut) contentHeight = 0f;
                GUI.color = cc;

                Rect outRect  = new Rect(x - ow, y - ow, width + ow2, height + ow3);
                Rect outRect2 = new Rect(x - ow, y - ow, width + ow2, headerHeight + ow2);
                Rect contRect = new Rect(x, y + headerHeight + ow, width, contentHeight);

                // Mouse Over - Change Color
                Color headColor = BoxHeaderColors[colorType];
                if (headRect.Contains(Event.current.mousePosition))
                {
                    headColor += new Color(0.1f, 0.1f, 0.2f);
                }

                // Draw Rects
                if (foldOut) EditorGUI.DrawRect(outRect,  BoxOutlineColors[colorType]);
                EditorGUI.DrawRect(outRect2, BoxOutlineColors[colorType]);
                EditorGUI.DrawRect(headRect, headColor);
                EditorGUI.DrawRect(contRect, BoxContentColors[colorType]);

                EditorGUI.LabelField(headRect, headerText, headerBoxLabelStyle);
            }
        }
    }
}

#endif