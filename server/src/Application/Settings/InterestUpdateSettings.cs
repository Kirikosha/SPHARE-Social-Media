using Application.Models;

namespace Application.Settings;

public class InterestUpdateSettings
{
    public float DecayFactor { get; set; } = 0.85f;
    public float MinWeight { get; set; } = -0.5f;
    public float MaxWeight { get; set; } = 0.8f;
    public float PruneThreshold { get; set; } = 0.05f;
    public Dictionary<SignalType, float> SignalWeights { get; set; } = new()
    {
        { SignalType.Like, 0.3f },
        { SignalType.Unlike, -0.2f },
        { SignalType.Comment, 0.5f }
    }; 
}

public enum SignalType { Like, Unlike, Comment }