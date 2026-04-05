namespace Domain;

public static class InterestSignalWeights
{
    public const float View = 0.1f;
    public const float ViewWithDwell = 0.25f; // spent > 10 seconds
    public const float Like = 0.6f;
    public const float Comment = 0.75f;
    public const float Unlike = -0.2f;
}