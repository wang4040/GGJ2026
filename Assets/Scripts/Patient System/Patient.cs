using System.Collections.Generic;
using Unity.VisualScripting;

namespace PatientSystem
{
    // ----------------------------------------------------
    // Room types
    // ----------------------------------------------------
    public enum Room
    {
        Hall,
        Isolation,
        Incinerator
    }

    public class Patient
    {
        // ----------------------------------------------------
        // Default Configuration Values
        // ----------------------------------------------------

        public static float DefaultChildTimer = 10f;       // Time between infection ticks for children
        public static float DefaultOldTimer = 10f;         // Time between infection ticks for old
        public static float DefaultGracePeriod = 8f;       // Time before severe patient can explode
        public static float DefaultIncinerationTime = 5f;  // Time to complete incineration
        public static float DefaultMaskMultiplier = 0.5f;  // Mask reduces infection chance by 50%

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
        public PatientType Type { get; private set; }
        public bool HasMask { get; private set; }
        public bool GracePeriodDone { get; private set; }
        public bool IsDragging { get; private set; } = false;
        public bool IsInIncinerator { get; private set; } = false;
        public float TimerDuration { get; set; }
        public float GracePeriod { get; set; }
        public float IncinerationTime { get; set; }
        public float MaskMultiplier { get; set; }
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
            IncinerationTime = DefaultIncinerationTime;
            MaskMultiplier = DefaultMaskMultiplier;

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

        // Apply a mask to the patient. Returns false if already masked.
        public bool ApplyMask()
        {
            if (HasMask)
                return false;

            HasMask = true;
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
        // Level Changes
        // ----------------------------------------------------

        // Increase infection level by one. Returns false if unable to increase.
        public bool IncreaseLevel()
        {
            // Can't go higher than exploded
            if (currentLevel >= Level.Exploded)
                return false;

            // Severe patients must wait for grace period before exploding
            if (currentLevel == Level.Severe && !GracePeriodDone)
                return false;

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
            if (IsInIncinerator || currentLevel >= Level.Incinerating)
                return;
            
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
            Incinerate();  
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

        // Start incinerating the patient. Returns false if already dead.
        public bool Incinerate()
        {
            if (currentLevel >= Level.Exploded)
                return false;

            Level = Level.Incinerating;
            return true;
        }

        // Called when incineration completes
        public void FinishIncineration()
        {
        }

        public override string ToString()
        {
            return $"Patient[{Id}, {Type}, {currentLevel}, Mask={HasMask}]";
        }
    }
}