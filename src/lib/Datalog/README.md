# Datalog

This directory contains naive, bottom-up implementation of Datalog's semantics based on https://github.com/xavierpinho/VeryNaiveDatalog

It's been modified slightly to work with Biscuit's expression and scoping mechanisms. 

## Introduction

A Datalog program consists of a set of *facts* and *rules*. Facts denote 
assertions, while rules denote relationships (from which new facts 
are obtainable.)

The "Hello World" of Datalog is graph reachability:
```datalog
// Rules
ancestor(x,y) :- parent(x,y).
ancestor(x,y) :- ancestor(x,z), parent(z,y).

// Facts
parent(Homer, Lisa).
parent(Homer, Bart).
parent(Grampa, Homer).
```

For the snippet above, running the query `?ancestor(x, Bart)` (in English:
for which values of `x` does `ancestor(x,Bart)` hold?) would output 2
results, namely:

* `x = Homer`
* `x = Grampa`

which are obtainable only by first deriving the facts 
`ancestor(Grampa,Homer)`, `ancestor(Homer, Bart)` and 
`ancestor(Grampa,Bart)`, by repeated application of the rules to the facts.


## How to use

The snippet above can be encoded in VeryNaiveDatalog as follows (see `EvaluatorTests.cs`):

```c#
// Rules
var r0 = new Rule(new Fact("ancestor", new Variable("x"), new Variable("y")),
                new Fact("parent", new Variable("x"), new Variable("y")));

var r1 = new Rule(new Fact("ancestor", new Variable("x"), new Variable("y")),
                new Fact("ancestor", new Variable("x"), new Variable("z")),
                new Fact("parent", new Variable("z"), new Variable("y")));

var rules = new[]{r0, r1};

// Facts
var f0 = new Fact("parent", new Symbol("Homer"), new Symbol("Lisa"));
var f1 = new Fact("parent", new Symbol("Homer"), new Symbol("Bart"));
var f2 = new Fact("parent", new Symbol("Grampa"), new Symbol("Homer"));

var facts = new[]{f0, f1, f2};

// Query
var q = new Fact("ancestor", new Variable("x"), new Symbol("Bart"));

// Run
var result = facts.Query(q, rules);
```

## References

* [The Essence of Datalog](https://dodisturb.me/posts/2018-12-25-The-Essence-of-Datalog.html) in Mistral Contrastin's blog.
* [VeryNaiveDatalog]( https://github.com/xavierpinho/VeryNaiveDatalog)