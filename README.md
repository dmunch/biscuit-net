
# biscuit-net

biscuit-net is an implementation of [Biscuit](https://github.com/biscuit-auth/biscuit) in .NET/C#. It aims to be fully compatible with other existing implementations, so that tokens issued by, for example, the Rust version, could be validated by this library and vice versa.

## Documentation and specifications

- [biscuit website](https://www.biscuitsec.org) for documentation and examples
- [biscuit specification](https://github.com/biscuit-auth/biscuit)
- [biscuit-rust](https://github.com/biscuit-auth/biscuit-rust) for some more technical details.

## Usage

#### Create a biscuit
```csharp

var rootKey = new SigningKey();        
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

#### Attenuate a biscuit
```csharp
var attenuatedToken = Biscuit.Attenuate(token)
    .AddBlock()
        .Add("""check if resource("file4")""")
        .Add("""check if resource("file5")""")
    .EndBlock()
    .Serialize();
// attenuatedToken is now a byte[] attenuation of the original token, and ready to be shared
```

#### Verify a biscuit
```csharp
var verificationKey = new VerificationKey(rootKey.Public);
if(!Biscuit.TryDeserialize(token, verificationKey, out var biscuit, out var formatErr))
{
    throw new Exception($"Couldn't deserialize/validate biscuit: {formatErr}");
}

if(!Parser.Authorizer("""resource("file5"); allow if true;""").TryAuthorize(biscuit, out err))
{
    throw new Exception($"Couldn't authorize biscuit: {err}");
}
```

## License

Licensed under [Apache License, Version 2.0](./LICENSE).