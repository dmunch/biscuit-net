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

    List<ScopeType> _scopeTypes = new List<ScopeType>();
    List<PublicKey> _publicKeys = new List<PublicKey>();

    public RuleConstrained GetRule()
    {
        return new RuleConstrained(
            new Fact("check1"), 
            _facts,
            _expressions,
            new Scope(_scopeTypes, _publicKeys)
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

    public override void ExitOriginElementAuthority([NotNull] ExpressionsParser.OriginElementAuthorityContext context)
    {
        _scopeTypes.Add(ScopeType.Authority);
    }

    public override void ExitOriginElementPrevious([NotNull] ExpressionsParser.OriginElementPreviousContext context)
    {
        _scopeTypes.Add(ScopeType.Previous);
    } 

    public override void ExitOriginElementPublicKey([NotNull] ExpressionsParser.OriginElementPublicKeyContext context)
    {
        var bytes = context.PUBLICKEYBYTES().GetText().TrimStart('/');
        var publicKey = new PublicKey(Algorithm.Ed25519, Convert.FromHexString(bytes));
        _publicKeys.Add(publicKey);
    }
}