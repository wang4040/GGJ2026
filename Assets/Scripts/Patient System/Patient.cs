using System.Collections.Generic;
using Unity.VisualScripting;

namespace PatientSystem
{
    // ----------------------------------------------------
    // Room types
    // ----------------------------------------------------
    public enum Room
    {
        Hall, // place where normal patients stay.
        Isolation, // place where infected patients are isolated.
        Incinerator, // place where patients are burned.
    }

    public class Patient
    {
        // ----------------------------------------------------
        // Default Configuration Values
        // ----------------------------------------------------

        public static float DefaultChildTimer = 5f; // Time between infection ticks for children
        public static float DefaultOldTimer = 5f; // Time between infection ticks for old
        public static float DefaultGracePeriod = 10f; // Time before severe patient can explode
        public static float DefaultMaskMultiplier = 0.5f; // Mask reduces infection chance by 50%
        public static float DefaultMaskDuration = 10f; // How long the mask lasts before wearing off

        // ----------------------------------------------------
        // Patient Registry
        // ----------------------------------------------------

        private static int nextId = 1;
        private static Dictionary<int, Patient> allPatients = new Dictionary<int, Patient>();

        // ----------------------------------------------------
        // Instance
        // ----------------------------------------------------

        private Level currentLevel;

        // ----------------------------------------------------
        // Properties
        // ----------------------------------------------------

        public int Id { get; private set; }
        public PatientType Type { get; set; }
        public bool HasMask { get; private set; }
        public bool GracePeriodDone { get; private set; }
        public bool IsDragging { get; private set; } = false;
        public bool IsInIncinerator { get; private set; } = false;
        public float TimerDuration { get; set; }
        public float GracePeriod { get; set; }
        public float MaskMultiplier { get; set; }
        public float MaskDuration { get; set; }
        public float MaskTimer { get; set; } = 0f; // Tracks remaining mask time
        public Room Room { get; set; } = Room.Hall;

        // Current infection level with event notifications
        public Level Level
        {
            get { return currentLevel; }
            set
            {
                if (currentLevel == value)
                    return;

                Level oldLevel = currentLevel;
                currentLevel = value;

                // Notify listeners of level change
                PatientEvents.LevelChanged(Id, oldLevel, value);

                // Fire explosion event if patient exploded
                if (value == Level.Exploded)
                {
                    PatientEvents.Exploded(Id);
                }

                // Reset grace period when reaching severe
                if (value == Level.Severe)
                {
                    GracePeriodDone = false;
                }
            }
        }

        // True if patient is severe AND grace period has elapsed
        public bool CanExplode
        {
            get { return currentLevel == Level.Severe && GracePeriodDone; }
        }

        // True if patient has an active infection (Slight, Medium, or Severe)
        public bool IsInfected
        {
            get { return currentLevel >= Level.Slight && currentLevel <= Level.Severe; }
        }

        // ----------------------------------------------------
        // Constructor
        // ----------------------------------------------------

        public Patient(PatientType type, Level initialLevel = Level.Normal)
        {
            // Assign unique ID
            Id = nextId;
            nextId++;

            // Set initial state
            Type = type;
            currentLevel = initialLevel;
            HasMask = false;
            GracePeriodDone = false;

            // Set timer based on patient type
            TimerDuration = (type == PatientType.Child) ? DefaultChildTimer : DefaultOldTimer;

            // Apply default values
            GracePeriod = DefaultGracePeriod;
            MaskMultiplier = DefaultMaskMultiplier;
            MaskDuration = DefaultMaskDuration;

            // Register in global patient registry
            allPatients[Id] = this;
        }

        // ----------------------------------------------------
        // Patient Registry
        // ----------------------------------------------------

        // Get a patient by their ID
        public static Patient Get(int id)
        {
            if (allPatients.ContainsKey(id))
            {
                return allPatients[id];
            }
            return null;
        }

        // Get all registered patients
        public static Dictionary<int, Patient> GetAllPatients()
        {
            return allPatients;
        }

        // Remove a patient from the registry
        public static void Remove(int id)
        {
            allPatients.Remove(id);
        }

        // Clear all patients and reset ID counter
        public static void Reset()
        {
            allPatients.Clear();
            nextId = 1;
        }

        // ----------------------------------------------------
        // Mask
        // ----------------------------------------------------

        // Apply a mask to the patient.
        public bool ApplyMask()
        {
            if (HasMask)
                return false;

            // Only level 0, 1, 2 can wear mask
            if (currentLevel > Level.Medium)
                return false;

            HasMask = true;
            MaskTimer = MaskDuration; // Start the mask timer
            return true;
        }

        public bool RemoveMask()
        {
            HasMask = false;
            return true;
        }

        // Calculate infection chance based on room rate and mask status
        public float GetInfectionChance(float roomRate)
        {
            if (HasMask)
            {
                return roomRate * MaskMultiplier;
            }
            return roomRate;
        }

        // ----------------------------------------------------
        // Bite (Crazy patient attack)
        // ----------------------------------------------------

        /// <summary>
        /// Returns true if this patient can be bitten (level 0, 1, or 2)
        /// </summary>
        public bool CanBeBitten
        {
            get { return currentLevel <= Level.Medium && currentLevel != Level.Exploded; }
        }

        /// <summary>
        /// Returns true if this patient is Crazy and can bite others
        /// </summary>
        public bool CanBite
        {
            get { return currentLevel == Level.Crazy && !IsInIncinerator; }
        }

        /// <summary>
        /// Called when this patient is bitten by a Crazy patient.
        /// The bitten patient immediately becomes Severe.
        /// </summary>
        public void OnBitten()
        {
            if (!CanBeBitten)
                return;

            // Immediately become Severe
            Level = Level.Severe;
        }

        // ----------------------------------------------------
        // Level Changes
        // ----------------------------------------------------

        // Increase infection level by one. Returns false if unable to increase.
        public bool IncreaseLevel()
        {
            if (currentLevel >= Level.Crazy)
                return false;

            // Normal -> Slight -> Medium -> Severe -> Crazy -> Exploded
            Level = currentLevel + 1;
            return true;
        }

        // Decrease infection level by one. Returns false if unable to decrease.
        public bool DecreaseLevel()
        {
            // Can't go below normal
            if (currentLevel <= Level.Normal)
                return false;

            // Exploded/Incinerating patients can't recover
            if (currentLevel >= Level.Exploded)
                return false;

            Level = currentLevel - 1;
            return true;
        }

        // ----------------------------------------------------
        // Dragging
        // ----------------------------------------------------
        public void OnDragging()
        {
            IsDragging = true;
        }

        public void StopDragging()
        {
            IsDragging = false;
        }

        // ----------------------------------------------------
        // Room Registry
        // ----------------------------------------------------
        public static List<Patient> GetInfectedInRoom(Room room)
        {
            List<Patient> result = new List<Patient>();

            foreach (Patient patient in allPatients.Values)
            {
                if (patient.Room == room && patient.IsInfected)
                {
                    result.Add(patient);
                }
            }

            return result;
        }

        public static List<Patient> GetHealthyInRoom(Room room)
        {
            List<Patient> result = new List<Patient>();

            foreach (Patient patient in allPatients.Values)
            {
                if (patient.Room == room && patient.Level == Level.Normal)
                {
                    result.Add(patient);
                }
            }

            return result;
        }

        public void InIsolation()
        {
            if (IsInIncinerator)
                return;

            Room = Room.Isolation;
            StopDragging();
        }

        public void OutIsolation()
        {
            if (IsInIncinerator)
                return;
            Room = Room.Hall;
            StopDragging();
        }

        public void Incineration()
        {
            Room = Room.Incinerator;
            IsInIncinerator = true;
            IsDragging = false;
            PatientEvents.Incinerated(Id);
        }

        // ----------------------------------------------------
        // Infection Tick Calculator
        // ----------------------------------------------------

        public float GetWorseRate()
        {
            switch (currentLevel)
            {
                case Level.Normal:
                    return 0.3f; // 30% chance to get infected
                case Level.Slight:
                    return 0.4f;
                case Level.Medium:
                    return 0.5f;
                case Level.Severe:
                    return 0.7f;
                case Level.Crazy:
                    return 0.9f; // 90% chance to explode
                default:
                    return 0.5f;
            }
        }

        public float GetRecoveryRate()
        {
            // Patients can only recover in Isolation room
            if (Room != Room.Isolation)
                return 0f;

            switch (currentLevel)
            {
                case Level.Slight:
                    return 0.5f; // 50% chance to recover
                case Level.Medium:
                    return 0.3f;
                case Level.Severe:
                    return 0.15f;
                case Level.Crazy:
                    return 0.05f;
                default:
                    return 0f; // Normal can't recover further
            }
        }

        /// <summary>
        /// Processes an infection tick for this patient.
        /// 1. Roll against room infection rate to see if patient is "exposed" this tick
        /// 2. If exposed: roll against level-based worseRate to get worse
        /// 3. If NOT exposed: roll against level-based recoveryRate to get better
        /// </summary>
        public InfectionStage ProcessInfectionTick(float roomInfectionRate)
        {
            // Already dead, no processing
            if (currentLevel == Level.Exploded || IsInIncinerator)
                return InfectionStage.NoChange;

            // Calculate effective exposure chance
            float effectiveExposureChance = GetInfectionChance(roomInfectionRate);

            // Roll to see if patient is exposed this tick
            float exposureRoll = UnityEngine.Random.value;
            bool isExposed = exposureRoll < effectiveExposureChance;

            if (isExposed)
            {
                // Patient is exposed - roll to see if they get worse (level-dependent)
                float worseRoll = UnityEngine.Random.value;
                if (worseRoll < GetWorseRate())
                {
                    if (IncreaseLevel())
                        return InfectionStage.Worse;
                }
            }
            else
            {
                // Patient is not exposed - roll to see if they recover (level-dependent)
                if (currentLevel > Level.Normal)
                {
                    float recoveryRoll = UnityEngine.Random.value;
                    if (recoveryRoll < GetRecoveryRate())
                    {
                        if (DecreaseLevel())
                            return InfectionStage.Better;
                    }
                }
            }

            return InfectionStage.NoChange;
        }

        public float CalculateHealth()
        {
            switch (currentLevel)
            {
                case Level.Normal:
                    return 100f;
                case Level.Slight:
                    return 80f;
                case Level.Medium:
                    return 60f;
                case Level.Severe:
                    return 40f;
                case Level.Crazy:
                    return 20f;
                case Level.Exploded:
                    return 0f;
                default:
                    return 0f;
            }
        }

        // ----------------------------------------------------
        // Grace Period & Incineration
        // ----------------------------------------------------

        // Mark grace period as complete (severe patients can now explode)
        public void CompleteGracePeriod()
        {
            if (currentLevel == Level.Severe)
            {
                GracePeriodDone = true;
            }
        }

        public override string ToString()
        {
            return $"Patient[{Id}, {Type}, {currentLevel}, Mask={HasMask}]";
        }
    }
}
