// Implements: 02-mission-list.md scene manifest — every generated scene holds one SceneSetup naming its content id
// (a mission id like "M3.4", a test id like "Test_AtocBoss", or "_Boot"). LevelBuilder builds the placeholder content at runtime.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ruminahui
{
    public class SceneSetup : MonoBehaviour
    {
        public string contentId = "Test_PumaDummy";

        void Start()
        {
            // Additive streaming: Start can run before SceneStreamer calls SetActiveScene. Unparented objects created below
            // (characters, cameras) must land in THIS scene, not the one about to unload (LESSONS_LEARNED L-009).
            if (SceneManager.GetActiveScene() != gameObject.scene) SceneManager.SetActiveScene(gameObject.scene);
            if (MissionManager.Instance != null) MissionManager.Instance.OnSceneStarted(this);
            LevelBuilder.Build(contentId, transform);
        }
    }
}
