using System.Reflection;
using Xunit.Sdk;

namespace tests;

using biscuit_net;
using biscuit_net.Datalog;
using Json.Samples;

public record Asserts(string AuthorizerCode, Error? Error, FailedFormat? FormatError, IList<string> RevocationIds);
public record BiscuitCase(string Filename, string Title, string RootPublicKey, string RootPrivateKey, Asserts Validation)
{
    public bool Success => Validation.Error == null && Validation.FormatError == null;
    public byte[] Token => System.IO.File.ReadAllBytes($"samples-v2/{Filename}");
    public override string ToString()
    {
        return $"{Filename}: {Title} [{(Success? "Success" : "Error")}]";
    }
}

public class BiscuitCases : DataAttribute
{
    private readonly string? _fileName;
    private static Json.Samples.Sample? _samples;
    
    public BiscuitCases(string fileName)
    {
        _fileName = fileName;
    }

    public BiscuitCases()
    {
    }
    
    static BiscuitCases()
    {
        var samplesJson =  System.IO.File.ReadAllText("samples-v2/samples.json");
        _samples = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Samples.Sample>(samplesJson);
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        if(_samples == null)
        {
            throw new Exception("Error loading samples");
        }

        var testCases = _samples.Testcases;
        if(_fileName != null) 
        {
           testCases = _samples.Testcases.Where(tc => tc.Filename == _fileName).ToList();
        }

        return testCases
            .SelectMany(tc => MapBiscuitCases(_samples, tc))
            .OrderBy(c => int.Parse(c.Filename.Split('_')[0].Substring("test".Length)))
            .Select(biscuitCase => new object [] {biscuitCase})
            .ToArray();
    }

    public IEnumerable<BiscuitCase> MapBiscuitCases(Json.Samples.Sample samples, Testcase testCase)
    {
        return testCase.Validations
            .Select(validation => new BiscuitCase(
                Filename: testCase.Filename,
                Title: $"{testCase.Title}: {validation.Key}",
                Validation: MapValidation(validation.Value),
                RootPrivateKey: samples.Root_private_key,
                RootPublicKey: samples.Root_public_key
            )).ToArray();
    }

    Asserts MapValidation(File1 file)
    {
        FailedFormat? failedFormat = null;
        Error? error = null;

        if(file.Result?.Err?.FailedLogic?.Unauthorized != null)
        {
            var failedLogic = file.Result.Err.FailedLogic.Unauthorized;
            var authorizer = failedLogic.Checks[0].Authorizer;
            var block = failedLogic.Checks[0].Block;

            if(authorizer != null)
                error = new Error(new FailedAuthorizerCheck(authorizer.CheckId/*, null*/));
            else if(block != null)
                error = new Error(new FailedBlockCheck(block.Block_id, block.Check_id/*, null*/));
        }

        if(file.Result?.Err?.FailedLogic?.InvalidBlockRule != null)
        {
            var ibr = file.Result.Err.FailedLogic.InvalidBlockRule;
            var ruleId = (long)  ibr[0]; //assuming this is ruleId - not clear in the specs
            var rule = (string) ibr[1];
            
            error = new Error(new InvalidBlockRule((int)ruleId/*, rule*/));
        }


        if(file.Result?.Err?.Format != null)
        {
            var iss = file.Result?.Err?.Format.InvalidSignatureSize;
            var invalidSignature = file.Result?.Err?.Format?.Signature?.InvalidSignature;
            var signature = invalidSignature != null ? new biscuit_net.Signature(invalidSignature) : null;

            failedFormat = new FailedFormat(signature, iss);
        }
        
        return new Asserts(file.Authorizer_code, error, failedFormat, file.Revocation_ids);
    }
}