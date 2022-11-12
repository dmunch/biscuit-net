using Antlr4.Runtime.Misc;

namespace biscuit_net.Parser;
using Datalog;
using Expressions;

public class RuleListener : ExpressionsBaseListener
{
    TermVisitor _termVisitor = new TermVisitor();
    ExpressionsVisitor _expressionsVisitor = new ExpressionsVisitor();

    List<Fact> _facts = new List<Fact>();
    List<Expression> _expressions = new List<Expression>();

    public RuleConstrained GetRuleExpressions()
    {
        return new RuleConstrained(
            new Fact("check1"), 
            _facts,
            _expressions
        );
    }
    
    public override void ExitPredicate([NotNull] ExpressionsParser.PredicateContext context) 
    {
        var terms = context.term();
        var name = context.NAME().GetText();

        var Facts = terms.Select(t => _termVisitor.Visit(t)).ToList();

        _facts.Add(new Fact(name, Facts));
    }

    public override void ExitRule_body_element([NotNull] ExpressionsParser.Rule_body_elementContext context) 
    {
        if(context.expression() != null)
        {
            var ops = _expressionsVisitor.Visit(context.expression());
            _expressions.Add(new Expression(ops));
        }
    }
}