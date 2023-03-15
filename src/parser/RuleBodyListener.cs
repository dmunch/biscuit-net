using Antlr4.Runtime.Misc;

namespace biscuit_net.Parser;
using Datalog;
using Expressions;

public class RuleBodyListener : ExpressionsBaseListener
{
    TermVisitor _termVisitor = new TermVisitor();
    ExpressionsVisitor _expressionsVisitor = new ExpressionsVisitor();

    List<Fact> _facts = new List<Fact>();
    List<Expression> _expressions = new List<Expression>();

    List<ScopeType> _scopeTypes = new List<ScopeType>();
    List<PublicKey> _publicKeys = new List<PublicKey>();

    Fact? _head = null;
    public Rule GetRule()
    {
        if(_head == null) 
        {
            throw new Exception("Rule doesn't have a head");
        }

        return GetHeadlessRule(_head);
    }

    public Rule GetHeadlessRule(Fact head)
    {
        return new Rule(
            head, 
            _facts,
            _expressions,
            new Scope(_scopeTypes, _publicKeys)
        );
    }

    public override void ExitRule_([NotNull] ExpressionsParser.Rule_Context context) 
    {
        _head = PredicateToFact(context.predicate());
    }
    
    Fact PredicateToFact([NotNull] ExpressionsParser.PredicateContext context) 
    {
        var termContexts = context.term();
        var name = context.NAME().GetText();

        var terms = termContexts.Select(t => _termVisitor.Visit(t)).ToList();

        return new Fact(name, terms);
    }

    public override void ExitRule_body_element([NotNull] ExpressionsParser.Rule_body_elementContext context) 
    {
        if(context.expression() != null)
        {
            var ops = _expressionsVisitor.Visit(context.expression());
            _expressions.Add(new Expression(ops));
        }
        if(context.predicate() != null)
        {
            _facts.Add(PredicateToFact(context.predicate()));
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
        var publicKey = new PublicKey(Algorithm.Ed25519, HexConvert.FromHexString(bytes));
        _publicKeys.Add(publicKey);
    }
}