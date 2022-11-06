namespace biscuit_net;

public class SymbolTable
{
    List<string> _symbols;
    public SymbolTable(IEnumerable<string> initialSymbols)
    {
        _symbols = initialSymbols.ToList();
    }

    public void AddSymbols(IEnumerable<string> symbols)
    {
        _symbols.AddRange(symbols);
    }

    public string Lookup(ulong pos)
    {
        var table = new []{
            "read",
            "write",
            "resource",
            "operation",
            "right",
            "time",
            "role",
            "owner",
            "tenant",
            "namespace",
            "user",
            "team",
            "service",
            "admin",
            "email",
            "group",
            "member",
            "ip_address",
            "client",
            "client_ip",
            "domain",
            "path",
            "version",
            "cluster",
            "node",
            "hostname",
            "nonce",
            "query"
        };

        if(pos < 1024)
            return table[pos];

        return _symbols[(int)pos - 1024];
    }
}
