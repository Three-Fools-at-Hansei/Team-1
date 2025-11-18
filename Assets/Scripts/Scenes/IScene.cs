using System.Collections.Generic;

public interface IScene
{
    List<string> RequiredDataFiles { get; }

    public void Init();
    public void Clear();
    public eSceneType SceneType { get; }
}
