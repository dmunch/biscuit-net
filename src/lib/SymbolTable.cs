namespace biscuit_net;

public class SymbolTable
{
    List<string> _symbols = new List<string>();
    
    public SymbolTable()
    {
    }

    public SymbolTable(IEnumerable<string> initialSymbols)
    {
        AddSymbols(initialSymbols);
    }

    public void AddSymbols(IEnumerable<string> symbols)
    {
        foreach(var symbol in symbols)
        {
            _symbols.Add(symbol);
        }
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
