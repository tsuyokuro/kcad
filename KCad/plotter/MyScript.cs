﻿using Irony.Ast;
using Irony.Parsing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyScript
{
    public class MyGrammar : Grammar
    {
        const String IDENT = "ident";
        const String NUMBER = "number";
        const String STRING_LITERAL = "StringLiteral";
        const String EXPR = "Expr";
        const String UNEXPR = "UnExpr";
        const String TERM = "Term";
        const String BIN_EXPR = "BinExpr";
        const String PAR_EXPR = "ParExpr";
        const String UN_OP = "UnOp";
        const String BIN_OP = "BinaryOperator";
        const String EXPR_LIST = "ExprList";
        const String FUNC_CALL = "FuncCall";
        const String SET_STMT = "SetStmt";
        const String SET_OP = "SetOp";
        const String STATEMENT = "Statement";
        const String PROGRAM_LINE = "ProgramLine";
        const String PROGRAM = "Program";
        const String IDENT_LIST = "IdentList";
        const String IDENT_LIST_SET = "IdentListSet";

        const String ADD = "Add";
        const String SUB = "Sub";
        const String MUL = "Mul";
        const String DIV = "Div";
        const String NOT = "Not";
        const String SET = "Set";

        const String COMMA = "Comma";
        const String SEMC = "Semc";
        const String PAR_L = "ParL";
        const String PAR_R = "ParR";

        public enum NType
        {
            UNKNOWN = 0,

            IDENT = 1,
            NUMBER,
            STRING_LITERAL,
            EXPR,
            UNEXPR,
            TERM,
            BIN_EXPR,
            PAR_EXPR,
            UN_OP,
            BIN_OP,
            EXPR_LIST,
            FUNC_CALL,
            SET_STMT,
            SET_OP,
            STATEMENT,
            PROGRAM_LINE,
            PROGRAM,
            IDENT_LIST,
            IDENT_LIST_SET,

            ADD,
            SUB,
            MUL,
            DIV,
            NOT,
            SET,

            COMMA,
            SEMC,
            PAR_L,
            PAR_R,
        }

        Dictionary<string, NType> NameMap
           = new Dictionary<string, NType>()
        {
            {IDENT,             NType.IDENT},
            {NUMBER,            NType.NUMBER},
            {STRING_LITERAL,    NType.STRING_LITERAL},
            {EXPR,              NType.EXPR},
            {UNEXPR,            NType.UNEXPR},
            {TERM,              NType.TERM},
            {BIN_EXPR,          NType.BIN_EXPR},
            {PAR_EXPR,          NType.PAR_EXPR},
            {UN_OP,             NType.UN_OP},
            {BIN_OP,            NType.BIN_OP},
            {EXPR_LIST,         NType.EXPR_LIST},
            {FUNC_CALL,         NType.FUNC_CALL},
            {SET_STMT,          NType.SET_STMT},
            {SET_OP,            NType.SET_OP},
            {STATEMENT,         NType.STATEMENT},
            {PROGRAM_LINE,      NType.PROGRAM_LINE},
            {PROGRAM,           NType.PROGRAM},
            {IDENT_LIST,        NType.IDENT_LIST},
            {IDENT_LIST_SET,    NType.IDENT_LIST_SET},

            {ADD,               NType.ADD},
            {SUB,               NType.SUB},
            {MUL,               NType.MUL},
            {DIV,               NType.DIV},
            {NOT,               NType.NOT},
            {SET,               NType.SET},


            {COMMA,             NType.COMMA},
            {SEMC,              NType.SEMC},
            {PAR_L,             NType.PAR_L},
            {PAR_R,             NType.PAR_R},
        };

        public MyGrammar() : base(true)
        {
            var Number = new NumberLiteral(NUMBER);
            Number.DefaultIntTypes = new[]
            {
                TypeCode.Int32,
                TypeCode.Int64,
                NumberLiteral.TypeCodeBigInt
            };

            Number.DefaultFloatType = TypeCode.Double;

            var StrLiteral = TerminalFactory.CreateCSharpString(STRING_LITERAL);


            var Ident = new IdentifierTerminal(IDENT);
            var Expr = new NonTerminal(EXPR);
            var UnExpr = new NonTerminal(UNEXPR);
            var Term = new NonTerminal(TERM);
            var BinExpr = new NonTerminal(BIN_EXPR);
            var ParExpr = new NonTerminal(PAR_EXPR);
            var UnOp = new NonTerminal(UN_OP);
            var BinOp = new NonTerminal(BIN_OP);
            var ExprList = new NonTerminal(EXPR_LIST);
            var FuncCall = new NonTerminal(FUNC_CALL);
            var SetStmt = new NonTerminal(SET_STMT);
            var SetOp = new NonTerminal(SET_OP);
            var Statement = new NonTerminal(STATEMENT);
            var ProgramLine = new NonTerminal(PROGRAM_LINE);
            var Program = new NonTerminal(PROGRAM);

            var IdentList = new NonTerminal(IDENT_LIST);
            var IdentListSetStmt = new NonTerminal(IDENT_LIST_SET);

            var Add = new NonTerminal(ADD);
            var Sub = new NonTerminal(SUB);
            var Mul = new NonTerminal(MUL);
            var Div = new NonTerminal(DIV);
            var Not = new NonTerminal(NOT);
            var Set = new NonTerminal(SET);

            var Comma = new KeyTerm(",", COMMA);
            var Semc = new KeyTerm(";", SEMC);
            var ParL = new KeyTerm("(", PAR_L);
            var ParR = new KeyTerm(")", PAR_R);

            Add.Rule = "+";
            Sub.Rule = "-";
            Mul.Rule = "*";
            Div.Rule = "/";
            Not.Rule = "!";
            Set.Rule = "=";


            Expr.Rule = Term | BinExpr | UnExpr;

            Term.Rule = Number | ParExpr | FuncCall | Ident | StrLiteral;

            ParExpr.Rule = ParL + Expr + ParR;

            UnExpr.Rule = UnOp + Term + ReduceHere();

            UnOp.Rule = Add | Sub | Not;

            BinExpr.Rule = Expr + BinOp + Expr;

            BinOp.Rule = Add | Sub | Mul | Div;

            ExprList.Rule = MakeStarRule(ExprList, Comma, Expr);

            FuncCall.Rule = Expr + PreferShiftHere() + ParL + ExprList + ParR;
            //FuncCall.Rule = Ident + ParL + ExprList + ParR;

            SetStmt.Rule = Ident + SetOp + Expr;

            IdentList.Rule = MakeStarRule(IdentList, Comma, Ident);

            IdentListSetStmt.Rule =
                ParL + IdentList + ParR + SetOp + FuncCall |
                ParL + IdentList + ParR + SetOp + ParL + ExprList + ParR |
                ParL + IdentList + ParR + SetOp + ExprList;

            SetOp.Rule = Set;

            Statement.Rule = IdentListSetStmt | SetStmt | Expr | Empty | ExprList;

            ProgramLine.Rule = Statement + Semc;

            Program.Rule = MakeStarRule(Program, ProgramLine);

            this.Root = Program;

            RegisterOperators(1, Add, Sub);
            RegisterOperators(2, Mul, Div);

            this.LanguageFlags =
                LanguageFlags.NewLineBeforeEOF |
                LanguageFlags.SupportsBigInt |
                LanguageFlags.CreateAst;


            Number.AstConfig.NodeCreator = astNodeCreator;
            StrLiteral.AstConfig.NodeCreator = astNodeCreator;
            Ident.AstConfig.NodeCreator = astNodeCreator;
            Expr.AstConfig.NodeCreator = astNodeCreator;
            UnExpr.AstConfig.NodeCreator = astNodeCreator;
            Term.AstConfig.NodeCreator = astNodeCreator;
            BinExpr.AstConfig.NodeCreator = astNodeCreator;
            ParExpr.AstConfig.NodeCreator = astNodeCreator;
            UnOp.AstConfig.NodeCreator = astNodeCreator;
            BinOp.AstConfig.NodeCreator = astNodeCreator;
            ExprList.AstConfig.NodeCreator = astNodeCreator;
            FuncCall.AstConfig.NodeCreator = astNodeCreator;
            SetStmt.AstConfig.NodeCreator = astNodeCreator;
            SetOp.AstConfig.NodeCreator = astNodeCreator;
            Statement.AstConfig.NodeCreator = astNodeCreator;
            ProgramLine.AstConfig.NodeCreator = astNodeCreator;
            Program.AstConfig.NodeCreator = astNodeCreator;
            IdentList.AstConfig.NodeCreator = astNodeCreator;
            IdentListSetStmt.AstConfig.NodeCreator = astNodeCreator;

            Add.AstConfig.NodeCreator = astNodeCreator;
            Sub.AstConfig.NodeCreator = astNodeCreator;
            Mul.AstConfig.NodeCreator = astNodeCreator;
            Div.AstConfig.NodeCreator = astNodeCreator;
            Not.AstConfig.NodeCreator = astNodeCreator;
            Set.AstConfig.NodeCreator = astNodeCreator;

            Comma.AstConfig.NodeCreator = astNodeCreator;
            Semc.AstConfig.NodeCreator = astNodeCreator;
            ParL.AstConfig.NodeCreator = astNodeCreator;
            ParR.AstConfig.NodeCreator = astNodeCreator;
        }

        public void astNodeCreator(AstContext context, ParseTreeNode parseNode)
        {
            NodeInfo ni = new NodeInfo();

            if (NameMap.ContainsKey(parseNode.Term.Name))
            {
                ni.TypeCode = NameMap[parseNode.Term.Name];
            }
            else
            {
                ni.TypeCode = NType.UNKNOWN;
            }

            parseNode.AstNode = ni;
        }

        public override void BuildAst(LanguageData language, ParseTree parseTree)
        {
            if (!LanguageFlags.IsSet(LanguageFlags.CreateAst))
                return;
            var astContext = new AstContext(language);
            var astBuilder = new AstBuilder(astContext);
            astBuilder.BuildAst(parseTree);
        }

        public class NodeInfo
        {
            public NType TypeCode;
        }
    }

    public class Evaluator
    {
        public delegate int Func(int argCount, ValueStack stack);

        private Dictionary<String, Func> FunctionMap = new Dictionary<String, Func>();
        private Dictionary<String, Value> VariableMap = new Dictionary<String, Value>();


        private Exception mLastException = null;

        public struct Value
        {
            public enum Types
            {
                INT,
                DOUBLE,
                STRING,
            }

            public Types Type;

            private Object val;

            public void setVal(Int64 v)
            {
                Type = Types.INT;
                val = v;
            }

            public void setVal(double v)
            {
                Type = Types.DOUBLE;
                val = v;
            }

            public void setVal(String v)
            {
                Type = Types.STRING;
                val = v;
            }

            public void setVal(Value s)
            {
                val = s.val;
                Type = s.Type;
            }

            public void setVal(ParseTreeNode n)
            {
                val = n.Token.Value;

                MyGrammar.NType nt = NodeType(n);

                if (nt == MyGrammar.NType.NUMBER)
                {
                    Type = Types.DOUBLE;
                }
                else if (nt == MyGrammar.NType.STRING_LITERAL)
                {
                    Type = Types.STRING;
                }
            }

            public double getDouble()
            {
                if (val is String)
                {
                    return 0;
                }
                else if (val is int)
                {
                    return (double)(int)val;
                }
                else if (val is Int64)
                {
                    return (double)(Int64)val;
                }

                return (double)val;
            }

            public String getString()
            {
                return val.ToString();
            }
        }

        public class ValueStack
        {
            private Value[] Stack = new Value[10];
            public int sp = 0;

            public void push(ParseTreeNode n)
            {
                sp++;
                Stack[sp].setVal(n);
            }

            public void push(Value s)
            {
                sp++;
                Stack[sp].setVal(s);
            }

            public void push(double v)
            {
                sp++;
                Stack[sp].setVal(v);
            }

            public void push(Int64 v)
            {
                sp++;
                Stack[sp].setVal(v);
            }

            public Value getRelative(int i)
            {
                return Stack[sp + i];
            }

            public Value get()
            {
                return Stack[sp];
            }

            public int getCount()
            {
                return sp;
            }

            public Value pop()
            {
                Value val = Stack[sp];
                sp--;

                return val;
            }
        }

        private ValueStack VStack = new ValueStack();

        public bool NodeIs(ParseTreeNode node, MyGrammar.NType t)
        {
            MyGrammar.NodeInfo ni = (MyGrammar.NodeInfo)(node.AstNode);

            if (ni == null)
            {
                return false;
            }

            return (ni.TypeCode == t);
        }

        public static MyGrammar.NType NodeType(ParseTreeNode node)
        {
            MyGrammar.NodeInfo ni = (MyGrammar.NodeInfo)(node.AstNode);
            return ni.TypeCode;
        }

        public bool NodeIs(ParseTreeNode node, String name)
        {
            return (node.Term.Name == name);
        }

        public void addFunction(String name, Func func)
        {
            FunctionMap.Add(name, func);
        }

        public void setVariable(String name, double v)
        {
            Value val = default(Value);
            val.setVal(v);
            setVariable(name, val);
        }

        public void setVariable(String name, Value v)
        {
            if (!VariableMap.ContainsKey(name))
            {
                VariableMap.Add(name, default(Value));
            }

            VariableMap[name] = v;
        }

        public List<Value> getOutput()
        {
            var ret = new List<Value>();

            int cnt = VStack.getCount();

            for (int i = cnt - 1; i >= 0; i--)
            {
                Value val = VStack.getRelative(-i);
                ret.Add(val);
            }

            return ret;
        }

        public bool evaluate(ParseTree pt)
        {
            ParseTreeNode node = pt.Root;

            if (node == null)
            {
                return false;
            }

            try
            {
                foreach (ParseTreeNode c in node.ChildNodes)
                {
                    if (NodeIs(c, MyGrammar.NType.PROGRAM_LINE))
                    {
                        // Initialize Value stack
                        VStack.sp = 0;

                        // evaluate statement + ;
                        evalProgramLine(c);
                    }
                }
            }
            catch (Exception e)
            {
                mLastException = e;
                return false;
            }

            return true;
        }

        public Exception getLastException()
        {
            return mLastException;
        }

        public void evalProgramLine(ParseTreeNode line)
        {
            foreach (ParseTreeNode c in line.ChildNodes)
            {
                if (NodeIs(c, MyGrammar.NType.STATEMENT))
                {
                    evalStatement(c);
                }
            }
        }

        public void evalStatement(ParseTreeNode s)
        {
            foreach (ParseTreeNode c in s.ChildNodes)
            {
                if (NodeIs(c, MyGrammar.NType.EXPR))
                {
                    evalExpr(c);
                }
                else if (NodeIs(c, MyGrammar.NType.EXPR_LIST))
                {
                    evalExprList(c);
                }
                else if (NodeIs(c, MyGrammar.NType.SET_STMT))
                {
                    evalSetStmt(c);
                }
                else if (NodeIs(c, MyGrammar.NType.IDENT_LIST_SET))
                {
                    evalIdentListSet(c);
                }
            }
        }

        public void evalSetStmt(ParseTreeNode sets)
        {
            ParseTreeNode vn = sets.ChildNodes[0];
            String name = vn.Token.Text;

            ParseTreeNode op = sets.ChildNodes[1];

            ParseTreeNode expr = sets.ChildNodes[2];

            int sp = VStack.sp;

            evalExpr(expr);

            Value val = VStack.get();

            //VStack.sp = sp;

            setVariable(name, val);
        }

        public void evalIdentListSet(ParseTreeNode sets)
        {
            ParseTreeNode left = sets.ChildNodes[1];

            ParseTreeNode right = sets.ChildNodes[4];

            int cnt = 0;

            if (NodeIs(right, MyGrammar.NType.FUNC_CALL))
            {
                cnt = evalFuncCall(right);
            }
            else if (NodeIs(right, MyGrammar.NType.EXPR_LIST))
            {
                cnt = right.ChildNodes.Count;
                evalExprList(right);
            }
            else if (!NodeIs(right, MyGrammar.NType.EXPR_LIST))
            {
                right = sets.ChildNodes[5];

                if (NodeIs(right, MyGrammar.NType.EXPR_LIST))
                {
                    cnt = right.ChildNodes.Count;
                    evalExprList(right);
                }
            }

            if (left.ChildNodes.Count != cnt)
            {
                throw new Exception(string.Format("Tuple count is not match."));
            }

            int li = left.ChildNodes.Count - 1;

            int sp = VStack.sp;

            for (; li >= 0; li--)
            {
                String name = left.ChildNodes[li].Token.Text;

                Value v = VStack.pop();
                setVariable(name, v);
            }

            VStack.sp = sp;
        }

        public void evalExprList(ParseTreeNode el)
        {
            foreach (ParseTreeNode c in el.ChildNodes)
            {
                if (NodeIs(c, MyGrammar.NType.EXPR))
                {
                    evalExpr(c);
                }
            }
        }

        public void evalExpr(ParseTreeNode expr)
        {
            foreach (ParseTreeNode c in expr.ChildNodes)
            {
                if (NodeIs(c, MyGrammar.NType.TERM))
                {
                    evalTerm(c);
                }
                else if (NodeIs(c, MyGrammar.NType.BIN_EXPR))
                {
                    evalBinExpr(c);
                }
                else if (NodeIs(c, MyGrammar.NType.UNEXPR))
                {
                    evalUnEpr(c);
                }
            }
        }

        public void evalTerm(ParseTreeNode term)
        {
            foreach (ParseTreeNode c in term.ChildNodes)
            {
                if (NodeIs(c, MyGrammar.NType.NUMBER))
                {
                    VStack.push(c);
                }
                else if (NodeIs(c, MyGrammar.NType.STRING_LITERAL))
                {
                    VStack.push(c);
                }
                else if (NodeIs(c, MyGrammar.NType.PAR_EXPR))
                {
                    evalParExpr(c);
                }
                else if (NodeIs(c, MyGrammar.NType.FUNC_CALL))
                {
                    if (evalFuncCall(c) != 1)
                    {
                        throw new Exception(string.Format("Function can not be term."));
                    }
                }
                else if (NodeIs(c, MyGrammar.NType.IDENT))
                {
                    evalTermIdent(c);
                }
            }
        }

        public void evalTermIdent(ParseTreeNode idn)
        {
            String name = idn.Token.Text;

            Value val;

            if (VariableMap.ContainsKey(name))
            {
                val = VariableMap[name];
                VStack.push(val);
            }
            else
            {
                if (FunctionMap.ContainsKey(name))
                {
                    FunctionMap[name](0, VStack);
                }
                else
                {
                    VStack.push(0);
                }
            }
        }

        public int evalFuncCall(ParseTreeNode f)
        {
            ParseTreeNode ident = f.ChildNodes[0].ChildNodes[0].ChildNodes[0];

            String funcName = ident.Token.Text;

            ParseTreeNode exprList = f.ChildNodes[2];

            int sp = VStack.sp;

            evalExprList(exprList);

            int argCount = VStack.sp -sp;

            if (FunctionMap.ContainsKey(funcName))
            {
                return FunctionMap[funcName](argCount, VStack);
            }

            return 0;
        }

        public void evalParExpr(ParseTreeNode pe)
        {
            // ChildNode[0] is "("
            ParseTreeNode expr = pe.ChildNodes[1];
            evalExpr(expr);
        }

        public void evalBinExpr(ParseTreeNode be)
        {
            int sp = VStack.sp;

            ParseTreeNode e1 = be.ChildNodes[0];
            ParseTreeNode e2 = be.ChildNodes[2];

            evalExpr(e1);

            Value v1 = VStack.get();
            VStack.sp = sp;

            evalExpr(e2);

            Value v2 = VStack.get();
            VStack.sp = sp;

            ParseTreeNode op = be.ChildNodes[1];

            op = op.ChildNodes[0];

            if (NodeIs(op, MyGrammar.NType.ADD))
            {
                double v = v1.getDouble() + v2.getDouble();
                VStack.push(v);
            }
            else if (NodeIs(op, MyGrammar.NType.SUB))
            {
                double v = v1.getDouble() - v2.getDouble();
                VStack.push(v);
            }
            else if (NodeIs(op, MyGrammar.NType.MUL))
            {
                double v = v1.getDouble() * v2.getDouble();
                VStack.push(v);
            }
            else if (NodeIs(op, MyGrammar.NType.DIV))
            {
                double v = v1.getDouble() / v2.getDouble();
                VStack.push(v);
            }
        }

        public void evalUnEpr(ParseTreeNode ue)
        {
            int sp = VStack.sp;

            ParseTreeNode uop = ue.ChildNodes[0].ChildNodes[0];
            ParseTreeNode term = ue.ChildNodes[1];
            evalTerm(term);

            Value v1 = VStack.get();
            VStack.sp = sp;

            if (NodeIs(uop, MyGrammar.NType.ADD))
            {
                double v = v1.getDouble();
                VStack.push(v);
            }
            else if (NodeIs(uop, MyGrammar.NType.SUB))
            {
                double v = -v1.getDouble();
                VStack.push(v);
            }
        }


        public class BuiltinFuncs
        {
            public static void attachTo(Evaluator evaluator)
            {
                evaluator.addFunction("test", Test);
                evaluator.addFunction("test3", Test3);
            }

            public static int Test(int argCount, ValueStack stack)
            {
                double v2 = stack.pop().getDouble();
                double v1 = stack.pop().getDouble();

                double v = v1 - v2;

                stack.push(v);

                return 1;
            }

            public static int Test3(int argCount, ValueStack stack)
            {
                double v3 = stack.pop().getDouble();
                double v2 = stack.pop().getDouble();
                double v1 = stack.pop().getDouble();


                stack.push(v1);
                stack.push(v2);
                stack.push(v3);

                return 3;
            }
        }

        public class BuiltinMath
        {
            public static void attachTo(Evaluator evaluator)
            {
                evaluator.setVariable("PI", Math.PI);

                evaluator.addFunction("rad", angToRad);
                evaluator.addFunction("sin", Sin);
                evaluator.addFunction("cos", Cos);
                evaluator.addFunction("tan", Tan);
            }

            public static int Sin(int argCount, ValueStack stack)
            {
                double v = stack.pop().getDouble();
                stack.push(Math.Sin(v));
                return 1;
            }

            public static int Cos(int argCount, ValueStack stack)
            {
                double v = stack.pop().getDouble();
                stack.push(Math.Cos(v));
                return 1;
            }

            public static int Tan(int argCount, ValueStack stack)
            {
                double v = stack.pop().getDouble();
                stack.push(Math.Tan(v));
                return 1;
            }

            public static int angToRad(int argCount, ValueStack stack)
            {
                double v = stack.pop().getDouble();
                double r = v * Math.PI / (double)180.0;

                stack.push(r);
                return 1;
            }
        }
    }

    public class Executor
    {
        public enum Error
        {
            NO_ERROR = 0,
            SYNTAX_ERROR = 1,
            RUNTIME_ERROR = 2,
        }

        private MyGrammar mGrammar;
        private Parser mParser;
        private Evaluator mEval;

        public Parser parser { get { return mParser; } }
        public Evaluator evaluator { get { return mEval; } }

        private ParseTree mParseTree;
        public ParseTree parseTree { get { return mParseTree; } }

        public Executor()
        {
            mGrammar = new MyGrammar();
            mParser = new Parser(mGrammar);
            mEval = new Evaluator();

            Evaluator.BuiltinFuncs.attachTo(mEval);
            Evaluator.BuiltinMath.attachTo(mEval);
        }

        public Error eval(String src)
        {
            mParseTree = mParser.Parse(src);

            //String s = parseTree.ToXml();
            //Console.Write(s);

            if (parseTree.HasErrors())
            {
                return Error.SYNTAX_ERROR;
            }

            if (!mEval.evaluate(mParseTree))
            {
                return Error.RUNTIME_ERROR;
            }

            return Error.NO_ERROR;
        }

        public void addFunction(String name, Evaluator.Func func)
        {
            mEval.addFunction(name, func);
        }

        public List<Evaluator.Value> getOutput()
        {
            return mEval.getOutput();
        }
    }
}