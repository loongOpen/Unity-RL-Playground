using UnityEngine;

namespace Unity.MLAgents
{
    /// <summary>
    /// A class holding the capabilities flags for Reinforcement Learning across C# and the Trainer codebase.
    /// </summary>
    public class UnityRLCapabilities
    {
        /// <summary>
        /// Base RL capabilities.
        /// </summary>
        public bool BaseRLCapabilities;

        /// <summary>
        /// Concatenated PNG observations.
        /// </summary>
        public bool ConcatenatedPngObservations;

        /// <summary>
        /// Compressed channel mapping.
        /// </summary>
        public bool CompressedChannelMapping;

        /// <summary>
        /// Hybrid actions.
        /// </summary>
        public bool HybridActions;

        /// <summary>
        /// Training analytics.
        /// </summary>
        public bool TrainingAnalytics;

        /// <summary>
        /// Variable length observation.
        /// </summary>
        public bool VariableLengthObservation;

        /// <summary>
        /// Multi-agent groups.
        /// </summary>
        public bool MultiAgentGroups;

        /// <summary>
        /// A class holding the capabilities flags for Reinforcement Learning across C# and the Trainer codebase.  This
        /// struct will be used to inform users if and when they are using C# / Trainer features that are mismatched.
        /// </summary>
        /// <param name="baseRlCapabilities">Base RL capabilities.</param>
        /// <param name="concatenatedPngObservations">Concatenated PNG observations.</param>
        /// <param name="compressedChannelMapping">Compressed channel mapping.</param>
        /// <param name="hybridActions">Hybrid actions.</param>
        /// <param name="trainingAnalytics">Training analytics.</param>
        /// <param name="variableLengthObservation">Variable length observation.</param>
        /// <param name="multiAgentGroups">Multi-agent groups.</param>
        public UnityRLCapabilities(
            bool baseRlCapabilities = true,
            bool concatenatedPngObservations = true,
            bool compressedChannelMapping = true,
            bool hybridActions = true,
            bool trainingAnalytics = true,
            bool variableLengthObservation = true,
            bool multiAgentGroups = true)
        {
            BaseRLCapabilities = baseRlCapabilities;
            ConcatenatedPngObservations = concatenatedPngObservations;
            CompressedChannelMapping = compressedChannelMapping;
            HybridActions = hybridActions;
            TrainingAnalytics = trainingAnalytics;
            VariableLengthObservation = variableLengthObservation;
            MultiAgentGroups = multiAgentGroups;
        }

        /// <summary>
        /// Will print a warning to the console if Python does not support base capabilities and will
        /// return true if the warning was printed.
        /// </summary>
        /// <returns>True if the warning was printed, False if not.</returns>
        public bool WarnOnPythonMissingBaseRLCapabilities()
        {
            if (BaseRLCapabilities)
            {
                return false;
            }
            Debug.LogWarning("Unity has connected to a Training process that does not support" +
                "Base Reinforcement Learning Capabilities.  Please make sure you have the" +
                " latest training codebase installed for this version of the ML-Agents package.");
            return true;
        }
    }
}
