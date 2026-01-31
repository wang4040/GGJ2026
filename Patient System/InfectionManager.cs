using System;
using System.Collections.Generic;
using System.Linq;

namespace PatientSystem
{
    public class InfectionManager
    {
        // ----------------------------------------------------
        // Configuration
        // ----------------------------------------------------
        public float SpreadCheckInterval { get; set; } = 5f;

        private Dictionary<Room, float> infectionRates = new Dictionary<Room, float>
        {
            { Room.Hall, 0.3f },
            { Room.Isolation, 0.1f },
            { Room.Incinerator, 0.0f }
        };

        // ----------------------------------------------------
        // Timer Tracking
        // ----------------------------------------------------
        private Dictionary<int, float> infectionTimers = new Dictionary<int, float>();
        private Dictionary<int, float> graceTimers = new Dictionary<int, float>();
        private Dictionary<int, float> incinerationTimers = new Dictionary<int, float>();
        private float spreadTimer = 0f;

        private Random rng = new Random();

        // ----------------------------------------------------
        // Infection Change Chances (configurable)
        // ----------------------------------------------------
        // Higher level = harder to change (more stable but dangerous)
        public float NormalChangeChance = 0.50f;
        public float SlightChangeChance = 0.40f;
        public float MediumChangeChance = 0.30f;
        public float SevereChangeChance = 0.20f;
        public float CrazyChangeChance = 0f;

        // ----------------------------------------------------
        // Events
        // ----------------------------------------------------

        public event Action<int> OnIncinerationComplete;
        public event Action<int> OnPatientInfected;

        // ----------------------------------------------------
        // Infection Rate
        // ----------------------------------------------------

        public float GetInfectionRate(Room room)
        {
            return infectionRates[room];
        }

        public void SetInfectionRate(Room room, float rate)
        {
            infectionRates[room] = rate;
        }

        // ----------------------------------------------------
        // Infection Tick Logic
        // ----------------------------------------------------

        // Process one infection tick for a patient
        // Returns what happened: NoChange, Better, or Worse
        public InfectionStage ProcessInfectionTick(Patient patient)
        {
            // Dead or incinerating patients don't change
            if (patient.Level >= Level.Exploded)
                return InfectionStage.NoChange;

            // Get room infection rate
            float roomRate = GetInfectionRate(patient.Room);

            // Step 1: Is the patient exposed to infection this tick?
            float exposureChance = patient.GetInfectionChance(roomRate);
            bool isExposed = Roll(exposureChance);

            // Step 2: Does a state change happen?
            float changeChance = GetChangeChance(patient.Level);
            bool stateChanges = Roll(changeChance);

            if (!stateChanges)
                return InfectionStage.NoChange;

            // Step 3: Apply the change based on exposure
            if (isExposed)
            {
                // Exposed to infection -> gets worse
                if (patient.IncreaseLevel())
                    return InfectionStage.Worse;
            }
            else
            {
                // Not exposed -> recovers
                if (patient.DecreaseLevel())
                    return InfectionStage.Better;
            }

            return InfectionStage.NoChange;
        }

        // Get the probability of a state change based on infection level
        private float GetChangeChance(Level level)
        {
            switch (level)
            {
                case Level.Normal: return NormalChangeChance;
                case Level.Slight: return SlightChangeChance;
                case Level.Medium: return MediumChangeChance;
                case Level.Severe: return SevereChangeChance;
                default: return 0f;
            }
        }

        // Roll a random chance (returns true if successful)
        private bool Roll(float chance)
        {
            return (float)rng.NextDouble() < chance;
        }

        // ----------------------------------------------------
        // Core Update Loop
        // ----------------------------------------------------
        public void Update(float deltaTime)
        {
            UpdateInfectionTimers(deltaTime);
            UpdateGraceTimers(deltaTime);
            UpdateIncinerationTimers(deltaTime);
            UpdateSpreadTimer(deltaTime);
        }

        // ----------------------------------------------------
        // Timer Registration
        // ----------------------------------------------------
        public void RegisterPatient(int patientId)
        {
            Patient patient = Patient.Get(patientId);
            if (patient == null) return;

            infectionTimers[patientId] = patient.TimerDuration;
        }

        public void UnregisterPatient(int patientId)
        {
            infectionTimers.Remove(patientId);
            graceTimers.Remove(patientId);
            incinerationTimers.Remove(patientId);
        }

        // ----------------------------------------------------
        // Infection Roll Timers
        // ----------------------------------------------------
        private void UpdateInfectionTimers(float deltaTime)
        {
            var patientsToRemove = new List<int>();
            var patientsToUpdate = new List<(int id, float newTime)>();

            // First pass: figure out what needs to change
            foreach (var entry in infectionTimers)
            {
                int patientId = entry.Key;
                float timeRemaining = entry.Value - deltaTime;

                Patient patient = Patient.Get(patientId);

                // Patient no longer exists
                if (patient == null)
                {
                    patientsToRemove.Add(patientId);
                    continue;
                }

                // Skip dead patients
                if (patient.Level >= Level.Exploded)
                    continue;

                // Timer expired - process infection tick
                if (timeRemaining <= 0f)
                {
                    ProcessInfectionTick(patient);

                    // Start grace timer if newly severe
                    if (patient.Level == Level.Severe && !graceTimers.ContainsKey(patientId))
                    {
                        graceTimers[patientId] = patient.GracePeriod;
                    }

                    // Reset timer
                    patientsToUpdate.Add((patientId, patient.TimerDuration));
                }
                else
                {
                    patientsToUpdate.Add((patientId, timeRemaining));
                }
            }

            // Second pass: apply changes
            foreach (int id in patientsToRemove)
            {
                infectionTimers.Remove(id);
            }

            foreach (var (id, newTime) in patientsToUpdate)
            {
                infectionTimers[id] = newTime;
            }
        }

        // ----------------------------------------------------
        // Grace Period Timers
        // ----------------------------------------------------
        private void UpdateGraceTimers(float deltaTime)
        {
            var toRemove = new List<int>();

            foreach (var entry in graceTimers.ToList())
            {
                int patientId = entry.Key;
                float timeRemaining = entry.Value - deltaTime;

                Patient patient = Patient.Get(patientId);

                // Patient gone or no longer severe - stop tracking
                if (patient == null || patient.Level != Level.Severe)
                {
                    toRemove.Add(patientId);
                    continue;
                }

                // Grace period complete
                if (timeRemaining <= 0f)
                {
                    patient.CompleteGracePeriod();
                    toRemove.Add(patientId);
                }
                else
                {
                    graceTimers[patientId] = timeRemaining;
                }
            }

            foreach (int id in toRemove)
            {
                graceTimers.Remove(id);
            }
        }

        // ----------------------------------------------------
        // Incineration Timers
        // ----------------------------------------------------
        public void StartIncineration(int patientId)
        {
            Patient patient = Patient.Get(patientId);
            if (patient == null) return;

            if (patient.Incinerate())
            {
                incinerationTimers[patientId] = patient.IncinerationTime;
            }
        }

        private void UpdateIncinerationTimers(float deltaTime)
        {
            var completed = new List<int>();

            foreach (var entry in incinerationTimers.ToList())
            {
                int patientId = entry.Key;
                float timeRemaining = entry.Value - deltaTime;

                if (timeRemaining <= 0f)
                {
                    Patient patient = Patient.Get(patientId);
                    patient?.FinishIncineration();

                    completed.Add(patientId);
                }
                else
                {
                    incinerationTimers[patientId] = timeRemaining;
                }
            }

            foreach (int id in completed)
            {
                incinerationTimers.Remove(id);
                OnIncinerationComplete?.Invoke(id);
            }
        }

        // ----------------------------------------------------
        // Infection Spread
        // ----------------------------------------------------

        private void UpdateSpreadTimer(float deltaTime)
        {
            spreadTimer -= deltaTime;

            if (spreadTimer <= 0f)
            {
                CheckInfectionSpread(Room.Hall);
                CheckInfectionSpread(Room.Isolation);
                spreadTimer = SpreadCheckInterval;
            }
        }

        private void CheckInfectionSpread(Room room)
        {
            var infectedPatients = Patient.GetInfectedInRoom(room);
            if (infectedPatients.Count == 0) return;

            var healthyPatients = Patient.GetHealthyInRoom(room);
            float roomInfectionRate = infectionRates[room];

            foreach (Patient patient in healthyPatients)
            {
                float infectionChance = patient.GetInfectionChance(roomInfectionRate);
                float roll = (float)rng.NextDouble();

                if (roll < infectionChance)
                {
                    patient.IncreaseLevel();
                    OnPatientInfected?.Invoke(patient.Id);
                }
            }
        }

        // ----------------------------------------------------
        // Cleanup
        // ----------------------------------------------------
        public void Reset()
        {
            infectionTimers.Clear();
            graceTimers.Clear();
            incinerationTimers.Clear();
            spreadTimer = 0f;
        }
    }
}