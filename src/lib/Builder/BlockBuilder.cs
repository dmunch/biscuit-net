using System.Buffers;
using biscuit_net.Datalog;
using ProtoBuf;

namespace biscuit_net.Builder;


public interface IBlockSigner
{
    Proto.SignedBlock Sign(SymbolTable globalSymbols, PublicKey nextKey, ISigningKey signer);
}

public class ThirdPartyBlockSigner: IBlockSigner
{
    ThirdPartyBlock _thirdPartyBlock;      

    public ThirdPartyBlockSigner(ThirdPartyBlock thirdPartyBlock)
    {
        _thirdPartyBlock = thirdPartyBlock;
    }

    public Proto.SignedBlock Sign(SymbolTable globalSymbols, PublicKey nextKey, ISigningKey key)
    {
        var signedBlock = new Proto.SignedBlock();

        signedBlock.Block = _thirdPartyBlock.Bytes;
        signedBlock.externalSignature = new Proto.ExternalSignature();
        signedBlock.externalSignature.Signature = _thirdPartyBlock.Signature;
        signedBlock.externalSignature.publicKey = ProtoConverters.ToPublicKey(_thirdPartyBlock.PublicKey);
        signedBlock.nextKey = ProtoConverters.ToPublicKey(nextKey);
        
        var buffer = SignatureHelper.MakeBuffer(signedBlock.Block, signedBlock.externalSignature.Signature, signedBlock.nextKey.algorithm, signedBlock.nextKey.Key);
        signedBlock.Signature = key.Sign(new ReadOnlySpan<byte>(buffer));

        return signedBlock;
    }
}

public class BlockBuilder : IBlockSigner
{
    public List<Fact> Facts { get; } = new List<Fact>();
    public List<Rule> Rules { get; } = new List<Rule>();
    public List<Check> Checks { get; } = new List<Check>();

    IBiscuitBuilder _topLevelBuilder;
    
    public BlockBuilder(IBiscuitBuilder topLevelBuilder)
    {
        _topLevelBuilder = topLevelBuilder;
    }

    public BlockBuilder Add(Fact fact) { Facts.Add(fact); return this; }
    public BlockBuilder Add(Rule rule) { Rules.Add(rule); return this; }
    public BlockBuilder Add(Check check) { Checks.Add(check); return this; } 

    public IBiscuitBuilder EndBlock() => _topLevelBuilder;

    Proto.Block ToProto(SymbolTable globalSymbols)
    {
        var blockV2 = new Proto.Block();

        var symbols = globalSymbols;
        var symbolsBefore = symbols.Symbols.ToList(); //deep copy 

        blockV2.FactsV2s.AddRange(ProtoConverters.ToFactsV2(Facts, symbols));
        blockV2.RulesV2s.AddRange(ProtoConverters.ToRulesV2(Rules, symbols));
        blockV2.ChecksV2s.AddRange(ProtoConverters.ToChecksV2(Checks, symbols));
        
        blockV2.Symbols.AddRange(symbols.Symbols.Except(symbolsBefore)); //add symbol delta, not all symbols

        blockV2.Version = 3;

        blockV2.Scopes.Add(new Proto.Scope() { scopeType = Proto.Scope.ScopeType.Authority });

        return blockV2;
    }

    public Proto.SignedBlock Sign(SymbolTable globalSymbols, PublicKey nextKey, ISigningKey key)
    {
        var signedBlock = new Proto.SignedBlock();

        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, ToProto(globalSymbols));
        
        signedBlock.Block = bufferWriter.WrittenMemory.ToArray();
        signedBlock.nextKey = ProtoConverters.ToPublicKey(nextKey);
        
        var buffer = SignatureHelper.MakeBuffer(signedBlock.Block, signedBlock.nextKey.algorithm, signedBlock.nextKey.Key);
        signedBlock.Signature = key.Sign(new ReadOnlySpan<byte>(buffer));
        
        return signedBlock;    
    }
}