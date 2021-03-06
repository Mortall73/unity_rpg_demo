using Gameplay.Quests;
using Gameplay.Quests.Conditions;
using GameSystems;
using UnityEditor;
using UnityEngine;

namespace Editor.Scene
{
    [InitializeOnLoad]
    public static class Settings
    {
        [MenuItem("GameObject/Scene/SceneSettings", false, 10)]
        static void CreateSceneSettings(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("SceneSettings");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            go.AddComponent<SceneSettings>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        
        [MenuItem("GameObject/Scene/LevelSettings", false, 10)]
        static void CreateSceneLevelSettings(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("LevelSettings");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            go.AddComponent<LevelSettings>();
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}