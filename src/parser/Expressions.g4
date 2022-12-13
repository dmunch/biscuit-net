grammar Expressions;


origin_clause: 'trusting' origin_element (',' origin_element)*;
origin_element
    : 'authority' #originElementAuthority
    | 'previous' #originElementPrevious
    | signature_alg  PUBLICKEYBYTES #originElementPublicKey
    ;

signature_alg: 'ed25519';

authorizer: (authorizer_element)*;
authorizer_element: ( policy | check | fact /*| rule*/ ) ';';

fact: NAME '(' fact_term (',' fact_term)* ')';
check: 'check' kind=('if' | 'all') rule_body ('or' rule_body)*;
policy: kind=('allow' | 'deny') 'if' rule_body ('or' rule_body)*;

rule_body: rule_body_element (',' rule_body_element)* (origin_clause)?;
rule_body_element: predicate | expression;
predicate: NAME '(' term (',' term)* ')';

expression
    : '!' expression #expressionUnary
    | '(' expression ')' #expressionParentheses
    | expression METHOD_INVOCATION '(' (term ( ',' term)* )? ')' #expressionMethod
    | expression mult=('*' | '/') expression #expressionMult
    | expression add=('+' | '-') expression #expressionAdd
    | expression logic=('||' | '&&') expression #expressionLogic
    | expression comp=('>=' | '<=' | '>' | '<' | '==') expression #expressionComp
    | fact_term #expressionTerm
    | VARIABLE #expressionVariable
    ;

term: fact_term | VARIABLE;
fact_term: set_term #setTerm
    | set #setFactTerm;
set_term: BOOLEAN #booleanFactTerm
    | STRING #stringFactTerm
    | NUMBER #numberFactTerm
    | BYTES #bytesFactTerm
    | DATE #dateFactTerm;

VARIABLE: '$'[a-zA-Z_:0-9]+; //TODO


STRING : '"' ( '\\"' | . )*? '"' ; // match "foo", "\"", "x\"\"y", ... TODO unicode
NUMBER: '-'?[0-9]+;
BYTES: 'hex:'([a-f] | [0-9])+;
PUBLICKEYBYTES: '/'([a-f] | [0-9])+;
BOOLEAN: 'true' | 'false';
DATE: [0-9]* '-' [0-9] [0-9] '-' [0-9] [0-9] 'T' [0-9] [0-9] ':' [0-9] [0-9] ':' [0-9] [0-9] ( 'Z' | ( ('+' | '-') [0-9] [0-9] ':' [0-9] [0-9] ));
set: '['  ( fact_term (  ',' set_term)* )? ']';


METHOD_INVOCATION: '.' ([a-z] | [A-Z] ) ([a-z] | [A-Z] | [0-9] | '_' )*;
NAME: [a-zA-Z][a-zA-Z_:0-9]+;


WS : [ \t\r\n]+ -> skip ; // skip spaces, tabs, newlines