using UnityEngine;

// Level 1 uses the first two crushers, level 2 uses all four and they move faster.
public class LevelSetup : MonoBehaviour
{
    public HazardMover[] hazards;

    void Awake()
    {
        int level = GameSession.level;
        int count = level <= 1 ? 2 : hazards.Length;
        float faster = 1f + 0.4f * (level - 1);
        for (int i = 0; i < hazards.Length; i++)
        {
            hazards[i].gameObject.SetActive(i < count);
            hazards[i].speed *= faster;
        }
    }
}
