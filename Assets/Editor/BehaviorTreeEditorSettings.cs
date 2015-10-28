using UnityEngine;
using System.Collections;

public static class BehaviorTreeEditorSettings
{
    #region Temporary
    public static GUILayoutOption ButtonWidth = GUILayout.Width(150);
    public static GUILayoutOption decoratorTypePopupWidth = GUILayout.Width(200);
    public static GUILayoutOption methodChosePopupWidth = GUILayout.Width(300);
    public static GUILayoutOption labelWidth = GUILayout.Width(150);
    public static GUILayoutOption editableFieldsWidth = GUILayout.Width(400);
    public static GUILayoutOption floatFieldsWidth = GUILayout.Width(150);
    #endregion

    public static Color SequenceBackgroundColor;
    public static Color SelectorBackgroundColor;
    public static Color DecoratorBackgroundColor;
    public static Color TaskBackgroundColor;
    public static Color RemoveButtonBackgroundColor;
    public static Color AddButtonBackgroundColor;

    public static GUIStyle SequenceLabelStyle;
    public static GUIStyle SelectorLabelStyle;
    public static GUIStyle DecoratorLabelStyle;
    public static GUIStyle TaskLabelStyle;

    public static GUIStyle RemoveButtonStyle;
    public static GUIStyle AddButtonStyle;

    public static float ElementHeight;
    public static float ElementWidth;
    public static float HorizontalSpaceBetweenElements;
    public static float VerticalSpaceBetweenElements;

    public static void Init()
    {
        SequenceBackgroundColor = new Color(160.0f / 255.0f, 0.0f / 255.0f, 0.0f / 255.0f);
        SelectorBackgroundColor = new Color(0.0f / 255.0f, 0.0f / 255.0f, 160.0f / 255.0f);
        DecoratorBackgroundColor = new Color(0.0f / 255.0f, 160.0f / 255.0f, 00.0f / 255.0f);
        TaskBackgroundColor = new Color(0.0f / 255.0f, 160.0f / 255.0f, 40.0f / 255.0f);
        RemoveButtonBackgroundColor = new Color(160.0f / 255.0f, 0.0f / 255.0f, 0.0f / 255.0f);
        AddButtonBackgroundColor = new Color(0.0f / 255.0f, 255.0f / 255.0f, 0.0f / 255.0f);

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

        ElementHeight = 100.0f;
        ElementWidth = 160.0f;
        HorizontalSpaceBetweenElements = 90.0f;
        VerticalSpaceBetweenElements = 40.0f;
    }
}
