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
            Patient.RegisterNewExplosion();
            OnExploded?.Invoke(id);
        }

        public static void Incinerated(int id)
        {
            Patient.RegisterNewIncineration(id);
            OnIncinerated?.Invoke(id);
        }

        public static void ClearAll()
        {
            OnLevelChanged = null;
            OnExploded = null;
            OnIncinerated = null;
        }
    }
}