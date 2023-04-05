namespace biscuit_net.AspNet;

[AttributeUsage(AttributeTargets.Method)]
public class BiscuitPolicyAttribute : Attribute
{
    public string PolicyCode { get; set; }
    public BiscuitPolicyAttribute(string policyCode) 
    {
        PolicyCode = policyCode;
    }
}