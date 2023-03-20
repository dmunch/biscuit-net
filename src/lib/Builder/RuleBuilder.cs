using biscuit_net.Datalog;

namespace biscuit_net.Builder;

public static class CheckBuilderExtensions
{
    public static CheckBuilder AddCheck(this BlockBuilder builder, Check.CheckKind kind)
    {
        return new CheckBuilder(builder, kind);
    }
}

public class CheckBuilder
{
    readonly BlockBuilder _topLevelBuilder;
    readonly Check.CheckKind _kind;
    readonly List<RuleScoped> _rules;
    
    public CheckBuilder(BlockBuilder topLevelBuilder, Check.CheckKind kind)
    {
        _topLevelBuilder = topLevelBuilder;
        _kind = kind;
        _rules = new List<RuleScoped>();
    }

    public CheckBuilder(BlockBuilder topLevelBuilder, Check existingCheck)
    {
        _topLevelBuilder = topLevelBuilder;
        _kind = existingCheck.Kind;
        _rules = existingCheck.Rules.ToList();
    }

    
    public CheckBuilder Add(RuleScoped rule)
    {
        _rules.Add(rule);
        return this;
    }
    
    public BlockBuilder EndCheck()
    {
        var check = new Check(_rules, _kind);
        _topLevelBuilder.Add(check);
        return _topLevelBuilder;
    }
}

public class CheckRuleBuilder : RuleBuilderBase
{
    readonly CheckBuilder _topLevelBuilder;

    public CheckRuleBuilder(CheckBuilder toplevelBuilder, RuleScoped other) : base(other)
    {
        _topLevelBuilder = toplevelBuilder;
    }

    public CheckBuilder EndRule() => _topLevelBuilder.Add(GetRule());
    public CheckRuleBuilder AddBody(Fact fact) { _body.Add(fact); return this; }
    public CheckRuleBuilder AddExpression(Expressions.Expression expression) { _constraints.Add(expression); return this; }
    public CheckRuleBuilder Trusts(ScopeType scopeType) { _scopeTypes.Add(scopeType); return this; }
    public CheckRuleBuilder Trusts(PublicKey publicKey) { _trustedKeys.Add(publicKey); return this; }
}

public class BlockRuleBuilder : RuleBuilderBase
{
    readonly BlockBuilder _topLevelBuilder;

    public BlockRuleBuilder(BlockBuilder toplevelBuilder, RuleScoped other) : base(other)
    {
        _topLevelBuilder = toplevelBuilder;
    }

    public BlockBuilder EndRule() => _topLevelBuilder.Add(GetRule());
    public BlockRuleBuilder AddBody(Fact fact) { _body.Add(fact); return this; }
    public BlockRuleBuilder AddExpression(Expressions.Expression expression) { _constraints.Add(expression); return this; }
    public BlockRuleBuilder Trusts(ScopeType scopeType) { _scopeTypes.Add(scopeType); return this; }
    public BlockRuleBuilder Trusts(PublicKey publicKey) { _trustedKeys.Add(publicKey); return this; }
}


public class RuleBuilderBase
{    
    protected Fact _head;
    protected List<Fact> _body;
    protected List<Expressions.Expression> _constraints;
    protected List<ScopeType> _scopeTypes;
    protected List<PublicKey> _trustedKeys;

    
    public RuleBuilderBase(Fact head)
    {        
        _head = head;
        _body = new List<Fact>();
        _constraints = new List<Expressions.Expression>();
        _scopeTypes = new List<ScopeType>() { ScopeType.Authority } ;
        _trustedKeys = new List<PublicKey>();
    }

    public RuleBuilderBase(RuleScoped existingRule)
    {
        
        _head = existingRule.Head;
        _body = existingRule.Body.ToList();
        _constraints = existingRule.Constraints.ToList();
        _scopeTypes = existingRule.Scope.Types.ToList();
        _trustedKeys = existingRule.Scope.Keys.ToList();
    }
    
    protected RuleScoped GetRule()
    {
        return new RuleScoped(_head, _body, _constraints, new Scope(_scopeTypes, _trustedKeys));
    }
}
