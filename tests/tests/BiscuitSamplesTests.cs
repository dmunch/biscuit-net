using biscuit_net;
using biscuit_net.Parser;

namespace tests;
public class BiscuitSamplesTests
{
    [Theory]
    //[BiscuitCases("test001_basic.bc")] //OK
    //[BiscuitCases("test002_different_root_key.bc")] //OK
    //[BiscuitCases("test003_invalid_signature_format.bc")] //OK
    //[BiscuitCases("test007_scoped_rules.bc")] //OK
    //[BiscuitCases("test008_scoped_checks.bc")] //OK
    //[BiscuitCases("test009_expired_token.bc")] //OK - why? TODO needs expressions
    //[BiscuitCases("test010_authorizer_scope.bc")] //TODO: rules in authorizer
    //[BiscuitCases("test011_authorizer_authority_caveats.bc")] //TODO: rules in authorizer
    //[BiscuitCases("test012_authority_caveats.bc")] //OK
    //[BiscuitCases("test013_block_rules.bc")] //TODO contains time and string set expressions
    //[BiscuitCases("test014_regex_constraint.bc")] //TODO contains string expressions regex
    //[BiscuitCases("test016_caveat_head_name.bc")] //OK
    //[BiscuitCases("test017_expressions.bc")] //OK
    //[BiscuitCases("test018_unbound_variables_in_rule.bc")] //TODO
    //[BiscuitCases("test019_generating_ambient_from_variables.bc")] //OK
    //[BiscuitCases("test022_default_symbols.bc")] //TODO contains int term
    //[BiscuitCases("test023_execution_scope.bc")] //TODO contains int term
    //[BiscuitCases("test024_third_party.bc")] //TODO 
    //[BiscuitCases("test025_check_all.bc")] //TODO
    //[BiscuitCases("test026_public_keys_interning.bc")] //TODO
    //[BiscuitCases("test027_integer_wraparound.bc")] //TODO
    [BiscuitSamples()]
    public void Test(BiscuitSample biscuitCase)
    {
        var validator = new SignatureValidator(biscuitCase.RootPublicKey);
        if(!Biscuit.TryDeserialize(biscuitCase.Token, validator, out var biscuit, out var formatErr))
        {
            Assert.False(biscuitCase.Success);
            Assert.Equal(biscuitCase.Validation.FormatError, formatErr);
            return;
        }

        var authorizer = new Authorizer();

        /*foreach(var parseResult in Parse(biscuitCase.Validation.AuthorizerCode))
        {
            switch(parseResult)
            {
                case (var authorizerFact, null, null): authorizer.Add(authorizerFact); break;
                case (null, var authorizerCheckRule, null): authorizer.Add(new Check(new []{authorizerCheckRule})); break;
                case (null, null, var policy): authorizer.Add(policy); break;
                default: throw new Exception();
            }
        }*/
        var authorizerBlock = Parse(biscuitCase.Validation.AuthorizerCode);
        foreach(var fact in authorizerBlock.Facts)
        {
            authorizer.Add(fact);
        }

        foreach(var policy in authorizerBlock.Policies)
        {
            authorizer.Add(policy);
        }

        foreach(var chck in authorizerBlock.Checks)
        {
            authorizer.Add(chck);
        }

        var check = authorizer.TryAuthorize(biscuit, out var err);
        if(biscuitCase.Success)
        {
            Assert.True(check);
            return;
        }
        
        Assert.Equal(biscuitCase.Validation.Error, err);
    }

    [Theory]
    [BiscuitSamples()]
    public void Test_Revocation_Ids(BiscuitSample biscuitCase)
    {
        var validator = new SignatureValidator(biscuitCase.RootPublicKey);
        if(!Biscuit.TryDeserialize(biscuitCase.Token, validator, out var biscuit, out var formatErr))
        {
            return;
        }
        Assert.Equal(biscuitCase.Validation.RevocationIds, biscuit.RevocationIds);
    }

    AuthorizerBlock Parse(string code)
    {
        var parser = new Parser();

        return parser.ParseAuthorizer(code);
    }

#if false
    AuthorizerBlock Parse(string code)
    {
        string stringTermPattern = @"^([a-zA-Z_]+)\(""([a-zA-Z.0-9]+)""\);$";
        string intTermPattern = @"^([a-zA-Z_]+)\((\d+)\)$";
        string dateTermPattern = @"^([a-zA-Z_]+)\(((?:(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2}:\d{2}(?:\.\d+)?))(Z|[\+-]\d{2}:\d{2})?)\);$";

        var stringTermRegex = new Regex(stringTermPattern);
        var intTermRegex = new Regex(intTermPattern);
        var dateTermRegex = new Regex(dateTermPattern);

        var lines = code
            .Split("\n")
            .Where(line => !string.IsNullOrEmpty(line));

        var authorizerBlock = new AuthorizerBlock();
        foreach(var line in lines)
        {
            var stringMatch = stringTermRegex.Match(line);
            var dateMatch = dateTermRegex.Match(line);
            if(stringMatch.Success) 
            {
                var name = stringMatch.Groups[1].Value;
                var value = stringMatch.Groups[2].Value;

                authorizerBlock.Add(new Fact(name, new biscuit_net.Datalog.String(value)));
                return authorizerBlock;
            }
            else if(dateMatch.Success) 
            {
                var name = dateMatch.Groups[1].Value;
                var date = dateMatch.Groups[2].Value;

                var dateParsed = DateTime.Parse(date);
                var dateTAI = Date.ToTAI64(dateParsed);
                authorizerBlock.Add(new Fact(name, new Date(dateTAI)));
                return authorizerBlock;
            }
            else if(line.StartsWith("check if"))
            {
                var parser = new Parser();
                yield return (null, parser.ParseRule(line), null);
            }
            else if(line.StartsWith("allow if true"))
            {
                var parser = new Parser();
                yield return (null, null, Policy.AllowPolicy);
            }
            else if(line.StartsWith("deny if query(3)"))
            {
                var parser = new Parser();
                
                var policy = new Policy(PolicyKind.Deny, new [] { new RuleConstrained(
                        new Fact("policy1"), 
                        new [] {    
                            new Fact("query", new Integer(3))
                        },
                        Enumerable.Empty<Expression>(), 
                        Scope.DefaultRuleScope
                    ) 
                });

                yield return (null, null, policy);
            }
            else if(line.StartsWith("deny if query(1, 2)"))
            {
                var parser = new Parser();
                
                var policy = new Policy(PolicyKind.Deny, new [] { new RuleConstrained(
                        new Fact("policy2"), 
                        new [] {    
                            new Fact("query", new Integer(1), new Integer(2))
                        },
                        Enumerable.Empty<Expression>(), 
                        Scope.DefaultRuleScope
                    ) 
                });
                yield return (null, null, policy);
            }
            else if(line.StartsWith("deny if query(0) trusting ed25519/3c8aeced6363b8a862552fb2b0b4b8b0f8244e8cef3c11c3e55fd553f3a90f59"))
            {
                var parser = new Parser();
                
                var scope = new Scope(new [] { ScopeType.Authority }, new [] { new PublicKey(Algorithm.Ed25519, Convert.FromHexString("3c8aeced6363b8a862552fb2b0b4b8b0f8244e8cef3c11c3e55fd553f3a90f59"))});
                var policy = new Policy(PolicyKind.Deny, new [] { new RuleConstrained(
                        new Fact("policy3"), 
                        new [] {    
                            new Fact("query", new Integer(0))
                        },
                        Enumerable.Empty<Expression>(), 
                        scope
                    ) 
                });

                yield return (null, null, policy);
            }
            

            else throw new NotSupportedException(line);
        }
    }
#endif
}