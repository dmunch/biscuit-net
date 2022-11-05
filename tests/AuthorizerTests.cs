using VeryNaiveDatalog;
using biscuit_net;
using System.Text.RegularExpressions;
using parser;

namespace tests;
public class AuthorizerTests
{
    [Theory]
    //[BiscuitCases("test1_basic.bc")] //OK
    //[BiscuitCases("test2_different_root_key.bc")] //OK
    //[BiscuitCases("test3_invalid_signature_format.bc")] //OK
    //[BiscuitCases("test7_scoped_rules.bc")] //OK
    //[BiscuitCases("test8_scoped_checks.bc")] //OK
    //[BiscuitCases("test9_expired_token.bc")] //OK - why? TODO needs expressions
    //[BiscuitCases("test10_authorizer_scope.bc")] //TODO: rules in authorizer
    //[BiscuitCases("test11_authorizer_authority_caveats.bc")] //TODO: rules in authorizer
    //[BiscuitCases("test12_authority_caveats.bc")] //OK
    //[BiscuitCases("test13_block_rules.bc")] //TODO contains time and string set expressions
    //[BiscuitCases("test14_regex_constraint.bc")] //TODO contains string expressions regex
    //[BiscuitCases("test16_caveat_head_name.bc")] //OK
    //[BiscuitCases("test17_expressions.bc")] //OK
    //[BiscuitCases("test18_unbound_variables_in_rule.bc")] //TODO
    //[BiscuitCases("test19_generating_ambient_from_variables.bc")] //OK
    //[BiscuitCases("test22_default_symbols.bc")] //TODO contains int term
    //[BiscuitCases("test23_execution_scope.bc")] //TODO contains int term
    [BiscuitCases()]
    public void Test(BiscuitCase biscuitCase)
    {
        var validator = new SignatureValidator(biscuitCase.RootPublicKey);
        if(!Biscuit.TryDeserialize(biscuitCase.Token, validator, out var biscuit, out var formatErr))
        {
            Assert.False(biscuitCase.Success);
            Assert.Equal(biscuitCase.Validation.FormatError, formatErr);
            return;
        }
        
        var authorizer = new Authorizer();

        foreach(var (authorizerAtom, authorizerCheckRule) in Parse(biscuitCase.Validation.AuthorizerCode))
        {
            if(authorizerAtom != null)
            {
                authorizer.AddAtom(authorizerAtom);
            }
            if(authorizerCheckRule != null)
            {
                authorizer.AddCheck(new Check(new []{authorizerCheckRule}));
            }
        }
        
        var check = authorizer.TryAuthorize(biscuit, out var err);
        if(biscuitCase.Success)
        {
            Assert.True(check);
            return;
        }
        
        Assert.Equal(biscuitCase.Validation.Error, err);
    }

    IEnumerable<(Atom?, RuleExpressions?)> Parse(string code)
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

        foreach(var line in lines)
        {
            var stringMatch = stringTermRegex.Match(line);
            var dateMatch = dateTermRegex.Match(line);
            if(stringMatch.Success) 
            {
                var name = stringMatch.Groups[1].Value;
                var value = stringMatch.Groups[2].Value;

                yield return (new Atom(name, new String(value)), null);
            }
            else if(dateMatch.Success) 
            {
                var name = dateMatch.Groups[1].Value;
                var date = dateMatch.Groups[2].Value;

                var dateParsed = DateTime.Parse(date);
                var dateTAI = Date.ToTAI64(dateParsed);
                yield return (new Atom(name, new Date(dateTAI)), null);
            }
            else if(line.StartsWith("check if"))
            {
                var parser = new parser.Parser();
                yield return (null, parser.ParseRule(line));
            }

            else throw new NotSupportedException(line);
        }
    }
}