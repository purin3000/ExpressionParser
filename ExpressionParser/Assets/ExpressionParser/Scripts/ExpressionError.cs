using System;
using System.Collections.Generic;

namespace ExpressionParser {
	public enum ErrorCode {
		ParseError = 1,
		ArgumentCapacity = 2,
		PosError = 3,
		CodeString = 4,
		ParenthesesError = 5,

		NotValue = 6,
		FuncName = 7,
		FuncParenthesesError = 8,
		FuncEndParenthesesError = 9,
		UnknownFunc = 10,

		FuncError = 11,
		OpAddError = 12,
		OpSubError = 13,
		OpMulError = 14,
		OpDivError = 15,

		OpModError = 16,
		OpLT = 17,
		OpGT = 18,
		OpLE = 19,
		OpGE = 20,

		OpEQ = 21,
		OpNEQ = 22,
		OpTrue = 23,
		OpFalse = 24,
		OpNot = 25,

		OpPreMinus = 26,
		UnknownValueType = 27,
	}

	public class ExpressionError : Exception {
		private static readonly Dictionary<ErrorCode, Func<string, string, string>> MESSAGE_TABLE = new() {
			{ ErrorCode.ParseError, (arg1, arg2) => "解析に失敗しました" },
			{ ErrorCode.ArgumentCapacity, (capacity, arg2) => $"引数が多すぎます Capacity:{capacity}" },
			{ ErrorCode.PosError, (arg1, arg2) => "解析失敗。posが進んでいません" },
			{ ErrorCode.CodeString, (codeString, arg2) => $"{codeString}の右側の式に問題があります" },
			{ ErrorCode.ParenthesesError, (arg1, arg2) => "カッコが閉じられていません" },

			{ ErrorCode.NotValue, (arg1, arg2) => $"値ではありません str:{arg1}" },
			{ ErrorCode.FuncName, (funcName, arg2) => $"関数名に問題があります FuncName:{funcName}" },
			{ ErrorCode.FuncParenthesesError, (funcName, arg2) => $"関数の開始カッコがありません FuncName:{funcName}" },
			{ ErrorCode.FuncEndParenthesesError, (funcName, arg2) => $"関数がカッコで閉じられていません FuncName:{funcName}" },
			{ ErrorCode.UnknownFunc, (funcName, arg2) => $"未登録の関数です FuncName:{funcName}" },

			{ ErrorCode.FuncError, (funcName, arg2) => $"関数の記述に問題があります FuncName:{funcName}" },
			{ ErrorCode.OpAddError, (left, right) => ExprErrorMessage("+", left, right) },
			{ ErrorCode.OpSubError, (left, right) => ExprErrorMessage("-", left, right) },
			{ ErrorCode.OpMulError, (left, right) => ExprErrorMessage("*", left, right) },
			{ ErrorCode.OpDivError, (left, right) => ExprErrorMessage("/", left, right) },

			{ ErrorCode.OpModError, (left, right) => ExprErrorMessage("%", left, right) },
			{ ErrorCode.OpLT, (left, right) => ExprErrorMessage("<", left, right) },
			{ ErrorCode.OpGT, (left, right) => ExprErrorMessage(">", left, right) },
			{ ErrorCode.OpLE, (left, right) => ExprErrorMessage("<=", left, right) },
			{ ErrorCode.OpGE, (left, right) => ExprErrorMessage(">=", left, right) },

			{ ErrorCode.OpEQ, (left, right) => ExprErrorMessage("==", left, right) },
			{ ErrorCode.OpNEQ, (left, right) => ExprErrorMessage("!=", left, right) },
			{ ErrorCode.OpTrue, (left, right) => ExprErrorMessage("true", left, right) },
			{ ErrorCode.OpFalse, (left, right) => ExprErrorMessage("false", left, right) },
			{ ErrorCode.OpNot, (left, right) => ExprErrorMessage("!", left, right) },

			{ ErrorCode.OpPreMinus, (left, right) => ExprErrorMessage("-", left, right) },
			{ ErrorCode.UnknownValueType, (arg1, arg2) => $"未知のタイプです {arg1}" },
		};

		private static string ExprErrorMessage(string op, string left, string right) => left != null
			? $"計算できない式です op:{op} left:{left} right:{right}"
			: $"計算できない式です op:{op} right:{right}";

		public ExpressionError(ErrorCode errorCode, string arg1 = null) : base(
			MESSAGE_TABLE[errorCode].Invoke(arg1, null)) {
		}

		public ExpressionError(ErrorCode errorCode, ValueType left, ValueType right) : base(
			MESSAGE_TABLE[errorCode].Invoke(left.ToString(), right.ToString())) {
		}

		public ExpressionError(ErrorCode errorCode, ValueType right) : base(
			MESSAGE_TABLE[errorCode].Invoke(null, right.ToString())) {
		}
	}
}
