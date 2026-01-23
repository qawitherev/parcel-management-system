namespace ParcelManagement.Api.CustomAttribute
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipBlacklistCheckAttribute : Attribute {}
}