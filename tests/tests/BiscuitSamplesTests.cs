using biscuit_net;
using biscuit_net.Parser;

namespace tests;
public class BiscuitSamplesTests
{
    [Theory]
    //[BiscuitFileSample("test001_basic.bc")] //OK
    //[BiscuitFileSample("test002_different_root_key.bc")] //OK
    //[BiscuitFileSample("test003_invalid_signature_format.bc")] //OK
    //[BiscuitFileSample("test007_scoped_rules.bc")] //OK
    //[BiscuitFileSample("test008_scoped_checks.bc")] //OK
    //[BiscuitFileSample("test009_expired_token.bc")] //OK - why? TODO needs expressions
    //[BiscuitFileSample("test010_authorizer_scope.bc")] //TODO: rules in authorizer
    //[BiscuitFileSample("test011_authorizer_authority_caveats.bc")] //TODO: rules in authorizer
    //[BiscuitFileSample("test012_authority_caveats.bc")] //OK
    //[BiscuitFileSample("test013_block_rules.bc")] //TODO contains time and string set expressions
    //[BiscuitFileSample("test014_regex_constraint.bc")] //TODO contains string expressions regex
    //[BiscuitFileSample("test016_caveat_head_name.bc")] //OK
    //[BiscuitFileSample("test017_expressions.bc")] //OK
    //[BiscuitFileSample("test018_unbound_variables_in_rule.bc")] //TODO
    //[BiscuitFileSample("test019_generating_ambient_from_variables.bc")] //OK
    //[BiscuitFileSample("test022_default_symbols.bc")] //TODO contains int term
    //[BiscuitFileSample("test023_execution_scope.bc")] //TODO contains int term
    //[BiscuitFileSample("test024_third_party.bc")] //TODO 
    //[BiscuitFileSample("test025_check_all.bc")] //TODO
    //[BiscuitFileSample("test026_public_keys_interning.bc")] //TODO
    //[BiscuitFileSample("test027_integer_wraparound.bc")] //TODO
    [BiscuitSamples()]
    public void TestSuccessAndError(BiscuitSample biscuitCase)
    {
        var verificationKey = new Ed25519.VerificationKey(Convert.FromHexString(biscuitCase.RootPublicKey));
        if(!Biscuit.TryDeserialize(biscuitCase.Token, verificationKey, out var biscuit, out var formatErr))
        {
            Assert.False(biscuitCase.Success);
            Assert.Equal(biscuitCase.Validation.FormatError, formatErr);
            return;
        }

        var authorizer = Parser.Authorizer(biscuitCase.Validation.AuthorizerCode);
        
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
    public void TestWorldFacts(BiscuitSample biscuitCase)
    {
        var verificationKey = new Ed25519.VerificationKey(Convert.FromHexString(biscuitCase.RootPublicKey));
        if (!Biscuit.TryDeserialize(biscuitCase.Token, verificationKey, out var biscuit, out _))
        {
            //ignore
            return;
        }
        
        var authorizer = Parser.Authorizer(biscuitCase.Validation.AuthorizerCode);
        authorizer.TryAuthorize(biscuit, out _);

        var aFacts = new HashSet<biscuit_net.Datalog.Fact>(authorizer.World.Facts);
        Assert.Equal(biscuitCase.Validation.WorldFacts, aFacts);
    }

    [Theory]
    [BiscuitSamples()]
    public void Test_Revocation_Ids(BiscuitSample biscuitCase)
    {
        var verificationKey = new Ed25519.VerificationKey(Convert.FromHexString(biscuitCase.RootPublicKey));
        if (!Biscuit.TryDeserialize(biscuitCase.Token, verificationKey, out var biscuit, out _))
        {
            return;
        }
        Assert.Equal(biscuitCase.Validation.RevocationIds, biscuit.RevocationIds);
    }
}