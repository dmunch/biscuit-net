using ProtoBuf;
using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;

public class Biscuit
{
    public Block Authority { get; private set; }
    public IReadOnlyList<Block> Blocks { get; protected set; }
    public IReadOnlyList<string> RevocationIds { get; private set; }
        
    Biscuit(Block authority, IReadOnlyList<Block> blocks, IReadOnlyList<string> revocationIds)
    {
        Authority = authority;
        
        Blocks = blocks;
        RevocationIds = revocationIds;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> bytes, SignatureValidator validator, [NotNullWhen(true)] out Biscuit? biscuit, [NotNullWhen(false)] out FailedFormat? err)
    {        
        var biscuitProto = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);

        if(!biscuitProto.VerifySignatures(validator, out err))
        {
            biscuit = null; return false; 
        }

        var symbols = new SymbolTable();
        var keys = new KeyTable();
        var blocks = new Block[biscuitProto.Blocks.Count];
        var revocationIds = new string[biscuitProto.Blocks.Count + 1];

        var authority = FromProto(biscuitProto.Authority, symbols, keys);        
        revocationIds[0] = GetRevocationId(biscuitProto.Authority);

        for(var blockIdx = 0; blockIdx < biscuitProto.Blocks.Count; blockIdx++)
        {
            var block = biscuitProto.Blocks[blockIdx];
            var blockSymbolTable = block.externalSignature != null ? new SymbolTable() : symbols;
            
            blocks[blockIdx] = FromProto(block, blockSymbolTable, keys);
            revocationIds[blockIdx + 1] = GetRevocationId(block);
        }
        
        biscuit = new Biscuit(authority, blocks.AsReadOnly(), revocationIds.AsReadOnly());

        err = null; return true;
    }

    public static string GetRevocationId(Proto.SignedBlock signedBlock) 
        => Convert.ToHexString(signedBlock.Signature).ToLowerInvariant();

    public static Block FromProto(Proto.SignedBlock signedBlock, SymbolTable symbols, KeyTable keys)
    {
        var block = Serializer.Deserialize<Proto.Block>( (ReadOnlySpan<byte>) signedBlock.Block);
        
        symbols.AddSymbols(block.Symbols);
        keys.Add(block.publicKeys.Select(Converters.ToPublicKey));
        
        var scope = Converters.ToScope(block.Scopes, keys);
        scope = scope.IsEmpty ? Scope.DefaultBlockScope : scope; 

        return new Block(
            block.FactsV2s.ToFacts(symbols),
            block.RulesV2s.ToRules(symbols, keys),
            block.ChecksV2s.ToChecks(symbols, keys),
            block.Version,            
            scope,
            signedBlock.externalSignature != null ? Converters.ToPublicKey(signedBlock.externalSignature.publicKey) : null
        );
    }
}