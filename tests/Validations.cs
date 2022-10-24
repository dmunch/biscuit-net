using System.Runtime.Serialization;

namespace tests.Json.Samples;

public partial class Testcase
{
    // Based on the value of the name in the attribute, the generator knows not to generate this property
    [DataMember(Name = "validations")]
    public Dictionary<string, File1> Validations { get; set; } // Generates 'int' by default
}

public partial class Result
{
    [DataMember(Name = "Ok", EmitDefaultValue = false, Order = 0)]
    public int? Ok { get; set; }
}

public partial class Err
{
    [DataMember(Name = "Format", EmitDefaultValue = false, Order = 0)]
    public Format Format { get; set; }
}

public partial class Format
{
    [DataMember(Name = "Signature", EmitDefaultValue = false, Order = 0)]
    public Signature Signature { get; set; }

    [DataMember(Name = "InvalidSignatureSize", EmitDefaultValue = false, Order = 1)]
    public int? InvalidSignatureSize { get; set; }
}

public partial class Signature
{
    [DataMember(Name = "InvalidSignature", EmitDefaultValue = false, Order = 0)]
    public string InvalidSignature { get; set; }
}

public partial class Check
{
    [DataMember(Name = "Authorizer", EmitDefaultValue = false, Order = 1)]
    public Authorizer Authorizer { get; set; }
}

public partial class Authorizer
{
    [DataMember(Name = "check_id", EmitDefaultValue = false, Order = 0)]
    public int CheckId { get; set; }
    [DataMember(Name = "Rule", EmitDefaultValue = false, Order = 1)]
    public string Rule { get; set; }
}
