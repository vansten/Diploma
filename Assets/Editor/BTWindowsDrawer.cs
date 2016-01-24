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
    private List<string> _methods = new List<string>();
    private Queue<DrawerNodeClass> _drawingQueue = new Queue<DrawerNodeClass>();
    private string[] _addChildOptions = {"+", "Sequence", "Selector", "Decorator", "Task" };

    private Rect _infoRect = new Rect(20, 100, 200, 50);
    private Rect _addRootRect = new Rect(20, 180, 250, 15);
    private Vector2 _scrollPos = Vector2.zero;

    public void SetDecoratorsList(List<string> decorators)
    {
        _decorators = decorators;
    }

    public void SetMethodsList(List<string> methods)
    {
        _methods = methods;
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
            BehaviorTreeEditor.BTEditorWindow.Repaint();
        }

        _behaviorTree = behaviorTree;

        if (_behaviorTree.Child == null)
        {
            int i = EditorGUI.Popup(_addRootRect, "Add root: ", 0, _addChildOptions, BehaviorTreeEditorSettings.Instance.AddButtonStyle);
            switch (i)
            {
                case 1:
                    Sequence newSequence = new Sequence();
                    behaviorTree.AddChild(newSequence);
                    break;
                case 2:
                    Selector newSelector = new Selector();
                    behaviorTree.AddChild(newSelector);
                    break;
                case 3:
                    Decorator newDecorator = new Decorator();
                    behaviorTree.AddChild(newDecorator);
                    break;
                case 4:
                    Task t = new Task();
                    behaviorTree.AddChild(t);
                    break;
                default:
                    break;
            }
            
            BehaviorTreeEditorHelper.GenerateQueue(_drawingQueue, _behaviorTree);
        }

        _scrollPos = GUI.BeginScrollView(new Rect(0, 100, BehaviorTreeEditor.BTEditorWindow.position.width - BehaviorTreeEditorSettings.Instance.SideMenuRect.width - 10, BehaviorTreeEditor.BTEditorWindow.position.height - _addRootRect.height - 100),
                                        _scrollPos,
                                        new Rect(0, 100, BehaviorTreeEditorHelper.GetWidth() * (BehaviorTreeEditorSettings.Instance.ElementWidth + BehaviorTreeEditorSettings.Instance.HorizontalSpaceBetweenElements) + 110 + _infoRect.x,
                                                        BehaviorTreeEditorHelper.GetDepth(_drawingQueue) * (BehaviorTreeEditorSettings.Instance.ElementHeight + BehaviorTreeEditorSettings.Instance.VerticalSpaceBetweenElements) + 100));

        BehaviorTreeEditor.BTEditorWindow.BeginWindows();

        GUILayout.Window(-1, _infoRect, DrawInfo, "Behavior Tree Info");
        DrawStack();

        BehaviorTreeEditor.BTEditorWindow.EndWindows();
        GUI.EndScrollView();
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
                Vector3 startPos = new Vector3(dnc.Parent.Position.x + dnc.Parent.Position.width / 2, dnc.Parent.Position.y + dnc.Parent.Position.height);
                Vector3 endPos = new Vector3(dnc.Position.x + dnc.Position.width / 2, dnc.Position.y);
                float mnog = Vector3.Distance(startPos, endPos) * 0.2f;
                Vector3 startTangent = startPos + Vector3.right * (mnog);
                Vector3 endTangent = endPos + Vector3.left * (mnog);
                Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Color.black, null, 3.0f);
                Handles.EndGUI();
            }
            dnc.Position = GUILayout.Window(dnc.Index, dnc.Position, DrawNode, "");
        }
    }

    private void DrawNode(int id)
    {
        //GUI.DragWindow();
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
    }

    private void DrawRemoveChildOption(INode node)
    {
        if (GUILayout.Button("X", BehaviorTreeEditorSettings.Instance.RemoveButtonStyle))
        {
            if (node.IsRoot())
            {
                _behaviorTree.RemoveChild();
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
        int i = EditorGUILayout.Popup(0, _addChildOptions, BehaviorTreeEditorSettings.Instance.AddButtonStyle);
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
        EditorGUILayout.LabelField("Sequence", BehaviorTreeEditorSettings.Instance.SequenceLabelStyle);
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
        EditorGUILayout.LabelField("Selector", BehaviorTreeEditorSettings.Instance.SelectorLabelStyle);
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
        EditorGUILayout.LabelField("Decorator", BehaviorTreeEditorSettings.Instance.DecoratorLabelStyle);
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
        EditorGUILayout.LabelField("Task", BehaviorTreeEditorSettings.Instance.TaskLabelStyle);
        DrawRemoveChildOption(task);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        string currentMethod = task.MethodType + "." + task.MethodName;
        int selected = _methods.IndexOf(currentMethod);
        int newSelected = selected;
        newSelected = EditorGUILayout.Popup(selected, _methods.ToArray());
        if (newSelected != selected)
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
