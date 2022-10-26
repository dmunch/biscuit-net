grammar Expressions;

WS : [ \t\r\n]+ -> skip ; // skip spaces, tabs, newlines

VARIABLE: '$'[a-zA-Z_:0-9]+; //TODO
TERM: FACT_TERM | VARIABLE;
FACT_TERM: BOOLEAN | STRING | NUMBER | BYTES | DATE | SET;
SET_TERM: BOOLEAN | STRING | NUMBER | BYTES | DATE;

STRING : '"' ( '\\"' | . )*? '"' ; // match "foo", "\"", "x\"\"y", ... TODO unicode
NUMBER: '-'?[0-9]+;
BYTES: 'hex:'([a-z] | [0-9])+;
BOOLEAN: 'true' | 'false';
DATE: [0-9]* '-' [0-9] [0-9] '-' [0-9] [0-9] 'T' [0-9] [0-9] ':' [0-9] [0-9] ':' [0-9] [0-9] ( 'Z' | ( ('+' | '-') [0-9] [0-9] ':' [0-9] [0-9] ));
SET: '[' WS? ( FACT_TERM ( WS? ',' WS? SET_TERM)* WS? )? ']';

METHOD_NAME: ([a-z] | [A-Z] ) ([a-z] | [A-Z] | [0-9] | '_' )*;
OPERATOR: '<' | '>' | '<=' | '>=' | '==' | '&&' | '||' | '+' | '-' | '*' | '/';


expression: expression_element (WS OPERATOR WS expression_element)*;
expression_element: expression_unary | (expression_term expression_method? );
expression_unary: '!' WS expression;
expression_method: '.' METHOD_NAME '(' WS (TERM ( WS ',' WS TERM)* )? WS ')';
expression_term: TERM | ('(' WS expression WS ')');