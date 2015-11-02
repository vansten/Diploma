using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class BehaviorTreeEditorSettings : ISerializable
{
    private string _prefix = "BTSettings/";
    private string _labelWidthPrefs = "LabelWidth";
    private string _buttonWidthPrefs = "ButtonWidth";
    private string _editableWidthPrefs = "EditableWidth";
    private string _elementHeightPrefs = "ElementHeight";
    private string _elementWidthPrefs = "ElementWidth";
    private string _horizontalSpacePrefs = "HorizontalSpaceBetweenElements";
    private string _verticalSpacePrefs = "VerticalSpaceBetweenElements";
    private string _sequenceColorRPrefs = "SequenceR";
    private string _sequenceColorBPrefs = "SequenceB";
    private string _sequenceColorGPrefs = "SequenceG";
    private string _selectorColorRPrefs = "SelectorR";
    private string _selectorColorBPrefs = "SelectorB";
    private string _selectorColorGPrefs = "SelectorG";
    private string _decoratorColorBPrefs = "DecoratorB";
    private string _decoratorColorRPrefs = "DecoratorR";
    private string _decoratorColorGPrefs = "DecoratorG";
    private string _taskColorRPrefs = "TaskR";
    private string _taskColorBPrefs = "TaskB";
    private string _taskColorGPrefs = "TaskG";
    private string _removeButtonColorRPrefs = "RemoveButtonR";
    private string _removeButtonColorBPrefs = "RemoveButtonB";
    private string _removeButtonColorGPrefs = "RemoveButtonG";
    private string _addButtonColorRPrefs = "AddButtonR";
    private string _addButtonColorBPrefs = "AddButtonB";
    private string _addButtonColorGPrefs = "AddButtonG";
    private string _sideMenuWidth = "SideMenuWidth";
    private string _sideMenuHeight = "SideMenuHeight";
    
    private string _path = "Assets/BehaviorTreeEditor/Settings.settings";

    public Color SequenceBackgroundColor;
    public Color SelectorBackgroundColor;
    public Color DecoratorBackgroundColor;
    public Color TaskBackgroundColor;
    public Color RemoveButtonBackgroundColor;
    public Color AddButtonBackgroundColor;

    public GUIStyle SequenceLabelStyle;
    public GUIStyle SelectorLabelStyle;
    public GUIStyle DecoratorLabelStyle;
    public GUIStyle TaskLabelStyle;

    public GUIStyle RemoveButtonStyle;
    public GUIStyle AddButtonStyle;

    public Rect SideMenuRect;

    public GUILayoutOption ButtonOption = GUILayout.Width(150);
    public GUILayoutOption LabelOption = GUILayout.Width(150);
    public GUILayoutOption EditableFieldsOption = GUILayout.Width(400);

    public float ElementHeight;
    public float ElementWidth;
    public float HorizontalSpaceBetweenElements;
    public float VerticalSpaceBetweenElements;

    public float ButtonWidth;
    public float LabelWidth;
    public float EditableFieldsWidth;

    private static BehaviorTreeEditorSettings _instance;
    public static BehaviorTreeEditorSettings Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new BehaviorTreeEditorSettings();
            }

            return _instance;
        }
    }

    public BehaviorTreeEditorSettings(SerializationInfo info, StreamingContext context)
    {
        LabelWidth = (float)info.GetDouble(_prefix + _labelWidthPrefs);
        EditableFieldsWidth = (float)info.GetDouble(_prefix + _editableWidthPrefs);
        ButtonWidth = (float)info.GetDouble(_prefix + _buttonWidthPrefs);

        ElementHeight = (float)info.GetDouble(_prefix + _elementHeightPrefs);
        ElementWidth = (float)info.GetDouble(_prefix + _elementWidthPrefs);
        HorizontalSpaceBetweenElements = (float)info.GetDouble(_prefix + _horizontalSpacePrefs);
        VerticalSpaceBetweenElements = (float)info.GetDouble(_prefix + _verticalSpacePrefs);

        SequenceBackgroundColor = new Color();
        SequenceBackgroundColor.r = (float)info.GetDouble(_prefix + _sequenceColorRPrefs);
        SequenceBackgroundColor.b = (float)info.GetDouble(_prefix + _sequenceColorBPrefs);
        SequenceBackgroundColor.g = (float)info.GetDouble(_prefix + _sequenceColorGPrefs);
        SequenceBackgroundColor.a = 1.0f;

        SelectorBackgroundColor = new Color();
        SelectorBackgroundColor.r = (float)info.GetDouble(_prefix + _selectorColorRPrefs);
        SelectorBackgroundColor.b = (float)info.GetDouble(_prefix + _selectorColorBPrefs);
        SelectorBackgroundColor.g = (float)info.GetDouble(_prefix + _selectorColorGPrefs);
        SelectorBackgroundColor.a = 1.0f;

        DecoratorBackgroundColor = new Color();
        DecoratorBackgroundColor.r = (float)info.GetDouble(_prefix + _decoratorColorRPrefs);
        DecoratorBackgroundColor.b = (float)info.GetDouble(_prefix + _decoratorColorBPrefs);
        DecoratorBackgroundColor.g = (float)info.GetDouble(_prefix + _decoratorColorGPrefs);
        DecoratorBackgroundColor.a = 1.0f;

        TaskBackgroundColor = new Color();
        TaskBackgroundColor.r = (float)info.GetDouble(_prefix + _taskColorRPrefs);
        TaskBackgroundColor.b = (float)info.GetDouble(_prefix + _taskColorBPrefs);
        TaskBackgroundColor.g = (float)info.GetDouble(_prefix + _taskColorGPrefs);
        TaskBackgroundColor.a = 1.0f;

        AddButtonBackgroundColor = new Color();
        AddButtonBackgroundColor.r = (float)info.GetDouble(_prefix + _addButtonColorRPrefs);
        AddButtonBackgroundColor.b = (float)info.GetDouble(_prefix + _addButtonColorBPrefs);
        AddButtonBackgroundColor.g = (float)info.GetDouble(_prefix + _addButtonColorGPrefs);
        AddButtonBackgroundColor.a = 1.0f;

        RemoveButtonBackgroundColor = new Color();
        RemoveButtonBackgroundColor.r = (float)info.GetDouble(_prefix + _removeButtonColorRPrefs);
        RemoveButtonBackgroundColor.b = (float)info.GetDouble(_prefix + _removeButtonColorBPrefs);
        RemoveButtonBackgroundColor.g = (float)info.GetDouble(_prefix + _removeButtonColorGPrefs);
        RemoveButtonBackgroundColor.a = 1.0f;

        SideMenuRect = new Rect(0, 0, 0, 0);
        SideMenuRect.width = (float)info.GetDouble(_prefix + _sideMenuWidth);
        SideMenuRect.height = (float)info.GetDouble(_prefix + _sideMenuHeight);

        Init();
    }

    public BehaviorTreeEditorSettings()
    {
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {

        info.AddValue(_prefix + _labelWidthPrefs, LabelWidth, typeof(float));
        info.AddValue(_prefix + _editableWidthPrefs, EditableFieldsWidth, typeof(float));
        info.AddValue(_prefix + _buttonWidthPrefs, ButtonWidth, typeof(float));
        info.AddValue(_prefix + _elementHeightPrefs, ElementHeight, typeof(float));
        info.AddValue(_prefix + _elementWidthPrefs, ElementWidth, typeof(float));
        info.AddValue(_prefix + _horizontalSpacePrefs, HorizontalSpaceBetweenElements, typeof(float));
        info.AddValue(_prefix + _verticalSpacePrefs, VerticalSpaceBetweenElements, typeof(float));
        info.AddValue(_prefix + _sequenceColorRPrefs, SequenceBackgroundColor.r, typeof(float));
        info.AddValue(_prefix + _sequenceColorBPrefs, SequenceBackgroundColor.b, typeof(float));
        info.AddValue(_prefix + _sequenceColorGPrefs, SequenceBackgroundColor.g, typeof(float));
        info.AddValue(_prefix + _selectorColorRPrefs, SelectorBackgroundColor.r, typeof(float));
        info.AddValue(_prefix + _selectorColorBPrefs, SelectorBackgroundColor.b, typeof(float));
        info.AddValue(_prefix + _selectorColorGPrefs, SelectorBackgroundColor.g, typeof(float));
        info.AddValue(_prefix + _decoratorColorRPrefs, DecoratorBackgroundColor.r, typeof(float));
        info.AddValue(_prefix + _decoratorColorBPrefs, DecoratorBackgroundColor.b, typeof(float));
        info.AddValue(_prefix + _decoratorColorGPrefs, DecoratorBackgroundColor.g, typeof(float));
        info.AddValue(_prefix + _taskColorRPrefs, TaskBackgroundColor.r, typeof(float));
        info.AddValue(_prefix + _taskColorBPrefs, TaskBackgroundColor.b, typeof(float));
        info.AddValue(_prefix + _taskColorGPrefs, TaskBackgroundColor.g, typeof(float));
        info.AddValue(_prefix + _addButtonColorRPrefs, AddButtonBackgroundColor.r, typeof(float));
        info.AddValue(_prefix + _addButtonColorBPrefs, AddButtonBackgroundColor.b, typeof(float));
        info.AddValue(_prefix + _addButtonColorGPrefs, AddButtonBackgroundColor.g, typeof(float));
        info.AddValue(_prefix + _removeButtonColorRPrefs, RemoveButtonBackgroundColor.r, typeof(float));
        info.AddValue(_prefix + _removeButtonColorBPrefs, RemoveButtonBackgroundColor.b, typeof(float));
        info.AddValue(_prefix + _removeButtonColorGPrefs, RemoveButtonBackgroundColor.g, typeof(float));
        info.AddValue(_prefix + _sideMenuWidth, SideMenuRect.width, typeof(float));
        info.AddValue(_prefix + _sideMenuHeight, SideMenuRect.height, typeof(float));
    }

    private void Init()
    {
        Color[] color = new Color[1];

        SequenceLabelStyle = new GUIStyle();
        SequenceLabelStyle.normal.background = new Texture2D(1, 1);
        color[0] = SequenceBackgroundColor;
        SequenceLabelStyle.normal.background.SetPixels(color);
        SequenceLabelStyle.fontStyle = FontStyle.Bold;
        SequenceLabelStyle.alignment = TextAnchor.MiddleCenter;
        SequenceLabelStyle.normal.textColor = Color.white;

        SelectorLabelStyle = new GUIStyle();
        SelectorLabelStyle.normal.background = new Texture2D(1, 1);
        color[0] = SelectorBackgroundColor;
        SelectorLabelStyle.normal.background.SetPixels(color);
        SelectorLabelStyle.fontStyle = FontStyle.Bold;
        SelectorLabelStyle.alignment = TextAnchor.MiddleCenter;
        SelectorLabelStyle.normal.textColor = Color.white;

        DecoratorLabelStyle = new GUIStyle();
        DecoratorLabelStyle.normal.background = new Texture2D(1, 1);
        color[0] = DecoratorBackgroundColor;
        DecoratorLabelStyle.normal.background.SetPixels(color);
        DecoratorLabelStyle.fontStyle = FontStyle.Bold;
        DecoratorLabelStyle.alignment = TextAnchor.MiddleCenter;
        DecoratorLabelStyle.normal.textColor = Color.white;

        TaskLabelStyle = new GUIStyle();
        TaskLabelStyle.normal.background = new Texture2D(1, 1);
        color[0] = TaskBackgroundColor;
        TaskLabelStyle.normal.background.SetPixels(color);
        TaskLabelStyle.fontStyle = FontStyle.Bold;
        TaskLabelStyle.alignment = TextAnchor.MiddleCenter;
        TaskLabelStyle.normal.textColor = Color.white;

        RemoveButtonStyle = new GUIStyle();
        RemoveButtonStyle.normal.background = new Texture2D(1, 1);
        color[0] = RemoveButtonBackgroundColor;
        RemoveButtonStyle.normal.background.SetPixels(color);
        RemoveButtonStyle.normal.textColor = Color.white;
        RemoveButtonStyle.fontStyle = FontStyle.Bold;
        RemoveButtonStyle.alignment = TextAnchor.MiddleCenter;

        AddButtonStyle = new GUIStyle();
        AddButtonStyle.normal.background = new Texture2D(1, 1);
        color[0] = AddButtonBackgroundColor;
        AddButtonStyle.normal.background.SetPixels(color);
        AddButtonStyle.normal.textColor = Color.white;
        AddButtonStyle.fontStyle = FontStyle.Bold;
        AddButtonStyle.alignment = TextAnchor.MiddleCenter;

        LabelOption = GUILayout.Width(LabelWidth);
        ButtonOption = GUILayout.Width(ButtonWidth);
        EditableFieldsOption = GUILayout.Width(EditableFieldsWidth);
    }

    public void Save()
    {
        if(!Directory.Exists("Assets"))
        {
            Directory.CreateDirectory("Assets");
        }

        if (!Directory.Exists("Assets/BehaviorTreeEditor"))
        {
            Directory.CreateDirectory("Assets/BehaviorTreeEditor");
        }

        FileStream fileStream = new FileStream(_path, FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fileStream, Instance);
        fileStream.Dispose();

        Init();
    }

    public void Load()
    {
        if(!File.Exists(_path))
        {
            Debug.LogError(string.Format("{0} - file doesn't exists", _path));
            return;
        }

        FileStream fileStream = new FileStream(_path, FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        _instance = (BehaviorTreeEditorSettings)bf.Deserialize(fileStream);
        fileStream.Dispose();
    }
}
