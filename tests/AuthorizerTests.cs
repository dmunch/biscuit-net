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
    //[BiscuitCases("test13_block_rules.bc")] //TODO contains time and string set expressions
    //[BiscuitCases("test14_regex_constraint.bc")] //TODO contains string expressions regex
    //[BiscuitCases("test16_caveat_head_name.bc")] //OK
    //[BiscuitCases("test18_unbound_variables_in_rule.bc")] //TODO
    //[BiscuitCases("test19_generating_ambient_from_variables.bc")] //OK
    //[BiscuitCases("test22_default_symbols.bc")] //TODO contains int term
    //[BiscuitCases("test23_execution_scope.bc")] //TODO contains int term
    [BiscuitCases(BiscuitCases.CaseType.ErrFailedLogic)]
    public void Test(BiscuitCase biscuitCase)
    {
        var biscuit = Biscuit.Deserialize(biscuitCase.Token);
        
        var authorizer = new Authorizer();

        foreach(var (authorizerAtom, authorizerCheckRule) in Parse(biscuitCase.Validation.Authorizer_code))
        {
            if(authorizerAtom != null)
            {
                authorizer.AddAtom(authorizerAtom);
            }
            if(authorizerCheckRule != null)
            {
                authorizer.AddCheck(authorizerCheckRule);
            }
        }
        
        var check = authorizer.TryAuthorize(biscuit, out var err);
        if(biscuitCase.Validation.Result.Ok != null)
        {
            Assert.True(check);
            return;
        }
        
        //we currently only assert on a single failed logic/unauthorized check
        Assert.NotNull(biscuitCase.Validation.Result.Err?.FailedLogic?.Unauthorized?.Checks?.SingleOrDefault());
        var blockCheck = biscuitCase.Validation.Result.Err.FailedLogic.Unauthorized.Checks.First().Block;
        var authorizerCheck = biscuitCase.Validation.Result.Err.FailedLogic.Unauthorized.Checks.First().Authorizer;

        if(blockCheck != null)
        {
            Assert.Equal(blockCheck.Block_id, err.Block.BlockId);
            Assert.Equal(blockCheck.Check_id, err.Block.CheckId);
        } 
        else
        {
            Assert.Equal(authorizerCheck.CheckId, err.Authorizer.CheckId);
        }
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
            else if(line.StartsWith("check if right($0, $1), resource($0), operation($1)"))
            {
                yield return (null, new RuleExpressions(
                    new Atom("check1"), 
                    new []
                    {
                        new Atom("right", new Variable("0"), new Variable("1")),
                        new Atom("resource", new Variable("0")),
                        new Atom("operation", new Variable("1")),
                    }, 
                    Enumerable.Empty<biscuit_net.Proto.ExpressionV2>()
                ));
            }
            else if(line.StartsWith("check if must_be_present($0) or must_be_present($0)"))
            {
                yield return (null, new RuleExpressions(
                    new Atom("check1"), 
                    new []
                    {
                        new Atom("must_be_present", new Variable("0")),
                        new Atom("must_be_present", new Variable("0"))
                    }, 
                    Enumerable.Empty<biscuit_net.Proto.ExpressionV2>()
                ));
            }
            else if(line.StartsWith("check if read(0), write(1), resource(2), operation(3), right(4), time(5), role(6), owner(7), tenant(8), namespace(9), user(10), team(11), service(12), admin(13), email(14), group(15), member(16), ip_address(17), client(18), client_ip(19), domain(20), path(21), version(22), cluster(23), node(24), hostname(25), nonce(26), query(27)"))
            {
                var tokens = line.Split(", ");
                tokens[0] = tokens[0].Remove(0, "check if ".Length);

                var atoms = new List<Atom>();
                foreach(var token in tokens)
                {
                    var match = intTermRegex.Match(token.Trim(';'));
                    var name = match.Groups[1];
                    var intValueString = match.Groups[2];
                    
                    Assert.True(int.TryParse(intValueString.Value, out var intValue));
                    atoms.Add(new Atom(name.Value, new Integer(intValue)));
                }

                yield return (null, new RuleExpressions(
                    new Atom("check1"), 
                    atoms,
                    Enumerable.Empty<biscuit_net.Proto.ExpressionV2>()
                ));
            }


            
            else throw new NotSupportedException(line);
        }
    }
}