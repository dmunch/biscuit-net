grammar Expressions;

check: 'check' 'if' rule_body ('or' rule_body)*;

rule_body: rule_body_element (',' rule_body_element)*;
rule_body_element: /*predicate |*/ expression;


expression
    : '!' expression #expressionUnary
    | '(' expression ')' #expressionParentheses
    | expression '.' METHOD_NAME '(' (term ( ',' term)* )? ')' #expressionMethod
    | expression mult=('*' | '/') expression #expressionMult
    | expression add=('+' | '-') expression #expressionAdd
    | expression logic=('||' | '&&') expression #expressionLogic
    | expression comp=('>=' | '<=' | '>' | '<' | '==') expression #expressionComp
    | fact_term #expressionTerm
    | VARIABLE #expressionVariable
    ;

term: fact_term | VARIABLE;
fact_term: BOOLEAN #booleanFactTerm
    | STRING #stringFactTerm
    | NUMBER #numberFactTerm
    | BYTES #bytesFactTerm
    | DATE #dateFactTerm
    | set #setFactTerm;
set_term: BOOLEAN #booleanSetTerm
    | STRING #stringSetTerm
    | NUMBER #numberSetTerm
    | BYTES #bytesSetTerm
    | DATE #dateSetTerm;

VARIABLE: '$'[a-zA-Z_:0-9]+; //TODO

STRING : '"' ( '\\"' | . )*? '"' ; // match "foo", "\"", "x\"\"y", ... TODO unicode
NUMBER: '-'?[0-9]+;
BYTES: 'hex:'([a-z] | [0-9])+;
BOOLEAN: 'true' | 'false';
DATE: [0-9]* '-' [0-9] [0-9] '-' [0-9] [0-9] 'T' [0-9] [0-9] ':' [0-9] [0-9] ':' [0-9] [0-9] ( 'Z' | ( ('+' | '-') [0-9] [0-9] ':' [0-9] [0-9] ));
set: '['  ( fact_term (  ',' set_term)* )? ']';

METHOD_NAME: ([a-z] | [A-Z] ) ([a-z] | [A-Z] | [0-9] | '_' )*;
OPERATOR: '<' | '>' | '<=' | '>=' | '==' | '&&' | '||' | '+' | '-' | '*' | '/';

WS : [ \t\r\n]+ -> skip ; // skip spaces, tabs, newlines