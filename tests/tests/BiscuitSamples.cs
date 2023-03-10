using System.Reflection;
using Xunit.Sdk;
using biscuit_net;
using biscuit_net.Datalog;
using biscuit_net.Parser;

namespace tests;

public record Asserts(string AuthorizerCode, Error? Error, FailedFormat? FormatError, IList<string> RevocationIds, HashSet<Fact> WorldFacts);
public record BiscuitSample(string Filename, string Title, string RootPublicKey, string RootPrivateKey, Asserts Validation)
{
    public bool Success => Validation.Error == null && Validation.FormatError == null;
    public byte[] Token => System.IO.File.ReadAllBytes($"samples/{Filename}");

    public override string ToString()
    {
        return $"{Filename}: {Title} [{(Success? "Success" : "Error")}]";
    }
}

public class BiscuitFileSample : BiscuitSamples
{
    public BiscuitFileSample(string fileName) : base(tc => tc.Filename == fileName)
    {        
    }
}

public class BiscuitSamples : DataAttribute
{
    Func<QuickType.Testcase, bool> _filter;
    
    private static QuickType.Samples? _samples;
    
    public BiscuitSamples()
    {
        //by default, include all
        _filter = s => true;
    }

    public BiscuitSamples(Func<QuickType.Testcase, bool> filter)
    {
        _filter = filter;
    }
    
    static BiscuitSamples()
    {
        var samplesJson =  System.IO.File.ReadAllText("samples/samples.json");
        _samples = Newtonsoft.Json.JsonConvert.DeserializeObject<QuickType.Samples>(samplesJson);
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        if(_samples == null)
        {
            throw new Exception("Error loading samples");
        }

        var testCases = _samples.Testcases.Where(_filter).ToArray();
        
        return testCases
            .SelectMany(tc => MapBiscuitCases(_samples, tc))
            .OrderBy(c => int.Parse(c.Filename.Split('_')[0].Substring("test".Length)))
            .Select(biscuitCase => new object [] {biscuitCase})
            .ToArray();
    }

    public IEnumerable<BiscuitSample> MapBiscuitCases(QuickType.Samples samples, QuickType.Testcase testCase)
    {
        return testCase.Validations
            .Select(validation => new BiscuitSample(
                Filename: testCase.Filename,
                Title: $"{testCase.Title}: {validation.Key}",
                Validation: MapValidation(validation.Value),
                RootPrivateKey: samples.RootPrivateKey,
                RootPublicKey: samples.RootPublicKey
            )).ToArray();
    }

    static Asserts MapValidation(QuickType.File file)
    {
        FailedFormat? failedFormat = null;        
        Error? error = null;

        var parser = new Parser();
        var facts = new HashSet<Fact>();
        foreach(var factStr in file.World?.Facts ?? new string[0])
        {
            var fact = parser.ParseFact(factStr);
            facts.Add(fact);
        }
        
        if(file.Result?.Err?.FailedLogic?.Unauthorized != null)
        {
            var failedLogic = file.Result.Err.FailedLogic.Unauthorized;
            var authorizer = failedLogic.Checks[0].Authorizer;
            var block = failedLogic.Checks[0].Block;

            if(authorizer != null)
                error = new Error(new FailedAuthorizerCheck(authorizer.CheckId/*, null*/));
            else if(block != null)
                error = new Error(new FailedBlockCheck(block.BlockId, block.CheckId/*, null*/));
        }

        if(file.Result?.Err?.FailedLogic?.InvalidBlockRule != null)
        {
            var ibr = file.Result.Err.FailedLogic.InvalidBlockRule;
            var ruleId = ibr[0].Integer; //assuming this is ruleId - not clear in the specs
            var rule = ibr[1].String;
            
            error = new Error(new FailedLogic(new InvalidBlockRule((int)ruleId/*, rule*/)));
        }


        if(file.Result?.Err?.Format != null)
        {
            var iss = file.Result?.Err?.Format.InvalidSignatureSize;
            var invalidSignature = file.Result?.Err?.Format?.Signature?.InvalidSignature;
            var signature = invalidSignature != null ? new biscuit_net.Signature(invalidSignature) : null;

            failedFormat = new FailedFormat(signature, (int?)iss);
        }

        if(file.Result?.Err?.Execution != null)
        {
            var reason = file.Result.Err.Execution;
            error = new Error(new FailedExecution(reason));
        }
        
        return new Asserts(file.AuthorizerCode, error, failedFormat, file.RevocationIds, facts);
    }
}
