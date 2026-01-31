using System;

namespace PatientSystem
{
    public static class PatientEvents
    {
        public static event Action<int, Level, Level> OnLevelChanged;
        public static event Action<int> OnExploded;

        public static void LevelChanged(int id, Level oldLevel, Level newLevel)
        {
            OnLevelChanged?.Invoke(id, oldLevel, newLevel);
        }

        public static void Exploded(int id)
        {
            OnExploded?.Invoke(id);
        }

        public static void ClearAll()
        {
            OnLevelChanged = null;
            OnExploded = null;
        }
    }
}