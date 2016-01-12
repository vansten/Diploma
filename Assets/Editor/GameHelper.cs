using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

public class GameHelper : EditorWindow
{
    private static GameHelper _helperWindow;

    private GameObject _parentObject;
    private ReorderableList _sprites;

    [MenuItem("Window/Sprite setter")]
    public static void Init()
    {
        _helperWindow = GetWindow<GameHelper>();
        _helperWindow.name = "Sprite setter";
        _helperWindow._sprites = new ReorderableList(new List<Sprite>(), typeof(Sprite));
        _helperWindow._sprites.drawElementCallback = _helperWindow.DrawElement;
        _helperWindow.Show();
    }

    void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        _sprites.list[index] = (Sprite)EditorGUILayout.ObjectField((Object)_sprites.list[index], typeof(Sprite), false);
    }

    void OnGUI()
    {
        _parentObject = (GameObject)EditorGUILayout.ObjectField(_parentObject, typeof(GameObject), true);
        if(_parentObject != null)
        {
            _sprites.DoLayoutList();
            if(_sprites.list.Count > 0)
            {
                if (GUILayout.Button("Set sprites"))
                {
                    SpriteRenderer[] srs = _parentObject.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer sr in srs)
                    {
                        sr.sprite = (Sprite)_sprites.list[Random.Range(0, _sprites.list.Count)];
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please, select sprites you want to set", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please, select parent object of all sprites you want to set", MessageType.Info);
        }
    }
}