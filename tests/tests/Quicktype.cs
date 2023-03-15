/// This file is auto-generated from samples.json using Quicktype
/// Some manual adjustement needed to be made to make it compile and behave as desired
#nullable disable 
namespace QuickType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using R = Newtonsoft.Json.Required;
    using N = Newtonsoft.Json.NullValueHandling;

    public partial class Samples
    {
        [J("root_private_key")] public string RootPrivateKey { get; set; }
        [J("root_public_key")]  public string RootPublicKey { get; set; } 
        [J("testcases")]        public Testcase[] Testcases { get; set; } 
    }

    public partial class Testcase
    {
        [J("title")]       public string Title { get; set; }           
        [J("filename")]    public string Filename { get; set; }        
        [J("token")]       public Token[] Token { get; set; }          
        [J("validations")] public Validations Validations { get; set; }
    }

    public partial class Token
    {
        [J("symbols")]      public string[] Symbols { get; set; }   
        [J("public_keys")]  public string[] PublicKeys { get; set; }
        [J("external_key")] public string ExternalKey { get; set; } 
        [J("code")]         public string Code { get; set; }        
    }

    public partial class Validations : Dictionary<string, File>
    {
    }

    public partial class World
    {
        [J("facts")]    public string[] Facts { get; set; }          
        [J("rules")]    public string[] Rules { get; set; }          
        [J("checks")]   public string[] Checks { get; set; }         
        [J("policies")] public string[] Policies { get; set; }
    }

    public partial class Block
    {
        [J("block_id")] public uint BlockId { get; set; }
        [J("check_id")] public int CheckId { get; set; }
        [J("rule")]     public string Rule { get; set; } 
    }

    public partial class PolicyClass
    {
        [J("Allow")] public PolicyElement Allow { get; set; }
    }

    public partial class Empty
    {
        [J("world")]           public World World { get; set; }           
        [J("result")]          public Result Result { get; set; }         
        [J("authorizer_code")] public string AuthorizerCode { get; set; } 
        [J("revocation_ids")]  public string[] RevocationIds { get; set; }
    }

    public partial class Result
    {
        [J("Err", NullValueHandling = N.Ignore)] public Error Err { get; set; }
        [J("Ok", NullValueHandling = N.Ignore)]  public long? Ok { get; set; }     
    }

    public partial class Error
    {
        [J("FailedLogic", NullValueHandling = N.Ignore)] public FailedLogic FailedLogic { get; set; }
        [J("Format", NullValueHandling = N.Ignore)]      public Format Format { get; set; }                
        [J("Execution", NullValueHandling = N.Ignore)]      public string Execution { get; set; }                
    }

    public partial class FailedLogic
    {
        [J("Unauthorized", NullValueHandling = N.Ignore)]     public Unauthorized Unauthorized { get; set; }    
        [J("InvalidBlockRule", NullValueHandling = N.Ignore)] public InvalidBlockRule[] InvalidBlockRule { get; set; }
    }

    public partial class Unauthorized
    {
        [J("policy")] public PolicyClass Policy { get; set; }  
        [J("checks")] public Check[] Checks { get; set; }
    }

    public partial class Check
    {
        [J("Block", NullValueHandling = N.Ignore)]      public Block Block { get; set; }          
        [J("Authorizer", NullValueHandling = N.Ignore)] public Authorizer Authorizer { get; set; }
    }

    public partial class Authorizer
    {
        [J("check_id")] public int CheckId { get; set; }
        [J("rule")]     public string Rule { get; set; } 
    }

    public partial class Format
    {
        [J("Signature", NullValueHandling = N.Ignore)]            public Signature Signature { get; set; }       
        [J("InvalidSignatureSize", NullValueHandling = N.Ignore)] public long? InvalidSignatureSize { get; set; }
    }

    public partial class Signature
    {
        [J("InvalidSignature")] public string InvalidSignature { get; set; }
    }

    public partial class File
    {
        [J("world")]           public World World { get; set; }           
        [J("result")]          public FileResult Result { get; set; }    
        [J("authorizer_code")] public string AuthorizerCode { get; set; } 
        [J("revocation_ids")]  public string[] RevocationIds { get; set; }
    }

    public partial class FileResult
    {
        [J("Ok", NullValueHandling = N.Ignore)]  public long? Ok { get; set; }     
        [J("Err", NullValueHandling = N.Ignore)] public Error Err { get; set; }
    }

    public enum PolicyElement { AllowIfTrue };

    public partial struct InvalidBlockRule
    {
        public long? Integer;
        public string String;

        public static implicit operator InvalidBlockRule(long Integer) => new InvalidBlockRule { Integer = Integer };
        public static implicit operator InvalidBlockRule(string String) => new InvalidBlockRule { String = String };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                InvalidBlockRuleConverter.Singleton,
                PolicyElementConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class InvalidBlockRuleConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(InvalidBlockRule) || t == typeof(InvalidBlockRule?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new InvalidBlockRule { Integer = integerValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new InvalidBlockRule { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type InvalidBlockRule");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (InvalidBlockRule)untypedValue;
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type InvalidBlockRule");
        }

        public static readonly InvalidBlockRuleConverter Singleton = new InvalidBlockRuleConverter();
    }

    internal class PolicyElementConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(PolicyElement) || t == typeof(PolicyElement?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "allow if true")
            {
                return PolicyElement.AllowIfTrue;
            }
            throw new Exception("Cannot unmarshal type PolicyElement");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (PolicyElement)untypedValue;
            if (value == PolicyElement.AllowIfTrue)
            {
                serializer.Serialize(writer, "allow if true");
                return;
            }
            throw new Exception("Cannot marshal type PolicyElement");
        }

        public static readonly PolicyElementConverter Singleton = new PolicyElementConverter();
    }
}
