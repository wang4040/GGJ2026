using System;

namespace PatientSystem
{
    public static class PatientEvents
    {
        public static event Action<int, Level, Level> OnLevelChanged;
        public static event Action<int> OnExploded;
        public static event Action<int> OnIncinerated;

        public static void LevelChanged(int id, Level oldLevel, Level newLevel)
        {
            OnLevelChanged?.Invoke(id, oldLevel, newLevel);
        }

        public static void Exploded(int id)
        {
            Patient.RegisterNewDeath();
            OnExploded?.Invoke(id);
        }

        public static void Incinerated(int id)
        {
            Patient.RegisterNewDeath();
            OnIncinerated?.Invoke(id);
        }

        public static void ClearAll()
        {
            OnLevelChanged = null;
            OnExploded = null;
        }
    }
}