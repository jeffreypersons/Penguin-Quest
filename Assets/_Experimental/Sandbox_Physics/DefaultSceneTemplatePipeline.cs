using UnityEngine.SceneManagement;
using UnityEditor.SceneTemplate;


namespace PQ._Experimental.Physics
{
    // template for physics test scenes
    public class DefaultSceneTemplatePipeline : ISceneTemplatePipeline
    {
        public virtual bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
        {
            return true;
        }

        public virtual void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, bool isAdditive, string sceneName)
        {
            // no op
        }

        public virtual void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
        {
            // no op
        }
    }
}
