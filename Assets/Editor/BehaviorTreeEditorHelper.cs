using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public enum EditorState
{
    DrawSettings,
    DrawBehaviorTree
}

public class DrawerNodeClass
{
    public INode Node;
    public DrawerNodeClass Parent;
    public int Index;
    public int Depth;
    public int Width;
    public Rect Position;

    public void CalculateRect()
    {
        Position = new Rect(
                            Width * (BehaviorTreeEditorSettings.Instance.ElementWidth + BehaviorTreeEditorSettings.Instance.HorizontalSpaceBetweenElements) + 110,
                            Depth * (BehaviorTreeEditorSettings.Instance.ElementHeight + BehaviorTreeEditorSettings.Instance.VerticalSpaceBetweenElements) + 200,
                            BehaviorTreeEditorSettings.Instance.ElementWidth,
                            BehaviorTreeEditorSettings.Instance.ElementHeight);
    }
}

public static class BehaviorTreeEditorHelper
{
    private static int[] _widths;

    public static void GenerateQueue(Queue<DrawerNodeClass> drawingQueue, BehaviorTree bt)
    {
        Queue<INode> bfsQueue = new Queue<INode>();
        bfsQueue.Enqueue(bt.Child);
        int i = 0;
        drawingQueue.Clear();
        while (bfsQueue.Count > 0)
        {
            INode n = bfsQueue.Dequeue();
            if (n != null)
            {
                DrawerNodeClass newElement = new DrawerNodeClass();
                newElement.Index = i;
                newElement.Node = n;
                newElement.Depth = CountDepth(n);
                newElement.Parent = GetParent(drawingQueue, n.GetParent(), newElement.Depth - 1);
                drawingQueue.Enqueue(newElement);
                i++;

                Type t = n.GetType();
                if (t == typeof(Decorator))
                {
                    FillQueueWithDecorator(bfsQueue, (Decorator)n);
                }
                else if (t == typeof(Sequence))
                {
                    FillQueueWithSequence(bfsQueue, (Sequence)n);
                }
                else if (t == typeof(Selector))
                {
                    FillQueueWithSelector(bfsQueue, (Selector)n);
                }
            }
        }

        int maxDepth = 0;

        foreach (DrawerNodeClass dnc in drawingQueue)
        {
            if (dnc.Depth > maxDepth)
            {
                maxDepth = dnc.Depth;
            }
        }

        maxDepth += 1;
        _widths = new int[maxDepth];

        foreach (DrawerNodeClass dnc in drawingQueue)
        {
            dnc.Width = _widths[dnc.Depth];
            if(dnc.Parent != null && dnc.Width < dnc.Parent.Width)
            {
                dnc.Width = Mathf.Max(dnc.Width, dnc.Parent.Width);
                _widths[dnc.Depth] = dnc.Parent.Width;
            }
            _widths[dnc.Depth] += 1;
            dnc.CalculateRect();
        }

        BehaviorTreeEditor.BTEditorWindow.Repaint();
    }

    private static void FillQueueWithSelector(Queue<INode> queue, Selector s)
    {
        List<INode> children = s.Children;
        if (children != null)
        {
            foreach (INode c in children)
            {
                if (c != null)
                {
                    queue.Enqueue(c);
                }
            }
        }
    }

    private static void FillQueueWithSequence(Queue<INode> queue, Sequence s)
    {
        List<INode> children = s.Children;
        if (children != null)
        {
            foreach (INode c in children)
            {
                if (c != null)
                {
                    queue.Enqueue(c);
                }
            }
        }
    }

    private static void FillQueueWithDecorator(Queue<INode> queue, Decorator d)
    {
        INode child = d.Child;
        if (child != null)
        {
            queue.Enqueue(child);
        }
    }

    private static int CountDepth(INode n)
    {
        int depth = 0;
        INode tmp = n.GetParent();
        while (tmp != null)
        {
            tmp = tmp.GetParent();
            ++depth;
        }
        return depth;
    }

    public static int GetDepth(Queue<DrawerNodeClass> drawingQueue)
    {
        int maxDepth = 0;

        foreach (DrawerNodeClass dnc in drawingQueue)
        {
            if (dnc.Depth > maxDepth)
            {
                maxDepth = dnc.Depth;
            }
        }

        return maxDepth + 1;
    }

    public static int GetWidth()
    {
        int maxWidth = 0;
        foreach(int i in _widths)
        {
            if(maxWidth < i)
            {
                maxWidth = i;
            }
        }

        return maxWidth;
    }

    private static DrawerNodeClass GetParent(Queue<DrawerNodeClass> drawingQueue, INode node, int depth)
    {
        if (node == null)
        {
            return null;
        }

        foreach (DrawerNodeClass dnc in drawingQueue)
        {
            if (dnc.Depth == depth && dnc.Node == node)
            {
                return dnc;
            }
        }

        return null;
    }

    public static DrawerNodeClass GetNodeById(Queue<DrawerNodeClass> drawingQueue, int id)
    {
        if (id < 0)
        {
            return null;
        }

        foreach (DrawerNodeClass dnc in drawingQueue)
        {
            if (dnc.Index == id)
            {
                return dnc;
            }
        }

        return null;
    }

    public static void FillMethodsList(List<string> methodsList, GameObject go)
    {
        methodsList.Clear();
        MonoBehaviour[] components = go.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour mb in components)
        {
            Type t = mb.GetType();
            MethodInfo[] mis = t.GetMethods();
            foreach (MethodInfo mi in mis)
            {
                if (mi.ReturnType == typeof(TaskStatus))
                {
                    ParameterInfo[] pis = mi.GetParameters();
                    if (pis.Length == 2)
                    {
                        if (pis[0].ParameterType == typeof(GameObject) && pis[1].ParameterType == typeof(Blackboard))
                        {
                            string type = t.ToString();
                            string methodName = mi.Name;
                            string newMethod = type + "." + methodName;
                            methodsList.Add(newMethod);
                        }
                    }
                }
            }
        }
    }
}
