using ProtoBuf;
using VeryNaiveDatalog;
using parser;
using System.Buffers.Binary;
using NSec.Cryptography;

namespace biscuit_net;

//TODO Assuming the int is a RuleId - specification and examples are unclear here
public record InvalidBlockRule(int RuleId/*, RuleExpressions Rule*/);

public record FailedFormat(Signature? Signature, int? InvalidSignatureSize);
public record Signature(string InvalidSignature);

public class SignatureValidator
{
    SignatureAlgorithm _algorithm;
    PublicKey _key;
    
    public SignatureValidator(string publicKeyInHex) : this(Convert.FromHexString(publicKeyInHex))
    {
    }
    
    public SignatureValidator(byte[] publicKey)
    {
        _algorithm = SignatureAlgorithm.Ed25519;
        _key = PublicKey.Import(_algorithm, publicKey, KeyBlobFormat.RawPublicKey);
    }

    public bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        => _algorithm.Verify(_key, data, signature);

}

public class Block
{
    public IEnumerable<Atom> Atoms { get; protected set; }
    public IEnumerable<RuleExpressions> Rules { get; protected set; }
    public IEnumerable<Check> Checks { get; protected set; }

    public List<string> Symbols { get; private set; }

    public Block(Proto.Block block, List<string> symbols)
    {
        Atoms = block.FactsV2s.ToAtoms(symbols);
        Rules = block.RulesV2s.ToRules(symbols);
        Checks = block.ChecksV2s.ToChecks(symbols);

        Symbols = symbols;
    }
}

static class BlockSignatureVerification
{
    public static bool VerifySignature(this Proto.SignedBlock signedBlock, SignatureValidator validator, out int? invalidSignatureSize)
    {
        if(signedBlock.Signature.Length != 64)
        {
            invalidSignatureSize = signedBlock.Signature.Length;
            return false;
        }
        invalidSignatureSize = null;

        //IMPROVE: could use an array pool here
        var buffer = new byte[signedBlock.Block.Length + sizeof(int) + signedBlock.nextKey.Key.Length];
        var bytes = (Span<byte>) buffer;        

        signedBlock.Block.CopyTo(buffer, 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(signedBlock.Block.Length, sizeof(int)), (int)signedBlock.nextKey.algorithm);
        signedBlock.nextKey.Key.CopyTo(buffer, signedBlock.Block.Length + 4);

        return validator.Verify(buffer, signedBlock.Signature);
    }
}


public class Biscuit
{
    Proto.Biscuit _biscuit;
    public Block Authority { get; private set; }

    public List<string> Symbols { get; protected set; }= new List<string>();
    
    Biscuit(Proto.Biscuit biscuit, Block authority)
    {
        _biscuit = biscuit;
        Authority = authority;

        Symbols.AddRange(authority.Symbols.ToList());
        Blocks = LoadBlocks();
    }
    public IEnumerable<Block> Blocks { get; protected set; }

    IEnumerable<Block> LoadBlocks() 
    {
        foreach(var block in _biscuit.Blocks)
        {
            var blockBytes = (ReadOnlySpan<byte>) block.Block;
            var blockProto = Serializer.Deserialize<Proto.Block>(blockBytes);
            
            Symbols.AddRange(blockProto.Symbols);

            yield return new Block(blockProto, Symbols);
        }
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> bytes, SignatureValidator validator, out Biscuit? biscuit, out FailedFormat? err)
    {        
        var biscuitProto = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);

        if(!VerifySignatures(validator, biscuitProto, out var invalidSignatureSize))
        {
            biscuit = null;
            err = invalidSignatureSize != null 
                ? new FailedFormat(null, invalidSignatureSize)
                : new FailedFormat(new Signature("signature error: Verification equation was not satisfied"), null);
            return false; 
        }

        var authorityProto = Serializer.Deserialize<Proto.Block>((ReadOnlySpan<byte>)biscuitProto.Authority.Block);
        var authority = new Block(authorityProto, authorityProto.Symbols);

        biscuit = new Biscuit(biscuitProto, authority);
        err = null;
        return true;
    }

    
    static bool VerifySignatures(SignatureValidator validator, Proto.Biscuit biscuitProto, out int? invalidSignatureSize)
    {
        if(!biscuitProto.Authority.VerifySignature(validator, out invalidSignatureSize))
        {
            return false;
        }

        var nextValidator = new SignatureValidator(biscuitProto.Authority.nextKey.Key);
        foreach(var block in biscuitProto.Blocks)
        {
            if(!block.VerifySignature(nextValidator, out invalidSignatureSize))
            {
                return false;
            }
            nextValidator = new SignatureValidator(block.nextKey.Key);
        }

        return true;
    }

    public bool CheckBoundVariables(out InvalidBlockRule invalidBlockRule)
    {
        if(!CheckBoundVariables(Authority, out invalidBlockRule))
        {
            return false;
        }

        foreach(var block in Blocks)
        {
            if(!CheckBoundVariables(block, out invalidBlockRule))
            {
                return false;
            }
        }

        invalidBlockRule = null;
        return true;
    }

    bool CheckBoundVariables(Block block, out InvalidBlockRule invalidBlockRule)
    {
        int ruleId = 0;
        foreach(var rule in block.Rules)
        {
            var headVariables = rule.Head.Terms.OfType<Variable>();
            var bodyVariables = rule.Body.SelectMany(b => b.Terms).OfType<Variable>().ToHashSet();
            
            if(!headVariables.All(hv => bodyVariables.Contains(hv)))
            {
                invalidBlockRule = new InvalidBlockRule(ruleId);
                return false;
            }
            ruleId++;
        }

        invalidBlockRule = null;
        return true;
    }
}
