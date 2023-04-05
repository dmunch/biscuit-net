# ASP.NET Core Biscuit Authentication and Authorization Handlers

## Authentication and Authorization

ASP.NET Core Authentication and Authorization is happenign at two different times. 

## Authenticaiton

First, a request is authenticated - A token is checked for it's validity, and claims are extrated from the token to create a `ClaimsPrincipal`. 

As a first iteration, the biscuit bearer authentication handler allows to specify the name of the fact, which is used to extract claims. The fact which is used to generate claims needs to have two terms - the first one a `string`, which denotes the claim name, the second any datatype, which denotes the claim value. There's an automatic mapping from the second term's datatype to the claimtype.

```
claims("sub", "biscuitName");
claims("iss", "my-issuer");
claims("exp", 2023-03-26T21:52:00Z);
claims("iat", 2023-03-26T19:52:00Z);                    
```

Since the authentication handler doesn't assume fixed claim names, there's no built-in validation. Custom Datalog needs to be provided to authenticate the token in form of Biscuit's Authorzer code syntax. For the example stated above, this could be:  

```
check if now($now), claims("exp", $exp), $exp > $now; //token expiry time isn't reached
check all now($now), claims("nbf", $nbf), $nbf < $now; //token is already valid - check all will skip if there's no claims("nbf")
            
allow if true;
```

As you see, there's an additional fact `now` which is provided by the authentaction handler. Again, not automatially, but by specifcing a custom fact provider. 

```csharp
 o.FactProviders.Add(
    (context, clock) => new Fact("now", clock.UtcNow.DateTime)
);
```

### Examples

Best place for examples are the tests, i.e. [BiscuitAuthenticationHandlerTests](../../tests/aspnet/BiscuitAuthenticationHandlerTests.cs).

### Public key for token validation

The public key used to verify the token needs to be communicated out-of-band. There's currently no automatic discovery (yet?).

### Claims, expiry, not before

Biscuits don't have well-defined claims, like `exp`, `nbf` or `iss`. Logic for validating these claims can be provided as authorizer datalog rules as explained above. 

## Authorization

Todo 

## Source

Most of this code has been inspired by the ASP.NET Core Framework's [JwtBearerHandler](https://github.com/dotnet/aspnetcore/tree/main/src/Security/Authentication/JwtBearer/src). Compared to the JwtBearerHandler, the BiscuitBearerHandler does NOT support (yet):

- Automatic discovery of public keys for signature validation
- Events
- Detailed error messages in in the `WWW-Authenticate` header

It also does NOT use [SecurityTokenHandler](https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.securitytokenhandler)