//Required dummy class when targeting .netstandard2.1 from C# language versions supporting records
#if NETSTANDARD2_1
#pragma warning disable S2094 
namespace System.Runtime.CompilerServices;
internal static class IsExternalInit {}
#endif