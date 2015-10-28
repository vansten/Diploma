using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Collections;

public class BehaviorTreeEditor : EditorWindow
{
    public static BehaviorTreeEditor BTEditorWindow;
    private static IBTElementDrawer _drawer;

    private List<string> _drawers = new List<string>();
    private string _selectedDrawer = "";

    private TextAsset _selectedAsset;
    private TextAsset _prevSelectedAsset;
    private BehaviorTree _behaviorTree = null;
    private bool _isXMLFile = false;
    private string _behaviorTreeName;
    private bool _autosave = false;

    private List<string> _allMethods = new List<string>();
    List<string> _decoratorTypes = new List<string>();

    public bool Autosave
    {
        get { return _autosave; }
    }

    [MenuItem("Window/BT Editor")]
    [MenuItem("Behavior Tree/Open Editor")]
    public static void Open()
    {
        BTEditorWindow = GetWindow<BehaviorTreeEditor>();
        BTEditorWindow.Init();
        BehaviorTreeEditorSettings.Init();
    }

    void Init()
    {
        _selectedDrawer = PlayerPrefs.GetString("BTElementDrawer", "BTSimpleDrawer");
        Type drawerType = Type.GetType(_selectedDrawer);
        if(drawerType == null)
        {
            drawerType = Type.GetType("BTSimpleDrawer");
        }
        _drawer = (IBTElementDrawer)Activator.CreateInstance(Type.GetType(_selectedDrawer));

        Assembly ass = Assembly.GetAssembly(typeof(BehaviorTree));
        Type[] allTypes = ass.GetTypes();
        List<Type> types = new List<Type>(allTypes);
        Assembly ass2 = Assembly.GetAssembly(typeof(BehaviorTreeEditor));
        types.AddRange(ass2.GetTypes());
        allTypes = types.ToArray();
        
        //Fill drawers info and methods list
        foreach(Type t in allTypes)
        {
            if (t.IsInterface || t.GetInterface("INode") != null || t.FullName.Contains("+"))
            {
                continue;
            }
            else
            {
                if (t.GetInterface("IBTElementDrawer") != null)
                {
                    _drawers.Add(t.Name);
                }
                else
                {
                    MethodInfo[] mis = t.GetMethods();
                    foreach (MethodInfo mi in mis)
                    {
                        if (mi.ReturnType == typeof(TaskStatus) && mi.IsStatic)
                        {
                            ParameterInfo[] pis = mi.GetParameters();
                            if (pis.Length == 1)
                            {
                                if (pis[0].ParameterType == typeof(GameObject))
                                {
                                    string type = t.ToString();
                                    string methodName = mi.Name;
                                    string newMethod = type + "." + methodName;
                                    _allMethods.Add(newMethod);
                                }
                            }
                        }
                    }
                }
            }
        }

        //General settings
        BTEditorWindow.titleContent.text = "BT Editor";
        Rect windowRect = new Rect(50.0f, 50.0f, 1000.0f, 800.0f);
        BTEditorWindow.position = windowRect;
        BTEditorWindow._behaviorTree = null;

        //Fill decorator type list
        Array dt = Enum.GetValues(typeof(DecoratorType));
        foreach (DecoratorType dtv in dt)
        {
            _decoratorTypes.Add(dtv.ToString());
        }

        _drawer.SetDecoratorsList(_decoratorTypes);
        _drawer.SetMethodsList(_allMethods);
    }

    void OnGUI()
    {
        DrawSelectDrawer();
        _selectedAsset = (TextAsset)EditorGUILayout.ObjectField("Behavior Tree asset: ", _selectedAsset, typeof(TextAsset), false, BehaviorTreeEditorSettings.editableFieldsWidth);
        _autosave = EditorGUILayout.Toggle("Autosave: ", _autosave);
        if(_selectedAsset != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Behavior Tree name:", BehaviorTreeEditorSettings.labelWidth);
            EditorGUILayout.LabelField(_behaviorTreeName, BehaviorTreeEditorSettings.labelWidth);
            EditorGUILayout.EndHorizontal();
            if (_prevSelectedAsset != _selectedAsset)
            {
                FileSelectedChanged();
            }

            if(_isXMLFile)
            {
                if(_behaviorTree != null)
                {
                    if (GUILayout.Button("Save", BehaviorTreeEditorSettings.ButtonWidth))
                    {
                        Save();
                    }

                    if(_drawer != null)
                    {
                        _drawer.DrawBehaviorTree(_behaviorTree);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("That's not a BT", BehaviorTreeEditorSettings.labelWidth);
                }
            }
            else
            {
                _behaviorTree = null;
                EditorGUILayout.LabelField("Only XML files can contain Behavior Tree :)", BehaviorTreeEditorSettings.labelWidth);
            }
        }
        else
        {
            _behaviorTreeName = EditorGUILayout.TextField("Behavior Tree Name: ", _behaviorTreeName, BehaviorTreeEditorSettings.editableFieldsWidth);
            bool disabled = _behaviorTreeName == null || _behaviorTreeName.Length == 0;
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Create Behavior Tree", BehaviorTreeEditorSettings.ButtonWidth))
            {
                _behaviorTree = new BehaviorTree();
                string path = @"Assets/" + _behaviorTreeName + ".xml";
                BTSerializer.Serialize(_behaviorTree, path);
                AssetDatabase.Refresh();
                _selectedAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                FileSelectedChanged();
            }
            EditorGUI.EndDisabledGroup();

        }
        _prevSelectedAsset = _selectedAsset;
    }

    public void Save()
    {
        BTSerializer.Serialize(_behaviorTree, AssetDatabase.GetAssetPath(_selectedAsset.GetInstanceID()));
        AssetDatabase.Refresh();
    }

    private void FileSelectedChanged()
    {
        string path = AssetDatabase.GetAssetPath(_selectedAsset.GetInstanceID());
        _isXMLFile = path.EndsWith(".xml");
        if (_isXMLFile)
        {
            _behaviorTree = BTSerializer.Deserialize(_selectedAsset);
            if(_behaviorTree != null)
            {
                string nameExt = path.Substring(path.LastIndexOf('/') + 1);
                nameExt = nameExt.Substring(0, nameExt.LastIndexOf('.'));
                _behaviorTreeName = nameExt;
            }
        }
    }    

    void DrawSelectDrawer()
    {
        int selected = _drawers.IndexOf(_selectedDrawer);
        int nSelected = EditorGUILayout.Popup(selected, _drawers.ToArray(), BehaviorTreeEditorSettings.labelWidth);
        if(nSelected != selected)
        {
            _selectedDrawer = _drawers[nSelected];
            _drawer = (IBTElementDrawer)Activator.CreateInstance(Type.GetType(_selectedDrawer));
            _drawer.SetDecoratorsList(_decoratorTypes);
            _drawer.SetMethodsList(_allMethods);
            PlayerPrefs.SetString("BTElementDrawer", _selectedDrawer);
            PlayerPrefs.Save();
        }
    }
}
