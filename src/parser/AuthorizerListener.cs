using Antlr4.Runtime.Misc;

namespace biscuit_net.Parser;

using Antlr4.Runtime.Tree;
using Datalog;

public class AuthorizerListener : DatalogBaseListener
{
    readonly TermVisitor _termVisitor = new();
    readonly List<Fact> _facts = new();
    readonly List<RuleScoped> _rules = new();
    readonly List<Policy> _policies = new();
    readonly List<Check> _checks = new();


    public AuthorizerBlock GetAuthorizerBlock()
    {
        var authorizerBlock = new AuthorizerBlock();

        foreach(var fact in _facts)
        {
            authorizerBlock.Add(fact);
        }

        foreach(var rule in _rules)
        {
            authorizerBlock.Add(rule);
        }

        foreach(var policy in _policies)
        {
            authorizerBlock.Add(policy);
        }

        foreach(var check in _checks)
        {
            authorizerBlock.Add(check);
        }

        return authorizerBlock;
    }

    public Block GetBlock()
    {
        return new Block(
            _facts.ToList(),
            _rules.ToList(),
            _checks.ToList(),
            3
        );
    }

    public override void ExitFact([NotNull] DatalogParser.FactContext context) 
    {
        var terms = context.fact_term();
        var name = context.NAME().GetText();

        var Facts = terms.Select(t => _termVisitor.Visit(t)).ToList();

        _facts.Add(new Fact(name, Facts));
    }

    public override void ExitRule_([NotNull] DatalogParser.Rule_Context context)
    {
        var ruleBodyListener = new RuleBodyListener();
        ParseTreeWalker.Default.Walk(ruleBodyListener, context);

        _rules.Add(ruleBodyListener.GetRule());
    }

    public override void ExitPolicy([NotNull] DatalogParser.PolicyContext context)
    {
        var kind = context.kind.Text switch 
        {
            "deny" => PolicyKind.Deny,
            "allow" => PolicyKind.Allow,
            _ => throw new NotSupportedException($"Policy kind {context.kind.Text} not supported")
        };
        var rules = GetHeadlessRules(new Fact("policy1"), context.rule_body());

        var policy = new Policy(kind, rules.AsReadOnly());
        _policies.Add(policy);
    }

    public override void ExitCheck([NotNull] DatalogParser.CheckContext context)
    {
        var kind = context.kind.Text switch 
        {
            "if" => Check.CheckKind.One,
            "all" => Check.CheckKind.All,
            _ => throw new NotSupportedException($"Check kind {context.kind.Text} not supported")
        };

        var rules = GetHeadlessRules(new Fact("check1"), context.rule_body());
        
        var check = new Check(rules, kind);
        _checks.Add(check);
    }

    static List<RuleScoped> GetHeadlessRules(Fact head, IEnumerable<DatalogParser.Rule_bodyContext> ruleBodyContexts)
    {
        var rules = new List<RuleScoped>();
        foreach(var ruleBodyContext in ruleBodyContexts) 
        {
            var ruleBodyListener = new RuleBodyListener();
            ParseTreeWalker.Default.Walk(ruleBodyListener, ruleBodyContext);
            rules.Add(ruleBodyListener.GetHeadlessRule(head));
        }

        return rules;
    }
}
