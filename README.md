
# biscuit-net

biscuit-net is an implementation of [Biscuit](https://github.com/biscuit-auth/biscuit) in .NET/C#. It aims to be fully compatible with other existing implementations, so that tokens issued by, for example, the Rust version, could be validated by this library and vice versa.

## Documentation and specifications

- [biscuit website](https://www.biscuitsec.org) for documentation and examples
- [biscuit specification](https://github.com/biscuit-auth/biscuit)
- [biscuit-rust](https://github.com/biscuit-auth/biscuit-rust) for some more technical details.

## Basic Usage

### Create a biscuit
```csharp
var rootKey = Ed25519.NewSigningKey();
var token = Biscuit.New(rootKey)
    .AuthorityBlock("""
        right("/a/file1.txt", "read");
        right("/a/file1.txt", "write");
        right("/a/file2.txt", "read");
        right("/a/file2.txt", "write");
    """)
    .EndBlock()
.Serialize();

// token is now a byte[], ready to be shared
```

### Attenuate a biscuit
```csharp
var attenuatedToken = Biscuit.Attenuate(token)
    .AddBlock()
        .Add("""check if resource("file4")""")
        .Add("""check if resource("file5")""")
    .EndBlock()
    .Serialize();
// attenuatedToken is now a byte[] attenuation of the original token, and ready to be shared
```

### Verify a biscuit
```csharp
var verificationKey = new Ed25519.VerificationKey(rootKey.Public);
if(!Biscuit.TryDeserialize(token, verificationKey, out var biscuit, out var formatErr))
{
    throw new Exception($"Couldn't deserialize/validate biscuit: {formatErr}");
}

if(!Parser.Authorizer("""resource("file5"); allow if true;""").TryAuthorize(biscuit, out err))
{
    throw new Exception($"Couldn't authorize biscuit: {err}");
}
```

### Seal a biscuit

Sealing a biscuit means it can no longer be attenuated. 

```csharp
var rootKey = Ed25519.NewSigningKey();        
var token = Biscuit.New(rootKey)
    .AuthorityBlock()
        .Add("resource", "file4")
    .EndBlock()
    .Seal();


//this will throw an exception
Biscuit.Attenuate(token);
```

## Third-party blocks

You can learn more about third-party blocks in the relevant section of the Biscuit [specifiction](https://github.com/biscuit-auth/biscuit/blob/master/SPECIFICATIONS.md#appending-a-third-party-block)

### Adding a third-party block

```csharp
var rootKey = Ed25519.NewSigningKey();
var thirdPartyKey = Ed25519.NewSigningKey();

var verificationKey = new Ed25519.VerificationKey(rootKey.Public);        
        
var token1 = Biscuit.New(rootKey)
    .AuthorityBlock()
        .Add("resource", "file4")
    .EndBlock()
    .Serialize();

var token2 = Biscuit.Attenuate(token1)
    .AddThirdPartyBlock(request => 
        //the request would usually be send to a third-party over the wire
        //the third party processes the requests, builds a third-party block, signs
        //it, it sends it back.
        //for the sake of the example, everything here happens in-process
        Biscuit.NewThirdParty()
            .Add("""check if resource("file4")""")
            .Add("""check if resource("file5")""")
        .Sign(thirdPartyKey, request)
    )
    .Serialize();
// token2 is now a byte[], containing the third party block
```

### Trusting a third-party block issuer

```csharp
var rootKey = Ed25519.NewSigningKey();
var thirdPartyKey = Ed25519.NewSigningKey();

var token1 = Biscuit.New(rootKey)
    .AuthorityBlock()
        .Add("resource", "file4")                
    .EndBlock()
    .AddBlock()
        //this block trusts any blocks signed by the thirdPartyKey
        //even if these blocks have been appended only later-on 
        .Trusts(thirdPartyKey.Public)
        .Add("""check if resource("file5");""")
    .EndBlock()
    .AddBlock()                
        .AddCheck(Check.CheckKind.One)
            .AddRule("""resource("file5")""")
                //while the overall block only trusts authority and the authorizer
                //this rule also trusts blocks signed by the thirdPartyKey
                .Trusts(thirdPartyKey.Public)
            .EndRule()
        .EndCheck()
    .EndBlock()
    .Serialize();
```

## License

Licensed under [Apache License, Version 2.0](./LICENSE).