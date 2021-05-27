#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 날짜 : 2021-05-16 PM 3:34:18
// 작성자 : Rito

namespace Rito.EditorUtilities
{
    [DisallowMultipleComponent]
    public partial class PivotEditor : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private Vector3 pivotPos = Vector3.zero;
        [SerializeField] private bool editMode;
        [SerializeField] private bool pivotEditMode;
        [SerializeField] private bool snapMode;
        [SerializeField] private float snapValue = 0.1f;
        [SerializeField] private bool hideTransformTool;

        [SerializeField] private bool showBounds;
        [SerializeField] private bool confineInBounds;

        [SerializeField] private Vector3 minBounds;
        [SerializeField] private Vector3 maxBounds;
        [SerializeField] private Vector3 normalizedPivotPoint; // 0~1 내로 조정

        [SerializeField] private string meshName;

        [SerializeField] private bool foldOutOptions = true;
        [SerializeField] private bool foldOutBounds = true;
        [SerializeField] private bool foldOutPivotPos = true;
        [SerializeField] private bool foldOutTransform = true;
        [SerializeField] private bool foldOutSave = true;

        private void Reset()
        {
            TryGetComponent(out meshFilter);
            TryGetComponent(out meshRenderer);
        }

        // ==============================================================
        private const int ContextPriority = 100;

        [MenuItem("CONTEXT/MeshFilter/Edit Pivot", false, ContextPriority)]
        private static void Context_AddMeshEditor(MenuCommand mc)
        {
            var component = mc.context as Component;
            var me = component.gameObject.AddComponent<PivotEditor>();
            PutComponentOnTop(me);
        }

        [MenuItem("CONTEXT/MeshFilter/Edit Pivot", true, ContextPriority)]
        private static bool Context_AddMeshEditor_Validate(MenuCommand mc)
        {
            var component = mc.context as Component;
            PivotEditor me = component.GetComponent<PivotEditor>();
            return me == null;
        }

        /// <summary> 컴포넌트를 최상단에 올리기 </summary>
        private static void PutComponentOnTop(Component component)
        {
            for (int i = 0; i < 100 && UnityEditorInternal.ComponentUtility.MoveComponentUp(component); i++);
        }
    }
}

#else

using UnityEngine;
public class PivotEditor : MonoBehaviour
{
    private void Awake()
    {
        Destroy(this);
    }
}

#endif