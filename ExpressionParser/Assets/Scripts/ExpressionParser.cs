using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

using expression_parser;

namespace expression_parser
{
    /// <summary>
    /// Custom Function
    /// </summary>
    /// <param name="args"></param>
    /// <param name="argc"></param>
    /// <returns></returns>
    public delegate ExpressionValue ExpressionParserFunc(List<ExpressionValue> args, int argc);
}

public class ExpressionParser
{
    string _expr = "";
    string[] _words;
    int _pos;

    List<CommandType> _commandList = new List<CommandType>();
    List<ExpressionValue> _valueList = new List<ExpressionValue>();
    Stack<int> _parameterCounter = new Stack<int>();

    Stack<ExpressionValue> _calcStack = new Stack<ExpressionValue>();
    List<ExpressionValue> _argList = new List<ExpressionValue>();

    Dictionary<string, ExpressionParserFunc> _funcs = new Dictionary<string, ExpressionParserFunc>();

    readonly static Regex regReserve = new Regex(@"(==|!=|<=|>=|&&|\|\||[+\-/*()%<>!,])");
    readonly static Regex regDecNumber = new Regex(@"^\d+$");
    readonly static Regex regHexNumber = new Regex(@"^0[xX][\dabcdefABCDEF]+$");
    readonly static Regex regString = new Regex(@"^""[^""]+""$");
    readonly static Regex regVariableName = new Regex(@"^[^\d][\w_]*$");

    enum CommandType
    {
        None,

        Value,
        AddOp,
        SubOp,
        MulOp,
        DivOp,
        ModOp,
        PrevMinOp,

        EQ,
        NEQ,
        GT,
        GEQ,
        LT,
        LEQ,

        And,
        Or,
        Not,

        CallFunc,
    }

    /// <summary>
    /// 値を返す
    /// </summary>
    public ExpressionValue result
    {
        get {
            if (_calcStack.Count == 1) {
                return _calcStack.Peek();
            }
            return ExpressionValue.None;
        }
    }

    /// <summary>
    /// 式が変化したら構文木を構築し、値を返す
    /// </summary>
    public ExpressionValue Parse(string expr)
    {
        Profile.Begin("ExpressionParser.Parse");
        if (_expr != expr) {
            build(expr);
        }

        int valueIndex = 0;

        _calcStack.Clear();

        try {

            foreach (var command in _commandList) {
                switch (command) {
                case CommandType.Value: {
                        Profile.Begin("Value");
                        _calcStack.Push(_valueList[valueIndex++]);
                        Profile.End();
                        break;
                    }

                case CommandType.AddOp: {
                        Profile.Begin("AddOp");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left + right);
                        Profile.End();
                        break;
                    }

                case CommandType.SubOp: {
                        Profile.Begin("SubOp");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left - right);
                        Profile.End();
                        break;
                    }

                case CommandType.MulOp: {
                        Profile.Begin("MulOp");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left * right);
                        Profile.End();
                        break;
                    }

                case CommandType.DivOp: {
                        Profile.Begin("DivOp");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left / right);
                        Profile.End();
                        break;
                    }

                case CommandType.ModOp: {
                        Profile.Begin("ModOp");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left % right);
                        Profile.End();
                        break;
                    }

                case CommandType.PrevMinOp: {
                        Profile.Begin("PrevMinOp");
                        var right = _calcStack.Pop();
                        _calcStack.Push(-right);
                        Profile.End();
                        break;
                    }

                case CommandType.EQ: {
                        Profile.Begin("EQ");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left == right);
                        Profile.End();
                        break;
                    }

                case CommandType.NEQ: {
                        Profile.Begin("NEQ");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left != right);
                        Profile.End();
                        break;
                    }

                case CommandType.LT: {
                        Profile.Begin("LT");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left < right);
                        Profile.End();
                        break;
                    }

                case CommandType.LEQ: {
                        Profile.Begin("LEQ");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left <= right);
                        Profile.End();
                        break;
                    }

                case CommandType.GT: {
                        Profile.Begin("GT");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left > right);
                        Profile.End();
                        break;
                    }

                case CommandType.GEQ: {
                        Profile.Begin("GEQ");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left >= right);
                        Profile.End();
                        break;
                    }

                case CommandType.And: {
                        Profile.Begin("And");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left && right);
                        Profile.End();
                        break;
                    }

                case CommandType.Or: {
                        Profile.Begin("Or");
                        var right = _calcStack.Pop();
                        var left = _calcStack.Pop();
                        _calcStack.Push(left || right);
                        Profile.End();
                        break;
                    }

                case CommandType.Not: {
                        Profile.Begin("Not");
                        var right = _calcStack.Pop();
                        _calcStack.Push(!right);
                        Profile.End();
                        break;
                    }

                case CommandType.CallFunc: {
                        //Profile.Begin("arg");
                        var info = _valueList[valueIndex++];
                        if (info.argCount < _argList.Count) {
                            Profile.Begin("CallFunc");

                            for (int i = info.argCount - 1; 0 <= i; --i) {
                                _argList[i] = _calcStack.Pop();
                            }

                            Profile.Begin("call");
                            var ret = info.func(_argList, info.argCount);
                            Profile.End();

                            _calcStack.Push(ret);

                            Profile.End();
                        } else {
                            throw new System.Exception(string.Format("引数が多すぎます Capacity:{0}", _argList.Count));
                        }
                        break;
                    }
                }
            }

            if (_calcStack.Count != 1) {
                throw new System.Exception("解析に失敗しました");
            }
        }
        catch (System.Exception e) {
            Debug.Log(e.Message);
            _calcStack.Clear();
        }

        Profile.End();
        return result;
    }

    public void RegistFunc(string funcName, ExpressionParserFunc func)
    {
        _funcs.Add(funcName.ToLower(), func);
    }

    /// <summary>
    /// 構文木構築
    /// </summary>
    /// <param name="expr"></param>
    void build(string expr)
    {
        _expr = expr;
        _words = null;
        _pos = 0;
        _commandList.Clear();
        _valueList.Clear();

        _calcStack.Clear();

        _argList.Clear();
        for (int i=0 ; i<10 ; ++i) {
            _argList.Add(ExpressionValue.None);
        }

        _words = regReserve.Split(_expr).Select(str => str.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

        try {
            while (_pos < _words.Length) {
                var old = _pos;

                parseExpr();

                if (old == _pos) {
                    throw new System.Exception("解析失敗。posが進んでいません");
                }
            }
        }
        catch (System.Exception e) {
            Debug.Log(e.Message);
            _commandList.Clear();
        }
    }

    void callExprFunc(System.Action func, CommandType type, string errorMessage)
    {
        // ポインタを一つ進めて
        ++_pos;
        if (_pos < _words.Length) {
            // 引数を積む
            func();
            // 式を積む
            _commandList.Add(type);
        } else {
            throw new System.Exception(errorMessage);
        }
    }

    // <expr>   ::= <term2> | [ ( '&&' | '||' ) <term2> ] *
    void parseExpr()
    {
        parseTerm2();

        while (_pos < _words.Length) {
            var code = _words[_pos];
            if (code == "&&") {
                callExprFunc(parseTerm2, CommandType.And, "&& の右側の式に問題があります");
            } else if (code == "||") {
                callExprFunc(parseTerm2, CommandType.Or, "|| の右側の式に問題があります");
            } else {
                break;
            }
        }
    }

    // <term2>   ::= <term> | [ ('+' | '-') <term> ] *
    void parseTerm2()
    {
        parseTerm();

        while (_pos < _words.Length) {
            var code = _words[_pos];
            if (code == "+") {
                callExprFunc(parseTerm, CommandType.AddOp, "+ の右側の式に問題があります");
            } else if (code == "-") {
                callExprFunc(parseTerm, CommandType.SubOp, "- の右側の式に問題があります");
            } else {
                break;
            }
        }
    }

    // <term>   ::= <comp> | [ ('*' | '/' | '%') <comp> ]*
    void parseTerm()
    {
        parseComp();

        while (_pos < _words.Length) {
            var code = _words[_pos];
            if (code == "*") {
                callExprFunc(parseComp, CommandType.MulOp, "* の右側の式に問題があります");
            } else if (code == "/") {
                callExprFunc(parseComp, CommandType.DivOp, "/ の右側の式に問題があります");
            } else if (code == "%") {
                callExprFunc(parseComp, CommandType.ModOp, "% の右側の式に問題があります");
            } else {
                break;
            }
        }
    }


    // <comp>   ::= <factor> | ( '==' | '!=' | '<' | '>' | '<=' | '>=' ) <factor>
    void parseComp()
    {
        parseFactor();

        if (_pos < _words.Length) {
            if (_words[_pos] == "==") {
                callExprFunc(parseFactor, CommandType.EQ, "== の右側の式に問題があります");
            } else if (_words[_pos] == "!=") {
                callExprFunc(parseFactor, CommandType.NEQ, "!= の右側の式に問題があります");
            } else if (_words[_pos] == "<") {
                callExprFunc(parseFactor, CommandType.LT, "< の右側の式に問題があります");
            } else if (_words[_pos] == ">") {
                callExprFunc(parseFactor, CommandType.GT, "> の右側の式に問題があります");
            } else if (_words[_pos] == "<=") {
                callExprFunc(parseFactor, CommandType.LEQ, "<= の右側の式に問題があります");
            } else if (_words[_pos] == ">=") {
                callExprFunc(parseFactor, CommandType.GEQ, ">= の右側の式に問題があります");
            }
        }
    }

    // <factor> ::= <value> | '(' <expr> ')' | '!' <expr> 
    void parseFactor()
    {
        var code = _words[_pos];

        if (code == "(") {
            ++_pos;

            parseExpr();

            if (_pos < _words.Length && _words[_pos] == ")") {
                ++_pos;
            } else {
                throw new System.Exception("カッコが閉じられていません");
            }
        } else if (code == "!") {
            callExprFunc(parseExpr, CommandType.Not, "否定式に問題があります");
        } else {
            parseValue();
        }
    }

    // <value> ::= <value2> | ('+' | '-') <value>
    void parseValue()
    {
        if (_pos < _words.Length && _words[_pos] == "+") {
            ++_pos;
            parseValue();

        } else if (_pos < _words.Length && _words[_pos] == "-") {
            ++_pos;
            parseValue();
            _commandList.Add(CommandType.PrevMinOp);

        } else {
            parseValue2();
        }
    }

    // <value> ::= <number> | <string> | <func_call>
    void parseValue2()
    {
        var code = _words[_pos];

        if (regString.IsMatch(code)) {
            parseString();
        } else if (regVariableName.IsMatch(code)) {
            parseFuncCall();
        } else {
            parseNumber();
        }
    }

    // <number> :== 1つ以上の数字
    void parseNumber()
    {
        var code = _words[_pos];
        if (regDecNumber.IsMatch(code)) {
            ++_pos;
            _commandList.Add(CommandType.Value);
            _valueList.Add(new ExpressionValue(System.Convert.ToInt32(code)));

        } else if (regHexNumber.IsMatch(code)) {
            ++_pos;
            _commandList.Add(CommandType.Value);
            _valueList.Add(new ExpressionValue(System.Convert.ToInt32(code.Substring(2, code.Length - 2), 16)));
        } else {
            throw new System.Exception("値ではありません str:" + code);
        }
    }

    // <string> ::= '"' str '"'
    void parseString()
    {
        // parseValueで記述チェック済みなのでノーチェック
        var code = _words[_pos++];
        _commandList.Add(CommandType.Value);
        _valueList.Add(new ExpressionValue(code.Substring(1, code.Length - 2)));
    }

    // <func_call> ::= <variableName> '(' <parameter_list> ')'
    void parseFuncCall()
    {
        var funcName = _words[_pos++];

        if (!regVariableName.IsMatch(funcName)) {
            throw new System.Exception("関数名に問題があります FuncName:" + funcName);
        }

        if (_words.Length <= _pos) {
            throw new System.Exception("関数の開始カッコがありません FuncName:" + funcName);
        }

        if (_pos < _words.Length && _words[_pos++] == "(") {

            _parameterCounter.Push(0);

            parseParameterList();

            var count = _parameterCounter.Pop();

            if (_pos < _words.Length && _words[_pos++] == ")") {
                ExpressionParserFunc func;
                if (_funcs.TryGetValue(funcName.ToLower(), out func)) {
                    _commandList.Add(CommandType.CallFunc);
                    _valueList.Add(new ExpressionValue(func, count));
                } else {
                    throw new System.Exception("未登録の関数です FuncName:" + funcName);
                }
            } else {
                throw new System.Exception("関数がカッコで閉じていません FuncName:" + funcName);
            }

        } else {
            throw new System.Exception("関数の記述に問題があります FuncName:" + funcName);
        }
    }

    // 可変長引数は値を積むだけ
    // <parameter_list> :== empty | <expr> | [ ',' <parameter_list> ] * 
    void parseParameterList()
    {
        if (_words[_pos] == ")") {
            return;
        }

        parseExpr();

        var count = _parameterCounter.Pop();
        _parameterCounter.Push(count + 1);

        if (_pos < _words.Length) {
            var code = _words[_pos];
            if (code == ",") {
                ++_pos;

                parseParameterList();
            }
        }
    }

}
