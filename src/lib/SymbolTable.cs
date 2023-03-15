namespace biscuit_net;

public class SymbolTable
{
    readonly List<string> _symbols = new();
    public IReadOnlyList<string> Symbols { get => _symbols.AsReadOnly(); }
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

    static readonly List<string> table = new()
    {
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

    public string Lookup(ulong pos)
    {    
        if(pos < 1024)
            return table[(int)pos];

        return _symbols[(int)pos - 1024];
    }

    public uint LookupOrAdd(string str)
    {        
        if(table.Contains(str))
        {
            return (uint) table.IndexOf(str);
        }

        if(_symbols.Contains(str))
        {
            return (uint) _symbols.IndexOf(str) + 1024;
        }

        //we add the symbol
        _symbols.Add(str);
        return (uint) _symbols.Count - 1 + 1024;
    }
}
