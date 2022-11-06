namespace biscuit_net.Parser;

using Antlr4.Runtime.Misc;
using VeryNaiveDatalog;

using biscuit_net.Datalog;
using biscuit_net.Expressions;

public class RuleListener : ExpressionsBaseListener
{
    TermVisitor _termVisitor = new TermVisitor();
    ExpressionsVisitor _expressionsVisitor = new ExpressionsVisitor();

    List<Atom> _atoms = new List<Atom>();
    List<Expression> _expressions = new List<Expression>();

    public RuleExpressions GetRuleExpressions()
    {
        return new RuleExpressions(
            new Atom("check1"), 
            _atoms,
            _expressions
        );
    }
    
    public override void ExitPredicate([NotNull] ExpressionsParser.PredicateContext context) 
    {
        var terms = context.term();
        var name = context.NAME().GetText();

        var atoms = terms.Select(t => _termVisitor.Visit(t)).ToList();

        _atoms.Add(new Atom(name, atoms));
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