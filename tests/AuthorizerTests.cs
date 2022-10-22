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
    [BiscuitCases(BiscuitCases.CaseType.Success)]
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
        string pattern = @"^([a-zA-Z]+)\(""([a-zA-Z0-9]+)""\);";
        var regex = new Regex(pattern);
        return code
            .Split("\n")
            .Where(line => !string.IsNullOrEmpty(line))
            .Select(line => {
                var match = regex.Match(line);
                
                var name = match.Groups[1].Value;
                var value = match.Groups[2].Value;

                return new Atom(name, new Symbol(value));
            }).ToList();
    }
}