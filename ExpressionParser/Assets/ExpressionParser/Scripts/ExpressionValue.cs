// #define PROFILE_ENABLE

namespace ExpressionParser {
	public abstract class Profile {
#if PROFILE_ENABLE
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Begin(string label)
		{
			UnityEngine.Profiling.Profiler.BeginSample(label);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void End()
		{
			UnityEngine.Profiling.Profiler.EndSample();
		}

#else
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Begin(string label) {
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void End() {
		}
#endif
	}

	public readonly struct ExpressionValue {
		public enum ValueType {
			None,
			IntValue,
			StringValue,
			FuncName,
		}

		public static readonly ExpressionValue NONE = new(ValueType.None, 0, string.Empty);

		private readonly object _objValue;
		public readonly ValueType Type;
		public readonly int IntValue;

		public int ArgCount => IntValue;
		public string StringValue => (string)_objValue;
		public ExpressionParserFunc Func => (ExpressionParserFunc)_objValue;

		public ExpressionValue(ValueType type, int intVal, object objVal) {
			Type = type;
			IntValue = intVal;
			_objValue = objVal;
		}

		public ExpressionValue(bool val) {
			Type = ValueType.IntValue;
			IntValue = val ? 1 : 0;
			_objValue = string.Empty;
		}

		public ExpressionValue(int val) {
			Type = ValueType.IntValue;
			IntValue = val;
			_objValue = string.Empty;
		}

		public ExpressionValue(string str) {
			Type = ValueType.StringValue;
			IntValue = 0;
			_objValue = str;
		}

		public ExpressionValue(ExpressionParserFunc func, int argCount) {
			Type = ValueType.FuncName;
			IntValue = argCount;
			_objValue = func;
		}

		public override string ToString() {
			switch (Type) {
				case ValueType.IntValue:
					return "int:" + IntValue;

				case ValueType.StringValue:
					return "string:" + StringValue;

				case ValueType.FuncName:
					return $"func:{Func} argc:{ArgCount}";

				case ValueType.None:
					return "type:None";

				default:
					throw new ValueTypeError(Type);
			}
		}

		public static explicit operator bool(ExpressionValue right) {
			if (right.Type == ValueType.IntValue) {
				return right.IntValue != 0;
			}

			if (right.Type == ValueType.StringValue) {
				return !string.IsNullOrEmpty(right.StringValue);
			}

			return false;
		}

		public override bool Equals(object obj) {
			if (obj is ExpressionValue dst) {
				return (Type == dst.Type && IntValue == dst.IntValue && StringValue == dst.StringValue &&
				        ArgCount == dst.ArgCount);
			}

			return base.Equals(obj);
		}

		public override int GetHashCode() => base.GetHashCode();

		public static ExpressionValue operator !(ExpressionValue right) {
			if (right.Type == ValueType.IntValue) {
				return new ExpressionValue(right.IntValue == 0);
			}

			throw new ExprError("!", right);
		}

		public static ExpressionValue operator -(ExpressionValue right) {
			if (right.Type == ValueType.IntValue) {
				return new ExpressionValue(-right.IntValue);
			}

			throw new ExprError("-", right);
		}

		public static ExpressionValue operator +(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue + right.IntValue);
			}

			if (left.Type == ValueType.StringValue && right.Type == ValueType.StringValue) {
				return new ExpressionValue(left.StringValue + right.StringValue);
			}

			if (left.Type == ValueType.StringValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.StringValue + right.IntValue);
			}

			throw new ExprError("+", left, right);
		}

		public static ExpressionValue operator -(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue - right.IntValue);
			}

			throw new ExprError("-", left, right);
		}

		public static ExpressionValue operator *(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue * right.IntValue);
			}

			throw new ExprError("*", left, right);
		}

		public static ExpressionValue operator /(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue / right.IntValue);
			}

			throw new ExprError("/", left, right);
		}

		public static ExpressionValue operator %(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue % right.IntValue);
			}

			throw new ExprError("%", left, right);
		}

		public static ExpressionValue operator <(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue < right.IntValue);
			}

			throw new ExprError("<", left, right);
		}

		public static ExpressionValue operator >(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue > right.IntValue);
			}

			throw new ExprError(">", left, right);
		}

		public static ExpressionValue operator <=(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue <= right.IntValue);
			}

			throw new ExprError("<=", left, right);
		}

		public static ExpressionValue operator >=(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue >= right.IntValue);
			}

			throw new ExprError(">=", left, right);
		}

		public static ExpressionValue operator ==(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue == right.IntValue);
			}

			throw new ExprError("==", left, right);
		}

		public static ExpressionValue operator !=(ExpressionValue left, ExpressionValue right) {
			if (left.Type == ValueType.IntValue && right.Type == ValueType.IntValue) {
				return new ExpressionValue(left.IntValue != right.IntValue);
			}

			throw new ExprError("!=", left, right);
		}

		public static bool operator true(ExpressionValue right) {
			if (right.Type == ValueType.IntValue) return right.IntValue != 0;
			if (right.Type == ValueType.StringValue) return !string.IsNullOrEmpty(right.StringValue);
			throw new ExprError("true", right);
		}

		public static bool operator false(ExpressionValue right) {
			if (right.Type == ValueType.IntValue) return right.IntValue == 0;
			if (right.Type == ValueType.StringValue) return string.IsNullOrEmpty(right.StringValue);
			throw new ExprError("false", right);
		}

		public static ExpressionValue operator &(ExpressionValue left, ExpressionValue right) {
			return new ExpressionValue((bool)left && (bool)right);
		}

		public static ExpressionValue operator |(ExpressionValue left, ExpressionValue right) {
			return new ExpressionValue((bool)left || (bool)right);
		}
	}
}
