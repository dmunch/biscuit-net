using Antlr4.Runtime.Misc;

namespace biscuit_net.Parser;

using Antlr4.Runtime.Tree;
using Datalog;

public class AuthorizerListener : ExpressionsBaseListener
{
    TermVisitor _termVisitor = new TermVisitor();
    List<Fact> _facts = new List<Fact>();
    List<Rule> _rules = new List<Rule>();
    List<Policy> _policies = new List<Policy>();
    List<Check> _checks = new List<Check>();


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

    public override void ExitFact([NotNull] ExpressionsParser.FactContext context) 
    {
        var terms = context.fact_term();
        var name = context.NAME().GetText();

        var Facts = terms.Select(t => _termVisitor.Visit(t)).ToList();

        _facts.Add(new Fact(name, Facts));
    }

    public override void ExitRule_([NotNull] ExpressionsParser.Rule_Context context)
    {
        var ruleBodyListener = new RuleBodyListener();
        ParseTreeWalker.Default.Walk(ruleBodyListener, context);

        _rules.Add(ruleBodyListener.GetRule());
    }

    public override void ExitPolicy([NotNull] ExpressionsParser.PolicyContext context)
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

    public override void ExitCheck([NotNull] ExpressionsParser.CheckContext context)
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

    static List<Rule> GetHeadlessRules(Fact head, IEnumerable<ExpressionsParser.Rule_bodyContext> ruleBodyContexts)
    {
        var rules = new List<Rule>();
        foreach(var ruleBodyContext in ruleBodyContexts) 
        {
            var ruleBodyListener = new RuleBodyListener();
            ParseTreeWalker.Default.Walk(ruleBodyListener, ruleBodyContext);
            rules.Add(ruleBodyListener.GetHeadlessRule(head));
        }

        return rules;
    }
}
