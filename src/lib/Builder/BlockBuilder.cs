using System.Buffers;
using biscuit_net.Datalog;
using ProtoBuf;

namespace biscuit_net.Builder;


public interface IBlockSigner
{
    Proto.SignedBlock Sign(SymbolTable globalSymbols, KeyTable globalKeys, PublicKey nextKey, ISigningKey key);
}

public class ThirdPartyBlockSigner: IBlockSigner
{
    readonly ThirdPartyBlock _thirdPartyBlock;      

    public ThirdPartyBlockSigner(ThirdPartyBlock thirdPartyBlock)
    {
        _thirdPartyBlock = thirdPartyBlock;
    }

    public Proto.SignedBlock Sign(SymbolTable globalSymbols, KeyTable globalKeys, PublicKey nextKey, ISigningKey key)
    {
        var signedBlock = new Proto.SignedBlock
        {
            Block = _thirdPartyBlock.Bytes,
            externalSignature = new Proto.ExternalSignature
            {
                Signature = _thirdPartyBlock.Signature,
                publicKey = ProtoConverters.ToPublicKey(_thirdPartyBlock.PublicKey)
            },
            nextKey = ProtoConverters.ToPublicKey(nextKey)
        };

        var buffer = SignatureHelper.MakeBuffer(signedBlock.Block, signedBlock.externalSignature.Signature, signedBlock.nextKey);
        signedBlock.Signature = key.Sign(new ReadOnlySpan<byte>(buffer));

        return signedBlock;
    }
}

public class BlockBuilder : IBlockSigner
{
    public List<Fact> Facts { get; } = new List<Fact>();
    public List<RuleScoped> Rules { get; } = new List<RuleScoped>();
    public List<Check> Checks { get; } = new List<Check>();

    public List<ScopeType> ScopeTypes { get; } = new List<ScopeType>() { ScopeType.Authority } ;
    public List<PublicKey> TrustedKeys { get; } = new List<PublicKey>();

    readonly IBiscuitBuilder _topLevelBuilder;
    
    public BlockBuilder(IBiscuitBuilder topLevelBuilder)
    {
        _topLevelBuilder = topLevelBuilder;
    }

    public BlockBuilder Add(Fact fact) { Facts.Add(fact); return this; }
    public BlockBuilder Add(RuleScoped rule) { Rules.Add(rule); return this; }
    public BlockBuilder Add(Check check) { Checks.Add(check); return this; } 

    public BlockBuilder Trusts(ScopeType scopeType) { ScopeTypes.Add(scopeType); return this; }
    public BlockBuilder Trusts(PublicKey publicKey) { TrustedKeys.Add(publicKey); return this; }
    
    public IBiscuitBuilder EndBlock() => _topLevelBuilder;

    Proto.Block ToProto(SymbolTable globalSymbols, KeyTable globalKeys)
    {
        var blockV2 = new Proto.Block();

        var symbols = globalSymbols;
        var symbolsBefore = symbols.Symbols.ToList(); //deep copy 
        var keys = globalKeys;
        var keysBefore = keys.Keys.ToList(); //deep copy 

        blockV2.Version = 3;
        blockV2.FactsV2s.AddRange(ProtoConverters.ToFactsV2(Facts, symbols));
        blockV2.RulesV2s.AddRange(ProtoConverters.ToRulesV2(Rules, symbols, keys));
        blockV2.ChecksV2s.AddRange(ProtoConverters.ToChecksV2(Checks, symbols, keys));
        
        
        blockV2.Scopes.AddRange(ProtoConverters.ToScopes(ScopeTypes));
        blockV2.Scopes.AddRange(ProtoConverters.ToScopes(TrustedKeys, keys));

        blockV2.Symbols.AddRange(symbols.Symbols.Except(symbolsBefore)); //add symbol delta, not all symbols    
        blockV2.publicKeys.AddRange(keys.Keys.Except(keysBefore).Select(key => ProtoConverters.ToPublicKey(key))); //add key delta, not all keys
        
        return blockV2;
    }

    public Proto.SignedBlock Sign(SymbolTable globalSymbols, KeyTable globalKeys, PublicKey nextKey, ISigningKey key)
    {
        var signedBlock = new Proto.SignedBlock();

        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, ToProto(globalSymbols, globalKeys));
        
        signedBlock.Block = bufferWriter.WrittenMemory.ToArray();
        signedBlock.nextKey = ProtoConverters.ToPublicKey(nextKey);
        
        var buffer = SignatureHelper.MakeBuffer(signedBlock.Block, signedBlock.nextKey);
        signedBlock.Signature = key.Sign(new ReadOnlySpan<byte>(buffer));
        
        return signedBlock;    
    }
}