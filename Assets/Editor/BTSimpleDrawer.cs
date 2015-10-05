using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;

public class BTSimpleDrawer : IBTElementDrawer
{
    const string emptySpace = "     ";
    const string stringDrawerHelper = "|---";

    private List<string> _decorators;
    private List<string> _methods;
    private string[] _addChildOptions = { "+", "Sequence", "Selector", "Decorator", "Task" };

    private int _drawI = 0;
    private BehaviorTree _behaviorTree;

    public void DrawBehaviorTree(BehaviorTree behaviorTree)
    {
        _behaviorTree = behaviorTree;

        _drawI = 0;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tick Delay (in seconds): ", BehaviorTreeEditorSettings.labelWidth);
        _behaviorTree.TickDelay = Mathf.Clamp(EditorGUILayout.FloatField(_behaviorTree.TickDelay, BehaviorTreeEditorSettings.floatFieldsWidth), 0.0f, float.MaxValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Behavior Tree", BehaviorTreeEditorSettings.labelWidth);
        int newChild = DrawAddChildOption();
        if (newChild != 0)
        {
            switch (newChild)
            {
                case 1:
                    _behaviorTree.Child = new Sequence();
                    break;
                case 2:
                    _behaviorTree.Child = new Selector();
                    break;
                case 3:
                    _behaviorTree.Child = new Decorator();
                    break;
                case 4:
                    _behaviorTree.Child = new Task();
                    break;
                default:
                    break;
            }
        }
        if (_behaviorTree.Child != null)
        {
            _behaviorTree.Child.MakeRoot(_behaviorTree);
        }
        EditorGUILayout.EndHorizontal();
        ++_drawI;
        DrawNode(_behaviorTree.Child);
        --_drawI;
    }

    public void SetDecoratorsList(List<string> decorators)
    {
        _decorators = decorators;
    }

    public void SetMethodsList(List<string> methodsList)
    {
        _methods = methodsList;
    }

    private void DrawNode(INode currentNode)
    {
        if (currentNode == null)
        {
            return;
        }
        Type t = currentNode.GetType();
        if (t == typeof(Selector))
        {
            DrawSelector((Selector)currentNode);
            foreach (INode child in ((Selector)currentNode).Children.ToArray())
            {
                ++_drawI;
                DrawNode(child);
                --_drawI;
            }
        }
        else if (t == typeof(Sequence))
        {
            DrawSequence((Sequence)currentNode);
            foreach (INode child in ((Sequence)currentNode).Children.ToArray())
            {
                ++_drawI;
                DrawNode(child);
                --_drawI;
            }
        }
        else if (t == typeof(Decorator))
        {
            DrawDecorator((Decorator)currentNode);
            ++_drawI;
            DrawNode(((Decorator)currentNode).Child);
            --_drawI;
        }
        else
        {
            DrawTask((Task)currentNode);
        }
    }

    private void DrawSelector(Selector selector)
    {
        string toDraw = "";
        for (int i = 0; i < _drawI - 1; ++i)
        {
            toDraw += emptySpace;
        }
        toDraw += stringDrawerHelper;
        toDraw += "Selector";
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(toDraw, BehaviorTreeEditorSettings.labelWidth);
        AddChildToNode(selector);
        DrawRemoveChildOption(selector);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSequence(Sequence sequence)
    {
        string toDraw = "";
        for (int i = 0; i < _drawI - 1; ++i)
        {
            toDraw += emptySpace;
        }
        toDraw += stringDrawerHelper;
        toDraw += "Sequence";
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(toDraw, BehaviorTreeEditorSettings.labelWidth);
        AddChildToNode(sequence);
        DrawRemoveChildOption(sequence);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawDecorator(Decorator decorator)
    {
        string toDraw = "";
        for (int i = 0; i < _drawI - 1; ++i)
        {
            toDraw += emptySpace;
        }
        toDraw += stringDrawerHelper;
        toDraw += "Dcorator";
        toDraw += ", Type: ";
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(toDraw, BehaviorTreeEditorSettings.labelWidth);
        int selected = _decorators.IndexOf(decorator.Type.ToString());
        int nSelected = EditorGUILayout.Popup(selected, _decorators.ToArray(), BehaviorTreeEditorSettings.decoratorTypePopupWidth);
        if (nSelected != selected && nSelected >= 0 && nSelected < _decorators.Count)
        {
            DecoratorType newDT = (DecoratorType)Enum.Parse(typeof(DecoratorType), _decorators[nSelected]);
            decorator.Type = newDT;
        }
        AddChildToNode(decorator);
        DrawRemoveChildOption(decorator);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTask(Task task)
    {
        string toDraw = "";
        for (int i = 0; i < _drawI - 1; ++i)
        {
            toDraw += emptySpace;
        }
        toDraw += stringDrawerHelper;
        toDraw += "Task, Method: ";
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(toDraw, BehaviorTreeEditorSettings.labelWidth);
        string currentMethod = task.MethodType + "." + task.MethodName;
        int selected = _methods.IndexOf(currentMethod);
        int nSelected = selected;
        nSelected = EditorGUILayout.Popup(selected, _methods.ToArray(), BehaviorTreeEditorSettings.methodChosePopupWidth);
        DrawRemoveChildOption(task);
        EditorGUILayout.EndHorizontal();
        if (nSelected != selected)
        {
            string s = _methods[nSelected];
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
    }

    private int DrawAddChildOption()
    {
        int selected = 0;
        int nSelected = EditorGUILayout.Popup(selected, _addChildOptions, BehaviorTreeEditorSettings.labelWidth);
        return nSelected;
    }

    private void AddChildToNode(INode node)
    {
        int option = DrawAddChildOption();
        if (option != 0)
        {
            switch (option)
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
        }
    }

    private void DrawRemoveChildOption(INode node)
    {
        if (GUILayout.Button("-"))
        {
            node.Remove();
        }
    }
}