using UnityEngine;

public class BehaviorTreeComponent : MonoBehaviour
{
    public TextAsset BehaviorTreeAsset;
    public bool RunOnEnable;

    private BehaviorTree _myTree;   

    void Awake()
    {
        if (BehaviorTreeAsset == null)
        {
            Helper.LogAndBreak("Missing behavior tree asset");
        }
        else
        {
            _myTree = BTSerializer.Deserialize(BehaviorTreeAsset);
            if (_myTree == null)
            {
                Helper.LogAndBreak("Behavior Tree is null. Please check out asset");
            }
            else
            {
                _myTree.Initialize(gameObject);
                if (RunOnEnable)
                {
                    Run();
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(_myTree != null)
        {
            _myTree.Tick();
        }
	}

    public void Run()
    {
        if (_myTree != null)
        {
            _myTree.IsRunning = true;
        }

    }

    public void Stop()
    {
        if (_myTree != null)
        {
            _myTree.IsRunning = false;
        }
    }
}
