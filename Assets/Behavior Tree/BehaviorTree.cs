using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public enum TaskStatus
{
    FAILURE,
    RUNNING,
    SUCCESS
}

public enum DecoratorType
{
    ALWAYS_FAIL,
    ALWAYS_SUCC,
    REVERT_RESULT
}

public interface INode : IXmlSerializable
{
    void Initialize(Blackboard blackboard, GameObject owner, bool created);
    void AddChild(INode child);
    INode GetParent();
    void RemoveChild(INode child = null);
    void SetParent(INode parent);
    void Remove();
    void MakeRoot(BehaviorTree bt);
    bool IsRoot();
    TaskStatus Tick(out INode nodeRunning, GameObject owner);
}

public class Helper
{
    public static void LogAndBreak(string msg)
    {
        Debug.LogError(msg);
        Debug.Break();
    }
}

public class BTSerializer
{
    public static BehaviorTree Deserialize(TextAsset btAsset)
    {
        BehaviorTree bt = null;
        StringReader reader = new StringReader(btAsset.text);
        XmlReader xmlReader = XmlReader.Create(reader);
        bt = new BehaviorTree();
        try
        {
            bt.ReadXml(xmlReader);
            return bt;
        }
        catch(XmlException xmle)
        {
            Debug.LogWarning(xmle.Message);
            return null;
        }
    }

    public static bool Serialize(BehaviorTree bt, string filePath)
    {
        Queue<INode> queue = new Queue<INode>();
        if(!TryGenerateQueue(queue, bt))
        {
            Debug.LogWarning("Can't save behavior tree if some selector or sequence has no children");
            return false;
        }

        StreamWriter writer = new StreamWriter(filePath);
        XmlSerializer serializer = new XmlSerializer(bt.GetType());
        serializer.Serialize(writer, bt);
        writer.Flush();
        writer.Close();

        return true;
    }

    private static bool TryGenerateQueue(Queue<INode> bfsQueue, BehaviorTree bt)
    {
        bfsQueue.Clear();
        bfsQueue.Enqueue(bt.Child);

        while (bfsQueue.Count > 0)
        {
            INode n = bfsQueue.Dequeue();
            if (n != null)
            {
                Type t = n.GetType();
                if (t == typeof(Decorator))
                {
                    FillQueueWithDecorator(bfsQueue, (Decorator)n);
                }
                else if (t == typeof(Sequence))
                {
                    if (((Sequence)n).Children.Count == 0)
                    {
                        return false;
                    }
                    FillQueueWithSequence(bfsQueue, (Sequence)n);
                }
                else if (t == typeof(Selector))
                {
                    if (((Selector)n).Children.Count == 0)
                    {
                        return false;
                    }
                    FillQueueWithSelector(bfsQueue, (Selector)n);
                }
            }
        }

        return true;
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
}

public class Selector : INode
{
    private BehaviorTree _bt;
    private INode _parent;
    private List<INode> _children = new List<INode>();

    public List<INode> Children
    {
        get { return _children; }
        set { _children = value; }
    }

    public void Initialize(Blackboard blackboard, GameObject owner, bool created)
    {
        if(_children == null || _children.Count == 0)
        {
            if(!created)
            {
                Helper.LogAndBreak("Selector has no children");
            }
        }
        else
        {
            foreach(INode child in _children)
            {
                child.Initialize(blackboard, owner, created);
            }
        }
    }

    public TaskStatus Tick(out INode nodeRunning, GameObject owner)
    {
        nodeRunning = null;
        foreach(INode child in _children)
        {
            TaskStatus ts = child.Tick(out nodeRunning, owner);
            if (ts != TaskStatus.FAILURE)
            {
                return ts;
            }
        }

        return TaskStatus.FAILURE;
    }


    public void AddChild(INode child)
    {
        _children.Add(child);
    }

    public INode GetParent()
    {
        return _parent;
    }

    public void RemoveChild(INode child = null)
    {
        if (_children.Contains(child))
        {
            _children.Remove(child);
        }
    }

    public void SetParent(INode parent)
    {
        _parent = parent;
    }

    public void Remove()
    {
        if(_parent != null)
        {
            _parent.RemoveChild(this);
        }
        else
        {
            if(_bt != null)
            {
                _bt.RemoveChild();
            }
        }
    }

    public void MakeRoot(BehaviorTree bt)
    {
        _bt = bt;
    }

    public bool IsRoot()
    {
        return _bt != null;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        _children.Clear();
        reader.ReadStartElement("Selector");
        if (reader.Name == "Children")
        {
            reader.ReadStartElement();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                string s = reader.ReadElementString("Type", "");
                System.Object obj = Activator.CreateInstance(Type.GetType(s));
                (obj as IXmlSerializable).ReadXml(reader);
                AddChild((INode)obj);
                ((INode)obj).SetParent(this);
            }
            reader.ReadEndElement();
        }
        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("Selector");
        writer.WriteStartElement("Children");
        foreach (INode child in _children)
        {
            writer.WriteElementString("Type", child.GetType().ToString());
            (child as IXmlSerializable).WriteXml(writer);
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }
}

public class Sequence : INode
{
    private BehaviorTree _bt;
    private INode _parent;
    private List<INode> _children = new List<INode>();
    public List<INode> Children
    {
        get { return _children; }
        set { _children = value; }
    }

    public void Initialize(Blackboard blackboard, GameObject owner, bool created)
    {
        if (_children == null || _children.Count == 0)
        {
            if (!created)
            {
                Helper.LogAndBreak("Selector has no children");
            }
        }
        else
        {
            foreach (INode child in _children)
            {
                child.Initialize(blackboard, owner, created);
            }
        }
    }

    public TaskStatus Tick(out INode nodeRunning, GameObject owner)
    {
        nodeRunning = null;
        foreach(INode child in _children)
        {
            TaskStatus ts = child.Tick(out nodeRunning, owner);
            if(ts != TaskStatus.SUCCESS)
            {
                return ts;
            }
        }

        return TaskStatus.SUCCESS;
    }


    public void AddChild(INode child)
    {
        _children.Add(child);
    }

    public INode GetParent()
    {
        return _parent;
    }

    public void RemoveChild(INode child)
    {
        if (_children.Contains(child))
        {
            _children.Remove(child);
        }
    }

    public void SetParent(INode parent)
    {
        _parent = parent;
    }

    public void Remove()
    {
        if (_parent != null)
        {
            _parent.RemoveChild(this);
        }
        else
        {
            if (_bt != null)
            {
                _bt.RemoveChild();
            }
        }
    }

    public void MakeRoot(BehaviorTree bt)
    {
        _bt = bt;
    }

    public bool IsRoot()
    {
        return _bt != null;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        _children.Clear();
        reader.ReadStartElement("Sequence");
        if (reader.Name == "Children")
        {
            reader.ReadStartElement();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                string s = reader.ReadElementString("Type", "");
                System.Object obj = Activator.CreateInstance(Type.GetType(s));
                (obj as IXmlSerializable).ReadXml(reader);
                AddChild((INode)obj);
                ((INode)obj).SetParent(this);
            }
            reader.ReadEndElement();
    }
        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("Sequence");
        writer.WriteStartElement("Children");
        if (_children.Count > 0)
        {
            foreach (INode child in _children)
            {
                writer.WriteElementString("Type", child.GetType().ToString());
                (child as IXmlSerializable).WriteXml(writer);
            }
        }
        else
        {
            writer.WriteString("NULL");
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }
}

public class Task : INode
{
    private BehaviorTree _bt;
    private Blackboard _blackboard;
    private INode _parent;
    public delegate TaskStatus TickDelegate(GameObject owner, Blackboard blackboard);
    public event TickDelegate OnTaskTick;

    private Type _methodType;
    private string _methodName = String.Empty;
    private GameObject _owner;

    public string MethodType
    {
        get
        {
            if (_methodType == null)
            {
                return "";
            }
            return _methodType.ToString();
        }
    }

    public string MethodName
    {
        get
        {
            return _methodName;
        }
    }
    
    public void Initialize(Blackboard blackboard, GameObject owner, bool created)
    {
        _blackboard = blackboard;
        _owner = owner;
        SetMethod(_methodType, _methodName);
        if (OnTaskTick == null && !created)
        {
            Helper.LogAndBreak("Task's OnTaskTick event is null");
        }
    }

    public TaskStatus Tick(out INode nodeRunning, GameObject owner)
    {
        TaskStatus ts = TaskStatus.FAILURE;
        if (OnTaskTick != null)
        {
            ts = OnTaskTick(owner, _blackboard);
        }
        if (ts == TaskStatus.RUNNING)
        {
            nodeRunning = this;
        }
        else
        {
            nodeRunning = null;
        }
        return ts;
    }

    public void AddChild(INode child)
    {
        Debug.Log("You can't add child to task :) Nothing happened");
    }

    public void RemoveChild(INode child)
    {
        //I have no child
    }

    public void SetParent(INode parent)
    {
        _parent = parent;
    }

    public INode GetParent()
    {
        return _parent;
    }

    public void Remove()
    {
        if (_parent != null)
        {
            _parent.RemoveChild(this);
        }
        else
        {
            if (_bt != null)
            {
                _bt.RemoveChild();
            }
        }
    }

    public void MakeRoot(BehaviorTree bt)
    {
        _bt = bt;
    }

    public bool IsRoot()
    {
        return _bt != null;
    }

    public void SetMethod(Type t, string methodName)
    {
        _methodName = methodName;
        _methodType = t;

        if (t == null)
        {
            OnTaskTick = null;
            return;
        }

        Type delegateType = typeof(TickDelegate);
        MethodInfo mi = t.GetMethod(methodName);

        Component component = _owner.GetComponent(_methodType);
        Delegate d = Delegate.CreateDelegate(delegateType, component, mi, true);
        EventInfo ei = typeof(Task).GetEvent("OnTaskTick");
        MethodInfo addHandler = ei.GetAddMethod();
        System.Object[] addHandlerArgs = { d };
        addHandler.Invoke(this, addHandlerArgs);
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        reader.ReadStartElement("Task");
        reader.ReadStartElement("MethodType");
        string mtName = reader.ReadString();
        _methodType = Type.GetType(mtName);
        reader.ReadEndElement();
        reader.ReadStartElement("MethodName");
        _methodName = reader.ReadString();
        reader.ReadEndElement();
        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("Task");
        writer.WriteStartElement("MethodType");
        writer.WriteString(_methodType == null ? "NULL" : _methodType.ToString());
        writer.WriteEndElement();
        writer.WriteStartElement("MethodName");
        writer.WriteString(_methodName == string.Empty ? "NULL" : _methodName);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }
}

public class Decorator : INode
{
    private BehaviorTree _bt;
    private INode _parent;
    private INode _child;

    public INode Child
    {
        get { return _child; }
        set { _child = value; }
    }

    public DecoratorType Type
    {
        get;
        set;
    }
        
    public void Initialize(Blackboard blackboard, GameObject owner, bool created)
    {
        if(_child == null)
        {
            if(!created)
            {
                Helper.LogAndBreak("Decorator's child is null");
            }
        }
        else
        {
            _child.Initialize(blackboard, owner, created);
        }
    }

    public TaskStatus Tick(out INode nodeRunning, GameObject owner)
    {
        nodeRunning = null;
        TaskStatus toRet = Child.Tick(out nodeRunning, owner);
        switch(Type)
        {
            case DecoratorType.REVERT_RESULT:
                if(toRet == TaskStatus.FAILURE)
                {
                    toRet = TaskStatus.SUCCESS;
                }
                else if(toRet == TaskStatus.SUCCESS)
                {
                    toRet = TaskStatus.FAILURE;
                }
                break;
            case DecoratorType.ALWAYS_FAIL:
                if (toRet != TaskStatus.RUNNING)
                {
                    toRet = TaskStatus.FAILURE;
                }
                break;
            case DecoratorType.ALWAYS_SUCC:
                if(toRet != TaskStatus.RUNNING)
                {
                    toRet = TaskStatus.SUCCESS;
                }
                break;
        }
        return toRet;
    }

    public void AddChild(INode child)
    {
        Child = child;
    }

    public INode GetParent()
    {
        return _parent;
    }

    public void RemoveChild(INode child = null)
    {
        _child = null;
    }

    public void SetParent(INode parent)
    {
        _parent = parent;
    }

    public bool IsRoot()
    {
        return _bt != null;
    }

    public void Remove()
    {
        if (_parent != null)
        {
            _parent.RemoveChild(this);
        }
        else
        {
            if (_bt != null)
            {
                _bt.RemoveChild();
            }
        }
    }

    public void MakeRoot(BehaviorTree bt)
    {
        _bt = bt;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        reader.ReadStartElement("Decorator");
        reader.ReadStartElement("Child");
        string typeName = reader.ReadString();
        if(!typeName.Equals("NULL"))
        {
            Type type = System.Type.GetType(typeName);
            _child = (INode)Activator.CreateInstance(type);
            _child.SetParent(this);
            _child.ReadXml(reader);
        }
        reader.ReadEndElement();
        reader.ReadStartElement("DecoratorType");
        string decoType = reader.ReadString();
        Type = (DecoratorType)Enum.Parse(typeof(DecoratorType), decoType);
        reader.ReadEndElement();
        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("Decorator");
        writer.WriteStartElement("Child");
        if(_child != null)
        {
            writer.WriteString(_child.GetType().ToString());
            _child.WriteXml(writer);
        }
        else
        {
            writer.WriteString("NULL");
        }
        writer.WriteEndElement();
        writer.WriteStartElement("DecoratorType");
        writer.WriteString(Type.ToString());
        writer.WriteEndElement();
        writer.WriteEndElement();
    }
}

public class BehaviorTree : IXmlSerializable
{
    public INode Child
    {
        get;
        private set;
    }
    public INode CurrentRunning;
    public float TickDelay
    {
        get;
        set;
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get { return _isRunning; }
        set
        {
            _isRunning = value;
            if(_isRunning)
            {
                _timer = TickDelay;
            }
        }
    }

    private float _timer = 0.0f;
    private GameObject _owner;
    private Blackboard _blackboard;

    public void Initialize(GameObject owner, Blackboard blackboard, bool created = false)
    {
        _timer = 0.0f;
        _blackboard = blackboard;
        _owner = owner;

        if (Child == null)
        {
            if (!created)
            {
                Helper.LogAndBreak("Behavior Tree has no child");
            }
        }
        else
        {
            if(owner == null)
            {
                Helper.LogAndBreak("Owner of behavior tree is not set");
            }
            else
            {
                Child.Initialize(blackboard, owner, created);
            }
        }
    }

    public void AddChild(INode child)
    {
        Child = child;
        Child.Initialize(_blackboard, _owner, true);
        Child.MakeRoot(this);
    }

    public void RemoveChild()
    {
        Child = null;
    }

    public void Tick()
    {
        if(_isRunning)
        {
            _timer += Time.deltaTime;
            if (_timer >= TickDelay)
            {
                Child.Tick(out CurrentRunning, _owner);
                _timer = 0.0f;
            }
            else if (CurrentRunning != null)
            {
                if (CurrentRunning.Tick(out CurrentRunning, _owner) != TaskStatus.RUNNING)
                {
                    CurrentRunning = null;
                    _timer = TickDelay;
                }
            }
        }
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        try
        {
            reader.ReadStartElement("BehaviorTree");
            reader.ReadStartElement("BehaviorTree");
            reader.ReadStartElement("TickDelay");
            TickDelay = reader.ReadContentAsFloat();
            reader.ReadEndElement();
            reader.ReadStartElement("Child");
            string childType = reader.ReadString();
            if(!childType.Equals("NULL"))
            {
                Child = (INode)Activator.CreateInstance(Type.GetType(childType));
                Child.MakeRoot(this);
                Child.ReadXml(reader);
            }
            reader.ReadEndElement();
            reader.ReadEndElement();
            reader.ReadEndElement();
        }
        catch(XmlException xmle)
        {
            throw xmle;
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("BehaviorTree");
        writer.WriteStartElement("TickDelay");
        writer.WriteValue(TickDelay);
        writer.WriteEndElement();
        writer.WriteStartElement("Child");
        if(Child != null)
        {
            writer.WriteString(Child.GetType().ToString());
            Child.WriteXml(writer);
        }
        else
        {
            writer.WriteString("NULL");
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }
}
