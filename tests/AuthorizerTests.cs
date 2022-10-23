using VeryNaiveDatalog;
using biscuit_net;
using System.Text.RegularExpressions;

namespace tests;
public class AuthorizerTests
{
    [Theory]
    //[BiscuitCases("test1_basic.bc")] //OK
    //[BiscuitCases("test7_scoped_rules.bc")] //OK
    //[BiscuitCases("test8_scoped_checks.bc")] //OK
    //[BiscuitCases("test9_expired_token.bc")] //OK - why? TODO needs expressions
    //[BiscuitCases("test10_authorizer_scope.bc")] //TODO: rules in authorizer
    //[BiscuitCases("test11_authorizer_authority_caveats.bc")] //TODO: rules in authorizer
    //[BiscuitCases("test12_authority_caveats.bc")] //OK
    //[BiscuitCases("test13_block_rules.bc")] //TODO contains time and expressions
    //[BiscuitCases("test14_regex_constraint.bc")] //TODO contains string expressions regex
    //[BiscuitCases("test16_caveat_head_name.bc")] //OK
    //[BiscuitCases("test18_unbound_variables_in_rule.bc")] //TODO
    //[BiscuitCases("test19_generating_ambient_from_variables.bc")] //OK
    //[BiscuitCases("test23_execution_scope.bc")] //TODO contains int term
    [BiscuitCases(BiscuitCases.CaseType.ErrFailedLogic)]
    public void Test(BiscuitCase biscuitCase)
    {
        var biscuit = Biscuit.Deserialize(biscuitCase.Token);
        
        var authorizer = new Authorizer();

        foreach(var authorizerAtom in Parse(biscuitCase.Validation.Authorizer_code))
        {
            authorizer.AddAtom(authorizerAtom);
        }
        
        var (check, failedBlockId, failedCheckId, failedRule) = authorizer.Authorize(biscuit);
        if(biscuitCase.Validation.Result.Ok != null)
        {
            Assert.True(check);
            return;
        }
        
        //we currently only assert on a single failed logic/unauthorized check
        Assert.NotNull(biscuitCase.Validation.Result.Err?.FailedLogic?.Unauthorized?.Checks?.SingleOrDefault());
        var blockCheck = biscuitCase.Validation.Result.Err.FailedLogic.Unauthorized.Checks.First().Block;

        Assert.Equal(blockCheck.Block_id, failedBlockId);
        Assert.Equal(blockCheck.Check_id, failedCheckId);
    }

    IEnumerable<Atom> Parse(string code)
    {
        string stringPattern = @"^([a-zA-Z]+)\(""([a-zA-Z.0-9]+)""\);$";
        string datePattern = @"^([a-zA-Z]+)\(((?:(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2}:\d{2}(?:\.\d+)?))(Z|[\+-]\d{2}:\d{2})?)\);$";

        var stringRegex = new Regex(stringPattern);
        var dateRegex = new Regex(datePattern);

        var lines = code
            .Split("\n")
            .Where(line => !string.IsNullOrEmpty(line));

        
        return code
            .Split("\n")
            .Where(line => !string.IsNullOrEmpty(line))
            .Select(line => {
                
                var stringMatch = stringRegex.Match(line);
                if(stringMatch.Success) 
                {
                    var name = stringMatch.Groups[1].Value;
                    var value = stringMatch.Groups[2].Value;

                    return new Atom(name, new String(value));
                }

                var dateMatch = dateRegex.Match(line);
                if(dateMatch.Success) 
                {
                    var name = dateMatch.Groups[1].Value;
                    var date = dateMatch.Groups[2].Value;

                    var dateParsed = DateTime.Parse(date);
                    var dateTAI = Date.ToTAI64(dateParsed);
                    return new Atom(name, new Date(dateTAI));
                }

                throw new NotSupportedException(line);
            }).ToList();
    }
}