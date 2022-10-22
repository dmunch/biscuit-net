using System.Reflection;
using Xunit.Sdk;

namespace tests;
using Json.Samples;

public record BiscuitCase(byte[] Token, string Filename, string Title, File1 Validation)
{
    public override string ToString()
    {
        return $"{Filename}: {Title}";
    }
}

public class BiscuitCases : DataAttribute
{
    private readonly string? _fileName;
    private static Json.Samples.Sample _samples;
    private CaseType _caseType;

    public BiscuitCases(string fileName)
    {
        _fileName = fileName;
    }

    public enum CaseType 
    {
        All,
        Success,
        ErrFormat,
        ErrFailedLogic
    }

    public BiscuitCases()
    {
        _caseType = CaseType.All;
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
        if(_fileName != null) 
        {
            var testCase = _samples.Testcases.Single(tc => tc.Filename == _fileName);

            return MapBiscuitCases(testCase, f => true)
                .Select(biscuitCase => new object [] {biscuitCase})
                .ToArray();
        }

        Func<File1, bool> filter = _caseType switch
        {
            CaseType.All => f => true,
            CaseType.ErrFailedLogic => f => f.Result?.Err?.FailedLogic != null,
            CaseType.ErrFormat => f => f.Result?.Err?.Format != null,
            CaseType.Success => f => f.Result?.Ok != null,
            _ => throw new Exception("Not supported")
        };
        
        return _samples.Testcases
            .SelectMany(tc => MapBiscuitCases(tc, filter))
            .Select(biscuitCase => new object [] {biscuitCase})
            .ToArray();
    }

    public IEnumerable<BiscuitCase> MapBiscuitCases(Testcase testCase, Func<File1, bool> filter)
    {
        var token =  System.IO.File.ReadAllBytes($"samples-v2/{testCase.Filename}");
        return testCase.Validations
            .Where(kvp => filter(kvp.Value))
            .Select(validation => new BiscuitCase(
                Filename: testCase.Filename,
                Title: $"{testCase.Title}: {validation.Key}",
                Token: token,
                Validation: validation.Value
            )).ToArray();
    }
}
