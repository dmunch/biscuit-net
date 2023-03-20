using System.Buffers;
using biscuit_net.Datalog;
using ProtoBuf;

namespace biscuit_net.Builder;

public record ThirdPartyBlockRequest(PublicKey PreviousKey, IEnumerable<PublicKey> PublicKeys);

public class ThirdPartyBlockBuilder
{
    public List<Fact> Facts { get; } = new List<Fact>();
    public List<RuleScoped> Rules { get; } = new List<RuleScoped>();
    public List<Check> Checks { get; } = new List<Check>();

    public ThirdPartyBlockBuilder Add(Fact fact) { Facts.Add(fact); return this; }
    public ThirdPartyBlockBuilder Add(RuleScoped rule) { Rules.Add(rule); return this; }
    public ThirdPartyBlockBuilder Add(Check check) { Checks.Add(check); return this; } 

    Proto.Block ToProto(IEnumerable<PublicKey> previousKeys)
    {
        var blockV2 = new Proto.Block();
        
        var symbols = new SymbolTable();
        var keys = new KeyTable(previousKeys);

        blockV2.FactsV2s.AddRange(ProtoConverters.ToFactsV2(Facts, symbols));
        blockV2.RulesV2s.AddRange(ProtoConverters.ToRulesV2(Rules, symbols, keys));
        blockV2.ChecksV2s.AddRange(ProtoConverters.ToChecksV2(Checks, symbols, keys));
        
        blockV2.Symbols.AddRange(symbols.Symbols);

        blockV2.Version = 3;

        blockV2.Scopes.Add(new Proto.Scope() { scopeType = Proto.Scope.ScopeType.Authority });

        return blockV2;
    }

    public ThirdPartyBlock Sign(ISigningKey key, ThirdPartyBlockRequest thirdPartyBlockRequest)
    {        
        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, ToProto(thirdPartyBlockRequest.PublicKeys));
        
        var payload = bufferWriter.WrittenMemory.ToArray();
        
        var buffer = SignatureHelper.MakeBuffer(payload, ProtoConverters.ToPublicKey(thirdPartyBlockRequest.PreviousKey));
        var signature = key.Sign(new ReadOnlySpan<byte>(buffer));
        
        return new ThirdPartyBlock(payload, signature, key.Public);    
    }
}
