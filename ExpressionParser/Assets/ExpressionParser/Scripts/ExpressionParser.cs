using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ExpressionParser {
	/// <summary>
	/// 独自関数の定義
	/// </summary>
	/// <param name="args"></param>
	/// <param name="argc"></param>
	/// <returns></returns>
	public delegate ExpressionValue ExpressionParserFunc(List<ExpressionValue> args, int argc);

	public class ExpressionParser {
		private enum CommandType {
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

			Bool,

			CallFunc
		}

		private static readonly Regex REG_RESERVE = new(@"(==|!=|<=|>=|&&|\|\||[+\-/*()%<>!,])");
		private static readonly Regex REG_DEC_NUMBER = new(@"^\d+$");
		private static readonly Regex REG_HEX_NUMBER = new(@"^0[xX][\dabcdefABCDEF]+$");
		private static readonly Regex REG_STRING = new(@"^""[^""]+""$");
		private static readonly Regex REG_VARIABLE_NAME = new(@"^[^\d][\w_]*$");
		private static readonly Regex REG_BOOL = new(@"true|True|TRUE|false|False|FALSE");

		private readonly List<ExpressionValue> _argList = new();
		private readonly Stack<ExpressionValue> _calcStack = new();
		private readonly List<CommandType> _commandList = new();
		private readonly Dictionary<string, ExpressionParserFunc> _funcTable = new();
		private readonly Stack<int> _parameterCounter = new();
		private readonly List<ExpressionValue> _valueList = new();
		private string _expr = "";
		private int _pos;
		private string[] _words;

		/// <summary>
		/// 値を返す
		/// </summary>
		public ExpressionValue Result {
			get {
				if (_calcStack.Count == 1) return _calcStack.Peek();

				return ExpressionValue.None;
			}
		}

		/// <summary>
		/// 式が変化したら構文木を構築し、値を返す
		/// </summary>
		public ExpressionValue Parse(string expr) {
			Profile.Begin("ExpressionParser.Parse");
			if (_expr != expr) Build(expr);

			var valueIndex = 0;

			_calcStack.Clear();

			try {
				foreach (var command in _commandList)
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

						case CommandType.Bool: {
							Profile.Begin("Bool");
							_calcStack.Push(_valueList[valueIndex++]);
							Profile.End();
							break;
						}

						case CommandType.CallFunc: {
							//Profile.Begin("arg");
							var info = _valueList[valueIndex++];
							if (info.ArgCount < _argList.Count) {
								Profile.Begin("CallFunc");

								for (var i = info.ArgCount - 1; 0 <= i; --i) _argList[i] = _calcStack.Pop();

								Profile.Begin("call");
								var ret = info.func(_argList, info.ArgCount);
								Profile.End();

								_calcStack.Push(ret);

								Profile.End();
							}
							else {
								throw new Exception($"引数が多すぎます Capacity:{_argList.Count}");
							}

							break;
						}
					}

				if (_calcStack.Count != 1) throw new Exception("解析に失敗しました");
			}
			catch (Exception e) {
				Debug.Log(e.Message);
				_calcStack.Clear();
			}

			Profile.End();
			return Result;
		}

		/// <summary>
		/// 独自関数の登録
		/// </summary>
		/// <param name="funcName"></param>
		/// <param name="func"></param>
		public void AddCustomFunc(string funcName, ExpressionParserFunc func) {
			_funcTable.Add(funcName.ToLower(), func);
		}

		/// <summary>
		/// 構文木構築
		/// </summary>
		/// <param name="expr"></param>
		private void Build(string expr) {
			_expr = expr;
			_words = null;
			_pos = 0;
			_commandList.Clear();
			_valueList.Clear();

			_calcStack.Clear();

			_argList.Clear();
			for (var i = 0; i < 10; ++i) _argList.Add(ExpressionValue.None);

			_words = REG_RESERVE.Split(_expr).Select(str => str.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();

			try {
				while (_pos < _words.Length) {
					var old = _pos;

					ParseExpr();

					if (old == _pos) throw new Exception("解析失敗。posが進んでいません");
				}
			}
			catch (Exception e) {
				Debug.Log(e.Message);
				_commandList.Clear();
			}
		}

		private void CallExprFunc(Action func, CommandType type, string errorMessage) {
			// ポインタを一つ進めて
			++_pos;
			if (_pos < _words.Length) {
				// 引数を積む
				func();
				// 式を積む
				_commandList.Add(type);
			}
			else {
				throw new Exception(errorMessage);
			}
		}

		// <expr>   ::= <term2> | [ ( '&&' | '||' ) <term2> ] *
		private void ParseExpr() {
			ParseTerm2();

			while (_pos < _words.Length) {
				var code = _words[_pos];
				if (code == "&&")
					CallExprFunc(ParseTerm2, CommandType.And, "&& の右側の式に問題があります");
				else if (code == "||")
					CallExprFunc(ParseTerm2, CommandType.Or, "|| の右側の式に問題があります");
				else
					break;
			}
		}

		// <term2>   ::= <term> | [ ('+' | '-') <term> ] *
		private void ParseTerm2() {
			ParseTerm();

			while (_pos < _words.Length) {
				var code = _words[_pos];
				if (code == "+")
					CallExprFunc(ParseTerm, CommandType.AddOp, "+ の右側の式に問題があります");
				else if (code == "-")
					CallExprFunc(ParseTerm, CommandType.SubOp, "- の右側の式に問題があります");
				else
					break;
			}
		}

		// <term>   ::= <comp> | [ ('*' | '/' | '%') <comp> ]*
		private void ParseTerm() {
			ParseComp();

			while (_pos < _words.Length) {
				var code = _words[_pos];
				if (code == "*")
					CallExprFunc(ParseComp, CommandType.MulOp, "* の右側の式に問題があります");
				else if (code == "/")
					CallExprFunc(ParseComp, CommandType.DivOp, "/ の右側の式に問題があります");
				else if (code == "%")
					CallExprFunc(ParseComp, CommandType.ModOp, "% の右側の式に問題があります");
				else
					break;
			}
		}


		// <comp>   ::= <factor> | ( '==' | '!=' | '<' | '>' | '<=' | '>=' ) <factor>
		private void ParseComp() {
			ParseFactor();

			if (_pos < _words.Length) {
				if (_words[_pos] == "==")
					CallExprFunc(ParseFactor, CommandType.EQ, "== の右側の式に問題があります");
				else if (_words[_pos] == "!=")
					CallExprFunc(ParseFactor, CommandType.NEQ, "!= の右側の式に問題があります");
				else if (_words[_pos] == "<")
					CallExprFunc(ParseFactor, CommandType.LT, "< の右側の式に問題があります");
				else if (_words[_pos] == ">")
					CallExprFunc(ParseFactor, CommandType.GT, "> の右側の式に問題があります");
				else if (_words[_pos] == "<=")
					CallExprFunc(ParseFactor, CommandType.LEQ, "<= の右側の式に問題があります");
				else if (_words[_pos] == ">=") CallExprFunc(ParseFactor, CommandType.GEQ, ">= の右側の式に問題があります");
			}
		}

		// <factor> ::= <value> | '(' <expr> ')' | '!' <expr> 
		private void ParseFactor() {
			var code = _words[_pos];

			if (code == "(") {
				++_pos;

				ParseExpr();

				if (_pos < _words.Length && _words[_pos] == ")")
					++_pos;
				else
					throw new Exception("カッコが閉じられていません");

				//} else if (code == "!") {
				//    callExprFunc(parseExpr, CommandType.Not, "否定式に問題があります");
			}
			else {
				ParseValue();
			}
		}

		// <value> ::= <value2> | ('+' | '-' | '!') <factor>
		private void ParseValue() {
			if (_pos < _words.Length && _words[_pos] == "+") {
				++_pos;
				ParseFactor();
			}
			else if (_pos < _words.Length && _words[_pos] == "-") {
				++_pos;
				ParseFactor();
				_commandList.Add(CommandType.PrevMinOp);
			}
			else if (_pos < _words.Length && _words[_pos] == "!") {
				++_pos;
				ParseFactor();
				_commandList.Add(CommandType.Not);
			}
			else {
				ParseValue2();
			}
		}

		// <value> ::= <number> | <string> | <func_call>
		private void ParseValue2() {
			var code = _words[_pos];

			if (REG_STRING.IsMatch(code))
				ParseString();
			else if (REG_BOOL.IsMatch(code))
				ParseBool();
			else if (REG_VARIABLE_NAME.IsMatch(code))
				ParseFuncCall();
			else
				ParseNumber();
		}

		// <number> :== 1つ以上の数字
		private void ParseNumber() {
			var code = _words[_pos];
			if (REG_DEC_NUMBER.IsMatch(code)) {
				++_pos;
				_commandList.Add(CommandType.Value);
				_valueList.Add(new ExpressionValue(Convert.ToInt32(code)));
			}
			else if (REG_HEX_NUMBER.IsMatch(code)) {
				++_pos;
				_commandList.Add(CommandType.Value);
				_valueList.Add(new ExpressionValue(Convert.ToInt32(code.Substring(2, code.Length - 2), 16)));
			}
			else {
				throw new Exception("値ではありません str:" + code);
			}
		}

		// <string> ::= '"' str '"'
		private void ParseString() {
			// parseValueで記述チェック済みなのでノーチェック
			var code = _words[_pos++];
			_commandList.Add(CommandType.Value);
			_valueList.Add(new ExpressionValue(code.Substring(1, code.Length - 2)));
		}

		// <bool> ::= true(True, TRUE) | false(False, FALSE)
		private void ParseBool() {
			var code = _words[_pos++];
			_commandList.Add(CommandType.Bool);
			_valueList.Add(new ExpressionValue(Convert.ToBoolean(code)));
		}

		// <func_call> ::= <variableName> '(' <parameter_list> ')'
		private void ParseFuncCall() {
			var funcName = _words[_pos++];

			if (!REG_VARIABLE_NAME.IsMatch(funcName)) throw new Exception("関数名に問題があります FuncName:" + funcName);

			if (_words.Length <= _pos) throw new Exception("関数の開始カッコがありません FuncName:" + funcName);

			if (_pos < _words.Length && _words[_pos++] == "(") {
				_parameterCounter.Push(0);

				ParseParameterList();

				var count = _parameterCounter.Pop();

				if (_pos < _words.Length && _words[_pos++] == ")") {
					if (_funcTable.TryGetValue(funcName.ToLower(), out var func)) {
						_commandList.Add(CommandType.CallFunc);
						_valueList.Add(new ExpressionValue(func, count));
					}
					else {
						throw new Exception("未登録の関数です FuncName:" + funcName);
					}
				}
				else {
					throw new Exception("関数がカッコで閉じていません FuncName:" + funcName);
				}
			}
			else {
				throw new Exception("関数の記述に問題があります FuncName:" + funcName);
			}
		}

		// 可変長引数は値を積むだけ
		// <parameter_list> :== empty | <expr> | [ ',' <parameter_list> ] * 
		private void ParseParameterList() {
			if (_words[_pos] == ")") return;

			ParseExpr();

			var count = _parameterCounter.Pop();
			_parameterCounter.Push(count + 1);

			if (_pos < _words.Length) {
				var code = _words[_pos];
				if (code == ",") {
					++_pos;

					ParseParameterList();
				}
			}
		}
	}
}
