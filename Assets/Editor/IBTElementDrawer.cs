using System.Collections.Generic;

public interface IBTElementDrawer
{
    void SetMethodsList(List<string> methodsList);
    void SetDecoratorsList(List<string> decorators);
    void DrawBehaviorTree(BehaviorTree behaviorTree);
}
