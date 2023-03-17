
# biscuit-net

biscuit-net is an implementation of [Biscuit](https://github.com/biscuit-auth/biscuit) in .NET/C#. It aims to be fully compatible with other existing implementations, so that tokens issued by, for example, the Rust version, could be validated by this library and vice versa.

## Documentation and specifications

- [biscuit website](https://www.biscuitsec.org) for documentation and examples
- [biscuit specification](https://github.com/biscuit-auth/biscuit)
- [biscuit-rust](https://github.com/biscuit-auth/biscuit-rust) for some more technical details.


## Biscuit introduction 

Taken from [biscuit-rust](https://github.com/biscuit-auth/biscuit-rust): 

Biscuit is an authorization token for microservices architectures with the following properties:

- decentralized validation: any node could validate the token only with public information;
- offline delegation: a new, valid token can be created from another one by attenuating its rights, by its holder, without communicating with anyone;
- capabilities based: authorization in microservices should be tied to rights related to the request, instead of relying to an identity that might not make sense to the verifier;
- flexible rights managements: the token uses a logic language to specify attenuation and add bounds on ambient data;
- small enough to fit anywhere (cookies, etc).

Non goals:

- This is not a new authentication protocol. Biscuit tokens can be used as opaque tokens delivered by other systems such as OAuth.
- Revocation: while tokens come with expiration dates, revocation requires external state management.


## Basic Usage

These usage samples and additinal ones, as well es required usings can be found in  [BiscuitBuilderTests.cs](tests/tests/BiscuitBuilderTests.cs).

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

You can learn more about third-party blocks in the relevant section of the Biscuit [specifiction](https://github.com/biscuit-auth/biscuit/blob/master/SPECIFICATIONS.md#appending-a-third-party-block), or in this [blog Post.](https://www.biscuitsec.org/blog/third-party-blocks-why-how-when-who/)

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

## Project Structure

The most interesting bits of this implementation are:
- [Datalog Interpreter](src/lib/Datalog)
- [ANTLR](https://www.antlr.org/) based parser [grammar](src/parser/Datalog.g4)
- [Ed2559](src/lib/Ed25519.cs) cryptography, using [NSec/libsodium](https://nsec.rocks/)

## Test coverage

The implementation currently passes all the tests of the conformance [test suite](https://github.com/biscuit-auth/biscuit/tree/master/samples/current) published as part of the [Biscuit specification](https://github.com/biscuit-auth/biscuit/blob/master/SPECIFICATIONS.md).

There are also additional unit-like tests, however those could be a more complete. Feel free to chip in.

## NuGet Prereleases

NuGet preleases are currently available on Github Packages. To consume them, your project needs to configure an additional NuGet source like follows.

```sh
# create a new project-specif nuget.config, if you don't already have one
dotnet new nugetconfig  
# add the GitHub nuget source 
dotnet nuget add source https://nuget.pkg.github.com/dmunch/index.json   
# install biscuit_net.Parser, which pulls in the other packages as dependies and is required for the examples to work 
dotnet add package biscuit_net.Parser
```

## A word of caution: hic sunt dracones

This code should NOT be used in production scenarios before further scrutinizing and reviewing happend. You have been warned. 

## License

Licensed under [Apache License, Version 2.0](./LICENSE).