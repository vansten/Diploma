using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

public class BTWindowsDrawer : IBTElementDrawer
{
    private BehaviorTree _behaviorTree;

    private List<string> _decorators;
    private List<string> _methods;
    private Queue<DrawerNodeClass> _drawingQueue = new Queue<DrawerNodeClass>();
    private string[] _addChildOptions = {"+", "Sequence", "Selector", "Decorator", "Task" };

    private Rect _infoRect = new Rect(20, 100, 200, 50);
    private Vector2 _scrollPos = Vector2.zero;

    public void SetDecoratorsList(List<string> decorators)
    {
        _decorators = decorators;
    }

    public void SetMethodsList(List<string> methodsList)
    {
        _methods = methodsList;
    }

    public void DrawBehaviorTree(BehaviorTree behaviorTree)
    {
        if(behaviorTree == null || BehaviorTreeEditor.BTEditorWindow == null)
        {
            return;
        }

        if(behaviorTree != _behaviorTree)
        {
            BehaviorTreeEditorHelper.GenerateQueue(_drawingQueue, behaviorTree);
        }

        _behaviorTree = behaviorTree;

        EditorGUILayout.BeginVertical();
        _scrollPos = GUI.BeginScrollView(new Rect(0, 100, BehaviorTreeEditor.BTEditorWindow.position.width, BehaviorTreeEditor.BTEditorWindow.position.height - 100),
                                        _scrollPos,
                                        new Rect(0, 100, BehaviorTreeEditorHelper.GetWidth() * (BehaviorTreeEditorSettings.ElementWidth + BehaviorTreeEditorSettings.HorizontalSpaceBetweenElements) + 110,
                                                        BehaviorTreeEditorHelper.GetDepth(_drawingQueue) * (BehaviorTreeEditorSettings.ElementHeight + BehaviorTreeEditorSettings.VerticalSpaceBetweenElements) + 100));
        BehaviorTreeEditor.BTEditorWindow.BeginWindows();

        GUILayout.Window(-1, _infoRect, DrawInfo, "Behavior Tree Info");
        DrawStack();

        BehaviorTreeEditor.BTEditorWindow.EndWindows();
        GUI.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawInfo(int id)
    {
        float newTickDelay = Mathf.Clamp(EditorGUILayout.FloatField("Tick delay: ", _behaviorTree.TickDelay), 0.0f, float.PositiveInfinity);
        if (newTickDelay != _behaviorTree.TickDelay)
        {
            _behaviorTree.TickDelay = newTickDelay;
            if (BehaviorTreeEditor.BTEditorWindow.Autosave)
            {
                BehaviorTreeEditor.BTEditorWindow.Save();
            }
        }
    }
    
    private void DrawStack()
    {
        if(_drawingQueue.Count == 0)
        {
            //Draw add child option only
            return;
        }

        foreach(DrawerNodeClass dnc in _drawingQueue)
        {
            if(dnc.Parent != null)
            {
                Handles.BeginGUI();
                Handles.color = Color.black;
                Handles.DrawLine(
                                 new Vector3(dnc.Parent.Position.x + dnc.Parent.Position.width / 2, dnc.Parent.Position.y + dnc.Parent.Position.height),
                                 new Vector3(dnc.Position.x + dnc.Position.width / 2, dnc.Position.y)
                                 );
                Handles.EndGUI();
            }
            dnc.Position = GUILayout.Window(dnc.Index, dnc.Position, DrawNode, "");
        }
    }

    private void DrawNode(int id)
    {
        DrawerNodeClass dnc = BehaviorTreeEditorHelper.GetNodeById(_drawingQueue, id);
        if(dnc == null)
        {
            return;
        }

        Type t = dnc.Node.GetType();
        if (t == typeof(Selector))
        {
            DrawSelector((Selector)dnc.Node);
        }
        else if (t == typeof(Sequence))
        {
            DrawSequence((Sequence)dnc.Node);
        }
        else if (t == typeof(Decorator))
        {
            DrawDecorator((Decorator)dnc.Node);
        }
        else
        {
            DrawTask((Task)dnc.Node);
        }
        
        GUI.DragWindow();
    }

    private void DrawRemoveChildOption(INode node)
    {
        if (GUILayout.Button("X", BehaviorTreeEditorSettings.RemoveButtonStyle))
        {
            if (node.IsRoot())
            {
                _behaviorTree.Child = null;
            }
            else
            {
                node.GetParent().RemoveChild(node);
            }
            BehaviorTreeEditorHelper.GenerateQueue(_drawingQueue, _behaviorTree);
        }
    }

    private void DrawAddChildOption(INode node)
    {
        int i = EditorGUILayout.Popup(0, _addChildOptions, BehaviorTreeEditorSettings.AddButtonStyle);
        if(i == 0)
        {
            return;
        }
        switch(i)
        {
            case 1:
                Sequence newSequence = new Sequence();
                node.AddChild(newSequence);
                newSequence.SetParent(node);
                break;
            case 2:
                Selector newSelector = new Selector();
                node.AddChild(newSelector);
                newSelector.SetParent(node);
                break;
            case 3:
                Decorator newDecorator = new Decorator();
                node.AddChild(newDecorator);
                newDecorator.SetParent(node);
                break;
            case 4:
                Task t = new Task();
                node.AddChild(t);
                t.SetParent(node);
                break;
            default:
                break;
        }

        BehaviorTreeEditorHelper.GenerateQueue(_drawingQueue, _behaviorTree);
    }

    private void DrawSequence(Sequence sequence)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Sequence", BehaviorTreeEditorSettings.SequenceLabelStyle);
        DrawRemoveChildOption(sequence);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        DrawAddChildOption(sequence);
        EditorGUILayout.EndVertical();
    }

    private void DrawSelector(Selector selector)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Selector", BehaviorTreeEditorSettings.SelectorLabelStyle);
        DrawRemoveChildOption(selector);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        DrawAddChildOption(selector);
        EditorGUILayout.EndVertical();
    }

    private void DrawDecorator(Decorator decorator)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Decorator", BehaviorTreeEditorSettings.DecoratorLabelStyle);
        DrawRemoveChildOption(decorator);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if(decorator.Child == null)
        {
            DrawAddChildOption(decorator);
        }
        else
        {
            int selected = _decorators.IndexOf(decorator.Type.ToString());
            int newSelected = EditorGUILayout.Popup(selected, _decorators.ToArray());
            if (newSelected != selected && newSelected >= 0 && newSelected < _decorators.Count)
            {
                DecoratorType newDT = (DecoratorType)Enum.Parse(typeof(DecoratorType), _decorators[newSelected]);
                decorator.Type = newDT;
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawTask(Task task)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Task", BehaviorTreeEditorSettings.TaskLabelStyle);
        DrawRemoveChildOption(task);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        string currentMethod = task.MethodType + "." + task.MethodName;
        int selected = _methods.IndexOf(currentMethod);
        int newSelected = selected;
        newSelected = EditorGUILayout.Popup(selected, _methods.ToArray());
        if(newSelected != selected)
        {
            string s = _methods[newSelected];
            string type = s.Substring(0, s.LastIndexOf('.'));
            string method = s.Substring(s.LastIndexOf('.') + 1);
            Assembly ass = Assembly.GetAssembly(typeof(BehaviorTree));
            Type[] types = ass.GetTypes();
            Type t = null;
            foreach (Type tp in types)
            {
                if (tp.Name == type)
                {
                    t = tp;
                    break;
                }
            }

            if (t != null)
            {
                task.SetMethod(t, method);
            }
        }
        EditorGUILayout.EndVertical();
    }
}