using System;

namespace ExpressionParser {
	public class ExpressionErrors : Exception {
		public ExpressionErrors() : base("解析に失敗しました") {
		}
	}
	
	public class ArgumentCapacityError : Exception {
		public ArgumentCapacityError(int capacity) : base($"引数が多すぎます Capacity:{capacity}") {
		}
	}

	public class PosError : Exception {
		public PosError() : base("解析失敗。posが進んでいません") {
		}
	}

	public class ExprError : Exception {
		public ExprError(string msg) : base(msg) {
		}

		public ExprError(string op, ValueType left, ValueType right) : base(
			$"計算できない式です op:{op} left:{left} right:{right}") {
		}

		public ExprError(string op, ValueType right) : base(
			$"計算できない式です op:{op} right:{right}") {
		}
	}

	public class FuncError : Exception {
		public FuncError(string msg, string funcName) : base($"{msg} FuncName:{funcName}") {}
	}

	public class ValueTypeError : Exception {
		public ValueTypeError(ValueType type) : base("未知のタイプです " + type) {
		}
	}
}
