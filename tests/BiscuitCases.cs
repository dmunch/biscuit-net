using System.Reflection;
using Xunit.Sdk;

namespace tests;

using biscuit_net;
using Json.Samples;

public record Asserts(string AuthorizerCode, Error Error);
public record BiscuitCase(string Filename, string Title, Asserts Validation)
{
    public bool Success => Validation.Error == null;
    public byte[] Token => System.IO.File.ReadAllBytes($"samples-v2/{Filename}");
    public override string ToString()
    {
        return $"{Filename}: {Title} [{(Success? "Success" : "Error")}][{(!Success ? Validation.Error: null)}]";
    }
}

public class BiscuitCases : DataAttribute
{
    private readonly string? _fileName;
    private static Json.Samples.Sample _samples;
    private CaseType _caseType;

    public BiscuitCases(string fileName) : this()
    {
        _fileName = fileName;
    }

    [Flags]
    public enum CaseType 
    {
        All = 0,
        Success = 1,
        Error = 2,
        Format = 4,
        FailedLogic = 8
    }

    public BiscuitCases()
    {
    }

    public BiscuitCases(CaseType caseType)
    {
        _caseType = caseType;
    }
    
    static BiscuitCases()
    {
        var samplesJson =  System.IO.File.ReadAllText("samples-v2/samples.json");
        _samples = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Samples.Sample>(samplesJson);
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var errorFilters = new List<Func<File1, bool>>();
        var successFilters = new List<Func<File1, bool>>();
        if(_caseType.HasFlag(CaseType.Error))
        {
            errorFilters.Add(f => f.Result?.Err != null);
        }
        if(_caseType.HasFlag(CaseType.FailedLogic))
        {
            errorFilters.Add(f => f.Result?.Err?.FailedLogic != null);
        }
        if(_caseType.HasFlag(CaseType.Format))
        {
            errorFilters.Add(f => f.Result?.Err?.Format != null);
        }
        if(_caseType.HasFlag(CaseType.Success))
        {
            successFilters.Add(f => f.Result?.Ok != null);
        }


        var testCases = _samples.Testcases;
        if(_fileName != null) 
        {
           testCases = _samples.Testcases.Where(tc => tc.Filename == _fileName).ToList();
        }

        var errorCases = testCases
            .SelectMany(tc => MapBiscuitCases(tc, errorFilters))
            .ToArray();

        var successCases = testCases
            .SelectMany(tc => MapBiscuitCases(tc, successFilters))
            .ToArray();

        return errorCases
            .Union(successCases)
            .Distinct()
            .OrderBy(c => c.Filename)
            .Select(biscuitCase => new object [] {biscuitCase});
    }

    public IEnumerable<BiscuitCase> MapBiscuitCases(Testcase testCase, IEnumerable<Func<File1, bool>> filters)
    {
        return testCase.Validations
            .Where(kvp => filters.All(f => f(kvp.Value)))
            .Select(validation => new BiscuitCase(
                Filename: testCase.Filename,
                Title: $"{testCase.Title}: {validation.Key}",
                Validation: MapValidation(validation.Value)
            )).ToArray();
    }

    Asserts MapValidation(File1 file)
    {
        FailedAuthorizerCheck authorizerCheck = null;
        FailedBlockCheck blockCheck = null;
        InvalidBlockRule invalidBlockRule = null;

        if(file.Result?.Err?.FailedLogic?.Unauthorized != null)
        {
            var failedLogic = file.Result.Err.FailedLogic.Unauthorized;
            var authorizer = failedLogic.Checks[0].Authorizer;
            var block = failedLogic.Checks[0].Block;

            if(authorizer != null)
                authorizerCheck = new FailedAuthorizerCheck(authorizer.CheckId/*, null*/);
            if(block != null)
                blockCheck = new FailedBlockCheck(block.Block_id, block.Check_id/*, null*/);
        }

        if(file.Result?.Err?.FailedLogic?.InvalidBlockRule != null)
        {
            var ibr = file.Result.Err.FailedLogic.InvalidBlockRule;
            var ruleId = (long)  ibr[0]; //assuming this is ruleId - not clear in the specs
            var rule = (string) ibr[1];
            
            invalidBlockRule = new InvalidBlockRule((int)ruleId/*, rule*/);
        }

        var error = authorizerCheck != null || blockCheck != null || invalidBlockRule != null 
                    ? new Error(blockCheck, authorizerCheck, invalidBlockRule)
                    : null;

        return new Asserts(file.Authorizer_code, error);
    }
}
