using Antlr4.Runtime.Misc;

namespace biscuit_net.Parser;

using Antlr4.Runtime.Tree;
using Datalog;

public class AuthorizerListener : ExpressionsBaseListener
{
    TermVisitor _termVisitor = new TermVisitor();
    List<Fact> _facts = new List<Fact>();
    List<RuleConstrained> _rules = new List<RuleConstrained>();
    List<Policy> _policies = new List<Policy>();
    List<Check> _checks = new List<Check>();


    public AuthorizerBlock GetAuthorizerBlock()
    {
        var authorizerBlock = new AuthorizerBlock();

        foreach(var fact in _facts)
        {
            authorizerBlock.Add(fact);
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

    public override void ExitFact([NotNull] ExpressionsParser.FactContext context) 
    {
        var terms = context.fact_term();
        var name = context.NAME().GetText();

        var Facts = terms.Select(t => _termVisitor.Visit(t)).ToList();

        _facts.Add(new Fact(name, Facts));
    }

    public override void ExitRule_body([NotNull] ExpressionsParser.Rule_bodyContext context)
    {
        var ruleListener = new RuleListener();
        ParseTreeWalker.Default.Walk(ruleListener, context);
        
        _rules.Add(ruleListener.GetRule());
    }

    public override void ExitPolicy([NotNull] ExpressionsParser.PolicyContext context)
    {
        var kind = context.kind.Text switch 
        {
            "deny" => PolicyKind.Deny,
            "allow" => PolicyKind.Allow,
            _ => throw new NotSupportedException($"Policy kind {context.kind.Text} not supported")
        };
        var policy = new Policy(kind, _rules.AsReadOnly());
        _policies.Add(policy);

        _rules = new List<RuleConstrained>();
    }

    public override void ExitCheck([NotNull] ExpressionsParser.CheckContext context)
    {
        var kind = context.kind.Text switch 
        {
            "if" => Check.CheckKind.One,
            "all" => Check.CheckKind.All,
            _ => throw new NotSupportedException($"Check kind {context.kind.Text} not supported")
        };
        var check = new Check(_rules, kind);
        _checks.Add(check);

        _rules = new List<RuleConstrained>();
    }
}
