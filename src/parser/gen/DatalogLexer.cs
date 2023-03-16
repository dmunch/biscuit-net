//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Datalog.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public partial class DatalogLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, T__21=22, T__22=23, T__23=24, 
		T__24=25, T__25=26, T__26=27, T__27=28, T__28=29, VARIABLE=30, STRING=31, 
		NUMBER=32, BYTES=33, PUBLICKEYBYTES=34, BOOLEAN=35, DATE=36, METHOD_INVOCATION=37, 
		NAME=38, WS=39;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "T__10", "T__11", "T__12", "T__13", "T__14", "T__15", "T__16", 
		"T__17", "T__18", "T__19", "T__20", "T__21", "T__22", "T__23", "T__24", 
		"T__25", "T__26", "T__27", "T__28", "VARIABLE", "STRING", "NUMBER", "BYTES", 
		"PUBLICKEYBYTES", "BOOLEAN", "DATE", "METHOD_INVOCATION", "NAME", "WS"
	};


	public DatalogLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public DatalogLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'trusting'", "','", "'authority'", "'previous'", "'ed25519'", "';'", 
		"'('", "')'", "'<-'", "'check'", "'if'", "'all'", "'or'", "'allow'", "'deny'", 
		"'!'", "'*'", "'/'", "'+'", "'-'", "'||'", "'&&'", "'>='", "'<='", "'>'", 
		"'<'", "'=='", "'['", "']'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, "VARIABLE", "STRING", "NUMBER", "BYTES", 
		"PUBLICKEYBYTES", "BOOLEAN", "DATE", "METHOD_INVOCATION", "NAME", "WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Datalog.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static DatalogLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,39,291,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,35,
		7,35,2,36,7,36,2,37,7,37,2,38,7,38,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,
		1,1,1,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,3,1,3,1,3,1,3,1,3,1,
		3,1,3,1,3,1,3,1,4,1,4,1,4,1,4,1,4,1,4,1,4,1,4,1,5,1,5,1,6,1,6,1,7,1,7,
		1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,9,1,9,1,10,1,10,1,10,1,11,1,11,1,11,1,11,
		1,12,1,12,1,12,1,13,1,13,1,13,1,13,1,13,1,13,1,14,1,14,1,14,1,14,1,14,
		1,15,1,15,1,16,1,16,1,17,1,17,1,18,1,18,1,19,1,19,1,20,1,20,1,20,1,21,
		1,21,1,21,1,22,1,22,1,22,1,23,1,23,1,23,1,24,1,24,1,25,1,25,1,26,1,26,
		1,26,1,27,1,27,1,28,1,28,1,29,1,29,4,29,189,8,29,11,29,12,29,190,1,30,
		1,30,1,30,1,30,5,30,197,8,30,10,30,12,30,200,9,30,1,30,1,30,1,31,3,31,
		205,8,31,1,31,4,31,208,8,31,11,31,12,31,209,1,32,1,32,1,32,1,32,1,32,1,
		32,4,32,218,8,32,11,32,12,32,219,1,33,1,33,4,33,224,8,33,11,33,12,33,225,
		1,34,1,34,1,34,1,34,1,34,1,34,1,34,1,34,1,34,3,34,237,8,34,1,35,5,35,240,
		8,35,10,35,12,35,243,9,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,
		1,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,1,35,3,35,
		267,8,35,1,36,1,36,3,36,271,8,36,1,36,5,36,274,8,36,10,36,12,36,277,9,
		36,1,37,1,37,4,37,281,8,37,11,37,12,37,282,1,38,4,38,286,8,38,11,38,12,
		38,287,1,38,1,38,1,198,0,39,1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,
		10,21,11,23,12,25,13,27,14,29,15,31,16,33,17,35,18,37,19,39,20,41,21,43,
		22,45,23,47,24,49,25,51,26,53,27,55,28,57,29,59,30,61,31,63,32,65,33,67,
		34,69,35,71,36,73,37,75,38,77,39,1,0,7,4,0,48,58,65,90,95,95,97,122,1,
		0,48,57,2,0,48,57,97,102,2,0,43,43,45,45,2,0,65,90,97,122,4,0,48,57,65,
		90,95,95,97,122,3,0,9,10,13,13,32,32,303,0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,
		0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,
		17,1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,
		0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,0,0,0,
		0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,0,49,
		1,0,0,0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,1,0,0,
		0,0,61,1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,0,0,71,
		1,0,0,0,0,73,1,0,0,0,0,75,1,0,0,0,0,77,1,0,0,0,1,79,1,0,0,0,3,88,1,0,0,
		0,5,90,1,0,0,0,7,100,1,0,0,0,9,109,1,0,0,0,11,117,1,0,0,0,13,119,1,0,0,
		0,15,121,1,0,0,0,17,123,1,0,0,0,19,126,1,0,0,0,21,132,1,0,0,0,23,135,1,
		0,0,0,25,139,1,0,0,0,27,142,1,0,0,0,29,148,1,0,0,0,31,153,1,0,0,0,33,155,
		1,0,0,0,35,157,1,0,0,0,37,159,1,0,0,0,39,161,1,0,0,0,41,163,1,0,0,0,43,
		166,1,0,0,0,45,169,1,0,0,0,47,172,1,0,0,0,49,175,1,0,0,0,51,177,1,0,0,
		0,53,179,1,0,0,0,55,182,1,0,0,0,57,184,1,0,0,0,59,186,1,0,0,0,61,192,1,
		0,0,0,63,204,1,0,0,0,65,211,1,0,0,0,67,221,1,0,0,0,69,236,1,0,0,0,71,241,
		1,0,0,0,73,268,1,0,0,0,75,278,1,0,0,0,77,285,1,0,0,0,79,80,5,116,0,0,80,
		81,5,114,0,0,81,82,5,117,0,0,82,83,5,115,0,0,83,84,5,116,0,0,84,85,5,105,
		0,0,85,86,5,110,0,0,86,87,5,103,0,0,87,2,1,0,0,0,88,89,5,44,0,0,89,4,1,
		0,0,0,90,91,5,97,0,0,91,92,5,117,0,0,92,93,5,116,0,0,93,94,5,104,0,0,94,
		95,5,111,0,0,95,96,5,114,0,0,96,97,5,105,0,0,97,98,5,116,0,0,98,99,5,121,
		0,0,99,6,1,0,0,0,100,101,5,112,0,0,101,102,5,114,0,0,102,103,5,101,0,0,
		103,104,5,118,0,0,104,105,5,105,0,0,105,106,5,111,0,0,106,107,5,117,0,
		0,107,108,5,115,0,0,108,8,1,0,0,0,109,110,5,101,0,0,110,111,5,100,0,0,
		111,112,5,50,0,0,112,113,5,53,0,0,113,114,5,53,0,0,114,115,5,49,0,0,115,
		116,5,57,0,0,116,10,1,0,0,0,117,118,5,59,0,0,118,12,1,0,0,0,119,120,5,
		40,0,0,120,14,1,0,0,0,121,122,5,41,0,0,122,16,1,0,0,0,123,124,5,60,0,0,
		124,125,5,45,0,0,125,18,1,0,0,0,126,127,5,99,0,0,127,128,5,104,0,0,128,
		129,5,101,0,0,129,130,5,99,0,0,130,131,5,107,0,0,131,20,1,0,0,0,132,133,
		5,105,0,0,133,134,5,102,0,0,134,22,1,0,0,0,135,136,5,97,0,0,136,137,5,
		108,0,0,137,138,5,108,0,0,138,24,1,0,0,0,139,140,5,111,0,0,140,141,5,114,
		0,0,141,26,1,0,0,0,142,143,5,97,0,0,143,144,5,108,0,0,144,145,5,108,0,
		0,145,146,5,111,0,0,146,147,5,119,0,0,147,28,1,0,0,0,148,149,5,100,0,0,
		149,150,5,101,0,0,150,151,5,110,0,0,151,152,5,121,0,0,152,30,1,0,0,0,153,
		154,5,33,0,0,154,32,1,0,0,0,155,156,5,42,0,0,156,34,1,0,0,0,157,158,5,
		47,0,0,158,36,1,0,0,0,159,160,5,43,0,0,160,38,1,0,0,0,161,162,5,45,0,0,
		162,40,1,0,0,0,163,164,5,124,0,0,164,165,5,124,0,0,165,42,1,0,0,0,166,
		167,5,38,0,0,167,168,5,38,0,0,168,44,1,0,0,0,169,170,5,62,0,0,170,171,
		5,61,0,0,171,46,1,0,0,0,172,173,5,60,0,0,173,174,5,61,0,0,174,48,1,0,0,
		0,175,176,5,62,0,0,176,50,1,0,0,0,177,178,5,60,0,0,178,52,1,0,0,0,179,
		180,5,61,0,0,180,181,5,61,0,0,181,54,1,0,0,0,182,183,5,91,0,0,183,56,1,
		0,0,0,184,185,5,93,0,0,185,58,1,0,0,0,186,188,5,36,0,0,187,189,7,0,0,0,
		188,187,1,0,0,0,189,190,1,0,0,0,190,188,1,0,0,0,190,191,1,0,0,0,191,60,
		1,0,0,0,192,198,5,34,0,0,193,194,5,92,0,0,194,197,5,34,0,0,195,197,9,0,
		0,0,196,193,1,0,0,0,196,195,1,0,0,0,197,200,1,0,0,0,198,199,1,0,0,0,198,
		196,1,0,0,0,199,201,1,0,0,0,200,198,1,0,0,0,201,202,5,34,0,0,202,62,1,
		0,0,0,203,205,5,45,0,0,204,203,1,0,0,0,204,205,1,0,0,0,205,207,1,0,0,0,
		206,208,7,1,0,0,207,206,1,0,0,0,208,209,1,0,0,0,209,207,1,0,0,0,209,210,
		1,0,0,0,210,64,1,0,0,0,211,212,5,104,0,0,212,213,5,101,0,0,213,214,5,120,
		0,0,214,215,5,58,0,0,215,217,1,0,0,0,216,218,7,2,0,0,217,216,1,0,0,0,218,
		219,1,0,0,0,219,217,1,0,0,0,219,220,1,0,0,0,220,66,1,0,0,0,221,223,5,47,
		0,0,222,224,7,2,0,0,223,222,1,0,0,0,224,225,1,0,0,0,225,223,1,0,0,0,225,
		226,1,0,0,0,226,68,1,0,0,0,227,228,5,116,0,0,228,229,5,114,0,0,229,230,
		5,117,0,0,230,237,5,101,0,0,231,232,5,102,0,0,232,233,5,97,0,0,233,234,
		5,108,0,0,234,235,5,115,0,0,235,237,5,101,0,0,236,227,1,0,0,0,236,231,
		1,0,0,0,237,70,1,0,0,0,238,240,7,1,0,0,239,238,1,0,0,0,240,243,1,0,0,0,
		241,239,1,0,0,0,241,242,1,0,0,0,242,244,1,0,0,0,243,241,1,0,0,0,244,245,
		5,45,0,0,245,246,7,1,0,0,246,247,7,1,0,0,247,248,5,45,0,0,248,249,7,1,
		0,0,249,250,7,1,0,0,250,251,5,84,0,0,251,252,7,1,0,0,252,253,7,1,0,0,253,
		254,5,58,0,0,254,255,7,1,0,0,255,256,7,1,0,0,256,257,5,58,0,0,257,258,
		7,1,0,0,258,266,7,1,0,0,259,267,5,90,0,0,260,261,7,3,0,0,261,262,7,1,0,
		0,262,263,7,1,0,0,263,264,5,58,0,0,264,265,7,1,0,0,265,267,7,1,0,0,266,
		259,1,0,0,0,266,260,1,0,0,0,267,72,1,0,0,0,268,270,5,46,0,0,269,271,7,
		4,0,0,270,269,1,0,0,0,271,275,1,0,0,0,272,274,7,5,0,0,273,272,1,0,0,0,
		274,277,1,0,0,0,275,273,1,0,0,0,275,276,1,0,0,0,276,74,1,0,0,0,277,275,
		1,0,0,0,278,280,7,4,0,0,279,281,7,0,0,0,280,279,1,0,0,0,281,282,1,0,0,
		0,282,280,1,0,0,0,282,283,1,0,0,0,283,76,1,0,0,0,284,286,7,6,0,0,285,284,
		1,0,0,0,286,287,1,0,0,0,287,285,1,0,0,0,287,288,1,0,0,0,288,289,1,0,0,
		0,289,290,6,38,0,0,290,78,1,0,0,0,18,0,190,196,198,204,209,217,219,223,
		225,236,241,266,270,273,275,282,287,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}