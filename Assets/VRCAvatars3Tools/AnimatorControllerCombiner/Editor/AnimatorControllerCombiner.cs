﻿using UnityEngine;
using UnityEditor;
using System.Linq;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using Gatosyocora.VRCAvatars3Tools.Utilitys;

// ver 1.1.1
// Copyright (c) 2020 gatosyocora
// MIT License. See LICENSE.txt

namespace Gatosyocora.VRCAvatars3Tools
{
    public class AnimatorControllerCombiner : EditorWindow
    {
        private AnimatorController srcController;
        private AnimatorController dstController;

        private bool[] isCopyLayers;
        private bool[] isCopyParameters;

        private int executionType = 0;
        private string[] executionTypeOptions = new string[] {
            "Add",
            "Insert"
        };

        private Vector2 srcControllerScrollPos = Vector2.zero;
        private Vector2 dstControllerScrollPos = Vector2.zero;

        [MenuItem("VRCAvatars3Tools/AnimatorControllerCombiner")]
        public static void Open()
        {
            GetWindow<AnimatorControllerCombiner>("AnimatorControllerCombiner");
        }

        private void OnGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                srcController = EditorGUILayout.ObjectField("Src AnimatorController", srcController, typeof(AnimatorController), true) as AnimatorController;
                if (check.changed)
                {
                    if (srcController != null)
                    {
                        isCopyLayers = Enumerable.Range(1, srcController.layers.Length)
                                            .Select(i => true)
                                            .ToArray();
                        isCopyParameters = Enumerable.Range(1, srcController.parameters.Length)
                                                .Select(i => true)
                                                .ToArray();
                    }
                }
            }
            if (srcController != null)
            {
                using (new EditorGUI.IndentLevelScope())
                using (var scroll = new EditorGUILayout.ScrollViewScope(srcControllerScrollPos, new GUIStyle(), new GUIStyle("verticalScrollbar")))
                {
                    srcControllerScrollPos = scroll.scrollPosition;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
                            for (int i = 0; i < srcController.layers.Length; i++)
                            {
                                var layer = srcController.layers[i];
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    isCopyLayers[i] = EditorGUILayout.ToggleLeft(layer.name, isCopyLayers[i]);
                                }
                            }
                        }
                        GUILayout.FlexibleSpace();
                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
                            for (int i = 0; i < srcController.parameters.Length; i++)
                            {
                                var parameter = srcController.parameters[i];
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    isCopyParameters[i] = EditorGUILayout.ToggleLeft($"[{parameter.type}]{parameter.name}", isCopyParameters[i]);
                                }
                            }
                        }
                    }
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Copy ↓", GUILayout.Width(60f));
                GUIStyle style_radio = new GUIStyle(EditorStyles.radioButton);
                executionType = GUILayout.SelectionGrid(executionType, executionTypeOptions, 2, style_radio);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.Space();

            dstController = EditorGUILayout.ObjectField("Dst AnimatorController", dstController, typeof(AnimatorController), true) as AnimatorController;
            if (dstController != null)
            {
                using (new EditorGUI.IndentLevelScope())
                using (var scroll = new EditorGUILayout.ScrollViewScope(dstControllerScrollPos, new GUIStyle(), new GUIStyle("verticalScrollbar")))
                {
                    dstControllerScrollPos = scroll.scrollPosition;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
                            foreach (var layer in dstController.layers)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField(layer.name);
                                }
                            }
                        }
                        GUILayout.FlexibleSpace();
                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
                            foreach (var parameter in dstController.parameters)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField($"[{parameter.type}]{parameter.name}");
                                }
                            }
                        }
                    }
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledGroupScope(!srcController || !dstController))
            {
                if (GUILayout.Button("Combine"))
                {
                    if (this.executionType == 0)
                    {
                        for (int i = 0; i < srcController.layers.Length; i++)
                        {
                            if (!isCopyLayers[i]) continue;
                            AnimatorControllerUtility.AddLayer(dstController, srcController.layers[i], i == 0);
                        }

                        for (int i = 0; i < srcController.parameters.Length; i++)
                        {
                            if (!isCopyParameters[i]) continue;
                            AnimatorControllerUtility.AddParameter(dstController, srcController.parameters[i]);
                        }
                    } else
                    {
                        int dstLayerCounts = dstController.layers.Length;
                        for (int i = 0; i < srcController.layers.Length; i++)
                        {
                            if (!isCopyLayers[i]) continue;
                            AnimatorControllerUtility.AddLayer(dstController, srcController.layers[i], i == 0);
                        }
                        for (int i = 0; i < dstLayerCounts; i++)
                        {
                            AnimatorControllerUtility.AddLayer(dstController, dstController.layers[i], i == 0);
                        }
                        for (int i = 0; i < dstLayerCounts; i++)
                        {
                            var stateMachine = dstController.layers[0].stateMachine;
                            dstController.RemoveLayer(0);
                            if (stateMachine != null)
                            {
                                AnimatorControllerUtility.RemoveObjectsInStateMachineToAnimatorController(stateMachine);
                            }
                        }

                        for (int i = 0; i < srcController.parameters.Length; i++)
                        {
                            if (!isCopyParameters[i]) continue;
                            AnimatorControllerUtility.AddParameter(dstController, srcController.parameters[i]);
                        }

                    }
                }
            }

            EditorGUILayout.Space();
        }
    }
}