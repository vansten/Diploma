using System.Collections.Generic;

public interface IBTElementDrawer
{
    void SetDecoratorsList(List<string> decorators);
    void SetMethodsList(List<string> methods);
    void DrawBehaviorTree(BehaviorTree behaviorTree);
}
