namespace DeCloud.Shared.Enums
{
    public enum NatType
    {
        Unknown,
        None,           // Public IP, no NAT
        FullCone,       // Easy to traverse
        RestrictedCone,
        PortRestricted,
        Symmetric       // Hardest to traverse, may need relay
    }
}
