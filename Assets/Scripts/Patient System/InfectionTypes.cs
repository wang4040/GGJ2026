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
        Crazy = 4,
        Exploded = 5,
        Incinerating = 6,
    }

    public enum InfectionStage
    {
        NoChange,
        Better,
        Worse
    }
}

