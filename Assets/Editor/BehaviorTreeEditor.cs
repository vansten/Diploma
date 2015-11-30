using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Collections;
using System.Linq;
using System.IO;

public class BehaviorTreeEditor : EditorWindow
{
    public static BehaviorTreeEditor BTEditorWindow;
    private static IBTElementDrawer _drawer;

    private List<string> _drawers = new List<string>();
    private string _selectedDrawer = "";

    private TextAsset _selectedAsset;
    private TextAsset _prevSelectedAsset;
    private BehaviorTree _behaviorTree = null;
    private GameObject _selectedGameObject;
    private bool _isXMLFile = false;
    private string _behaviorTreeName;
    private bool _autosave = false;

    private List<string> _allMethods = new List<string>();
    List<string> _decoratorTypes = new List<string>();

    private EditorState _state;
    private GUIStyle _headerStyle = null;
    private GUIStyle _menuStyle = null;

    private string _selectionErrorMessage;

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
        BehaviorTreeEditorSettings.Instance.Load();
        BehaviorTreeEditorSettings.Instance.Init();
    }

    void Init()
    {
        EditorState[] stateValues = (EditorState[])Enum.GetValues(typeof(EditorState));
        int currentState = PlayerPrefs.GetInt("BTEditorState", 0);
        _state = stateValues[currentState];
        _selectedDrawer = PlayerPrefs.GetString("BTElementDrawer", "BTWindowsDrawer");
        _autosave = PlayerPrefs.GetInt("BTAutosave", 0) == 1;

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
            }
        }

        //General settings
        BTEditorWindow.titleContent.text = "BT Editor";
        Rect windowRect = new Rect(50.0f, 50.0f, 1000.0f, 800.0f);
        BTEditorWindow.position = windowRect;
        BTEditorWindow._behaviorTree = null;
        BTEditorWindow.minSize = new Vector2(800, 600);

        //Fill decorator type list
        Array dt = Enum.GetValues(typeof(DecoratorType));
        foreach (DecoratorType dtv in dt)
        {
            _decoratorTypes.Add(dtv.ToString());
        }

        _drawer.SetDecoratorsList(_decoratorTypes);
        Selection.selectionChanged += SelectionChanged;

        SelectionChanged();
    }

    void OnGUI()
    {
        if(_headerStyle == null)
        {
            _headerStyle = GUI.skin.label;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.fontSize = 15;
        }

        if(_menuStyle == null)
        {
            _menuStyle = GUI.skin.box;
            _menuStyle.normal.background = new Texture2D(1, 1);
            Color[] color = new Color[1];
            color[0] = new Color(0.55f, 0.55f, 0.55f, 1.0f);
            _menuStyle.normal.background.SetPixels(color);
        }

        EditorGUILayout.BeginHorizontal();
        DrawMenu();
        EditorGUILayout.BeginVertical();

        
        
        GUILayout.BeginArea(new Rect(BehaviorTreeEditorSettings.Instance.SideMenuRect.width + 10, 10, position.width - BehaviorTreeEditorSettings.Instance.SideMenuRect.width - 10, position.height - 10));
        switch (_state)
        {
            case EditorState.DrawBehaviorTree:
                DrawBehaviorTree();
                break;
            case EditorState.DrawSettings:
                DrawSettings();
                break;
        }
        GUILayout.EndArea();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    void SelectionChanged()
    {
        if (Selection.gameObjects.Length > 0)
        {
            if (Selection.gameObjects.Length > 1)
            {
                _selectedGameObject = null;
                _selectionErrorMessage = "Multiediting not supported";
            }
            else
            {
                if (_selectedGameObject != Selection.gameObjects[0])
                {
                    BehaviorTreeComponent btc = Selection.gameObjects[0].GetComponent<BehaviorTreeComponent>();
                    if (btc == null)
                    {
                        _selectedGameObject = null;
                        _selectionErrorMessage = "Selected game object doesn't have BehaviorTreeComponent attached";
                    }
                    else
                    {
                        _allMethods.Clear();

                        Component[] comps = Selection.gameObjects[0].GetComponents<MonoBehaviour>();
                        foreach (Component c in comps)
                        {
                            Type t = c.GetType();
                            MethodInfo[] mis = new List<MethodInfo>(t.GetMethods()).Where(mi => mi.ReturnType == typeof(TaskStatus)).ToArray();
                            foreach (MethodInfo mi in mis)
                            {
                                if (mi.ReturnType != typeof(TaskStatus))
                                {
                                    continue;
                                }
                                ParameterInfo[] pis = mi.GetParameters();
                                if (pis.Length == 2)
                                {
                                    if (pis[0].ParameterType == typeof(GameObject) && pis[1].ParameterType == typeof(Blackboard))
                                    {
                                        string type = t.ToString();
                                        string methodName = mi.Name;
                                        string newMethod = type + "." + methodName;
                                        _allMethods.Add(newMethod);
                                    }
                                }
                            }
                        }

                        _drawer.SetMethodsList(_allMethods);

                        _selectedGameObject = Selection.gameObjects[0];
                        _selectedAsset = btc.BehaviorTreeAsset;
                        FileSelectedChanged();
                    }
                }
            }
        }
        else
        {
            _selectedGameObject = null;
            _selectionErrorMessage = "Please, select some object on scene with BehaviorTreeComponent attached";
        }

        Repaint();
    }

    void OnDestroy()
    {
        Selection.selectionChanged -= SelectionChanged;
    }

    public void Save()
    {
        if(!BTSerializer.Serialize(_behaviorTree, AssetDatabase.GetAssetPath(_selectedAsset.GetInstanceID())))
        {
            EditorUtility.DisplayDialog("ERROR", "Saving behavior tree failed, please look at console window", "OK");
        }
        AssetDatabase.Refresh();
    }

    private void FileSelectedChanged()
    {
        _behaviorTreeName = "";
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
                _behaviorTree.Initialize(_selectedGameObject, _selectedGameObject.GetComponent<BehaviorTreeComponent>().BlackboardObject, true);
            }
        }
    }   
    
    private void DrawMenu()
    {
        EditorGUILayout.BeginVertical();
        BehaviorTreeEditorSettings.Instance.SideMenuRect.height = position.height;
        GUILayout.BeginArea(BehaviorTreeEditorSettings.Instance.SideMenuRect, _menuStyle);

        if(GUI.Button(new Rect(10, 10, BehaviorTreeEditorSettings.Instance.SideMenuRect.width - 20, 20), "BT Editor"))
        {
            _state = EditorState.DrawBehaviorTree;
            List<EditorState> editorStates = new List<EditorState>((EditorState[])Enum.GetValues(typeof(EditorState)));
            PlayerPrefs.SetInt("BTEditorState", editorStates.IndexOf(_state));
            PlayerPrefs.Save();
        }
        if (GUI.Button(new Rect(10, 40, BehaviorTreeEditorSettings.Instance.SideMenuRect.width - 20, 20), "Settings"))
        {
            _state = EditorState.DrawSettings;
            List<EditorState> editorStates = new List<EditorState>((EditorState[])Enum.GetValues(typeof(EditorState)));
            PlayerPrefs.SetInt("BTEditorState", editorStates.IndexOf(_state));
            PlayerPrefs.Save();
        }

        float space = 60.0f;
        GUILayout.Space(space);
        Handles.BeginGUI();
        Handles.DrawLine(new Vector3(0, 1.3f * space, 0), new Vector3(BehaviorTreeEditorSettings.Instance.SideMenuRect.width - 1, 1.3f * space, 0));
        Handles.EndGUI();
        GUILayout.Space(0.3f * space);
        if (_state == EditorState.DrawBehaviorTree)
        {
            bool prevAutosave = _autosave;
            _autosave = EditorGUILayout.Toggle("Autosave: ", _autosave);
            if(prevAutosave != _autosave)
            {
                PlayerPrefs.SetInt("BTAutosave", _autosave ? 1 : 0);
                PlayerPrefs.Save();
            }
            DrawSelectDrawer();
        }
        else if(_state == EditorState.DrawSettings)
        {
            if(GUI.Button(new Rect(10, 90, BehaviorTreeEditorSettings.Instance.SideMenuRect.width - 20, 20), "Apply"))
            {
                BehaviorTreeEditorSettings.Instance.Save();
            }
        }
        GUILayout.EndArea();
        EditorGUILayout.EndVertical();
    } 

    private void DrawSelectDrawer()
    {
        int selected = _drawers.IndexOf(_selectedDrawer);
        int nSelected = EditorGUILayout.Popup(selected, _drawers.ToArray(), GUILayout.Width(165));
        if(nSelected != selected)
        {
            _selectedDrawer = _drawers[nSelected];
            _drawer = (IBTElementDrawer)Activator.CreateInstance(Type.GetType(_selectedDrawer));
            _drawer.SetDecoratorsList(_decoratorTypes);
            PlayerPrefs.SetString("BTElementDrawer", _selectedDrawer);
            PlayerPrefs.Save();
        }
    }

    private void DrawBehaviorTree()
    {
        if(_selectedGameObject == null)
        {
            EditorGUILayout.LabelField(_selectionErrorMessage);
            return;
        }

        if (_selectedAsset != null)
        {
            EditorGUILayout.LabelField("Behavior Tree name: " + _behaviorTreeName, BehaviorTreeEditorSettings.Instance.LabelOption);
            if (_prevSelectedAsset != _selectedAsset)
            {
                FileSelectedChanged();
            }

            if (_isXMLFile)
            {
                if (_behaviorTree != null)
                {
                    if (GUILayout.Button("Save", BehaviorTreeEditorSettings.Instance.ButtonOption))
                    {
                        Save();
                    }

                    if (_drawer != null)
                    {
                        _drawer.DrawBehaviorTree(_behaviorTree);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("That's not a BT", BehaviorTreeEditorSettings.Instance.LabelOption);
                }
            }
            else
            {
                _behaviorTree = null;
                EditorGUILayout.LabelField("Only XML files can contain Behavior Tree :)", BehaviorTreeEditorSettings.Instance.LabelOption);
            }
        }
        else
        {
            _behaviorTreeName = EditorGUILayout.TextField("Behavior Tree Name: ", _behaviorTreeName, BehaviorTreeEditorSettings.Instance.EditableFieldsOption);
            bool disabled = _behaviorTreeName == null || _behaviorTreeName.Length == 0;
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Create Behavior Tree", BehaviorTreeEditorSettings.Instance.ButtonOption))
            {
                _behaviorTree = new BehaviorTree();
                string path = EditorUtility.SaveFilePanel("Choose your behavior tree path", @"Assets/", _behaviorTreeName + ".xml", "xml");
                BTSerializer.Serialize(_behaviorTree, path);
                AssetDatabase.Refresh();
                string relPath = @"Assets\" + GetRelativePath(path, Application.dataPath);
                _selectedAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(@relPath);
                _selectedGameObject.GetComponent<BehaviorTreeComponent>().BehaviorTreeAsset = _selectedAsset;
                int lastInfexOfBackslash = relPath.LastIndexOf("\\");
                int lastIndexOfDot = relPath.LastIndexOf(".");
                int length = lastIndexOfDot - lastInfexOfBackslash;
                _behaviorTreeName = relPath.Substring(lastInfexOfBackslash + 1, length);
                FileSelectedChanged();
            }
            EditorGUI.EndDisabledGroup();

        }
        _prevSelectedAsset = _selectedAsset;
    }

    private string GetRelativePath(string filespec, string folder)
    {
        Uri pathUri = new Uri(filespec);
        // Folders must end in a slash
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            folder += Path.DirectorySeparatorChar;
        }
        Uri folderUri = new Uri(folder);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
    }

    private void DrawSettings()
    {
        EditorGUILayout.LabelField("General settings", _headerStyle, new GUILayoutOption[] { BehaviorTreeEditorSettings.Instance.LabelOption, GUILayout.Height(20) });
        BehaviorTreeEditorSettings.Instance.ButtonWidth = Mathf.Clamp(EditorGUILayout.FloatField("Buttons width: ", BehaviorTreeEditorSettings.Instance.ButtonWidth, BehaviorTreeEditorSettings.Instance.EditableFieldsOption), 100, float.MaxValue);
        BehaviorTreeEditorSettings.Instance.EditableFieldsWidth = Mathf.Clamp(EditorGUILayout.FloatField("Editable fields width: ", BehaviorTreeEditorSettings.Instance.EditableFieldsWidth, BehaviorTreeEditorSettings.Instance.EditableFieldsOption), 100, float.MaxValue);
        BehaviorTreeEditorSettings.Instance.LabelWidth = Mathf.Clamp(EditorGUILayout.FloatField("Labels width: ", BehaviorTreeEditorSettings.Instance.LabelWidth, BehaviorTreeEditorSettings.Instance.EditableFieldsOption), 200, float.MaxValue);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Nodes settings", _headerStyle, new GUILayoutOption[] { BehaviorTreeEditorSettings.Instance.LabelOption, GUILayout.Height(20) });
        BehaviorTreeEditorSettings.Instance.ElementHeight = Mathf.Clamp(EditorGUILayout.FloatField("Height: ", BehaviorTreeEditorSettings.Instance.ElementHeight, BehaviorTreeEditorSettings.Instance.EditableFieldsOption), 100, float.MaxValue);
        BehaviorTreeEditorSettings.Instance.ElementWidth = Mathf.Clamp(EditorGUILayout.FloatField("Width: ", BehaviorTreeEditorSettings.Instance.ElementWidth, BehaviorTreeEditorSettings.Instance.EditableFieldsOption), 200, float.MaxValue);
        BehaviorTreeEditorSettings.Instance.HorizontalSpaceBetweenElements = Mathf.Clamp(EditorGUILayout.FloatField("Horizontal space: ", BehaviorTreeEditorSettings.Instance.HorizontalSpaceBetweenElements, BehaviorTreeEditorSettings.Instance.EditableFieldsOption), 0, float.MaxValue);
        BehaviorTreeEditorSettings.Instance.VerticalSpaceBetweenElements = Mathf.Clamp(EditorGUILayout.FloatField("Vertical space: ", BehaviorTreeEditorSettings.Instance.VerticalSpaceBetweenElements, BehaviorTreeEditorSettings.Instance.EditableFieldsOption), 0, float.MaxValue);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Colors", _headerStyle, new GUILayoutOption[] { BehaviorTreeEditorSettings.Instance.LabelOption, GUILayout.Height(20) });
        BehaviorTreeEditorSettings.Instance.AddButtonBackgroundColor = EditorGUILayout.ColorField("Add child button: ", BehaviorTreeEditorSettings.Instance.AddButtonBackgroundColor, BehaviorTreeEditorSettings.Instance.EditableFieldsOption);
        BehaviorTreeEditorSettings.Instance.RemoveButtonBackgroundColor = EditorGUILayout.ColorField("Remove node button: ", BehaviorTreeEditorSettings.Instance.RemoveButtonBackgroundColor, BehaviorTreeEditorSettings.Instance.EditableFieldsOption);
        BehaviorTreeEditorSettings.Instance.SequenceBackgroundColor = EditorGUILayout.ColorField("Sequence header: ", BehaviorTreeEditorSettings.Instance.SequenceBackgroundColor, BehaviorTreeEditorSettings.Instance.EditableFieldsOption);
        BehaviorTreeEditorSettings.Instance.SelectorBackgroundColor = EditorGUILayout.ColorField("Selector header: ", BehaviorTreeEditorSettings.Instance.SelectorBackgroundColor, BehaviorTreeEditorSettings.Instance.EditableFieldsOption);
        BehaviorTreeEditorSettings.Instance.DecoratorBackgroundColor = EditorGUILayout.ColorField("Decorator header: ", BehaviorTreeEditorSettings.Instance.DecoratorBackgroundColor, BehaviorTreeEditorSettings.Instance.EditableFieldsOption);
        BehaviorTreeEditorSettings.Instance.TaskBackgroundColor = EditorGUILayout.ColorField("Task header: ", BehaviorTreeEditorSettings.Instance.TaskBackgroundColor, BehaviorTreeEditorSettings.Instance.EditableFieldsOption);
    }
}
