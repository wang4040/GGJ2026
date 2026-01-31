using UnityEngine;

namespace PatientSystem
{
    public enum PatientType
    {
        Child,
        Old
    }

    public enum Level
    {
        Normal = 0,
        Slight = 1,
        Medium = 2,
        Severe = 3,
        Exploded = 4,
        Incinerating = 5
    }

    public enum InfectionStage
    {
        NoChange,
        Better,
        Worse
    }
}

