//#define PROFILE_ENABLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace expression_parser
{
    public class Profile
    {
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
        public static void Begin(string label)
        {
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void End()
        {
        }
#endif

    }

    public readonly struct ExpressionValue
    {
        public enum ValueType
        {
            None,
            IntValue,
            StringValue,
            FuncName,
        }

        readonly int _intValue;
        readonly object _objValue;
        readonly ValueType _type;

        public ValueType type => _type;

        public int argCount => _intValue;

        public int intValue => _intValue;

        public string stringValue => (string)_objValue;

        public ExpressionParserFunc func => (ExpressionParserFunc)_objValue;

        public static readonly ExpressionValue None = new ExpressionValue(ValueType.None, 0, string.Empty);

        public ExpressionValue(ValueType type, int intVal, object objVal)
        {
            _type = type;
            _intValue = intVal;
            _objValue = objVal;
        }

        public ExpressionValue(bool val)
        {
            _type = ValueType.IntValue;
            _intValue = val ? 1 : 0;
            _objValue = string.Empty;
        }

        public ExpressionValue(int val)
        {
            _type = ValueType.IntValue;
            _intValue = val;
            _objValue = string.Empty;
        }

        public ExpressionValue(string str)
        {
            _type = ValueType.StringValue;
            _intValue = 0;
            _objValue = str;
        }

        public ExpressionValue(ExpressionParserFunc func, int argCount)
        {
            _type = ValueType.FuncName;
            _intValue = argCount;
            _objValue = func;
        }

        public override string ToString()
        {
            switch (type) {
            case ValueType.IntValue:
                return "int:" + intValue;

            case ValueType.StringValue:
                return "string:" + stringValue;

            case ValueType.FuncName:
                return string.Format("func:{0} argc:{1}", func, argCount);

            case ValueType.None:
                return "type:None";

            default:
                throw new System.Exception("未知のタイプです " + type);
            }
        }

        public static explicit operator bool(ExpressionValue right)
        {
            if (right.type == ValueType.IntValue) {
                return right.intValue != 0;
            }
            if (right.type == ValueType.StringValue) {
                return !string.IsNullOrEmpty(right.stringValue);
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is ExpressionValue) {
                ExpressionValue dst = (ExpressionValue)obj; //boxing発生するが仕方がない
                return (type == dst.type && intValue == dst.intValue && stringValue == dst.stringValue && argCount == dst.argCount);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static ExpressionValue operator !(ExpressionValue right)
        {
            if (right.type == ValueType.IntValue) return new ExpressionValue(right.intValue == 0);
            throw new System.Exception(string.Format("計算できない式です op:! right:{0}", right));
        }

        public static ExpressionValue operator-(ExpressionValue right)
        {
            if (right.type == ValueType.IntValue) {
                return new ExpressionValue(-right.intValue);
            }
            throw new System.Exception(string.Format("計算できない式です op:- right:{0}", right));
        }

        public static ExpressionValue operator +(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue + right.intValue);
            }

            if (left.type == ValueType.StringValue && right.type == ValueType.StringValue) {
                return new ExpressionValue(left.stringValue + right.stringValue);
            }

            if (left.type == ValueType.StringValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.stringValue + right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:+ left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator -(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue - right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:- left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator *(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue * right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:* left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator /(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue / right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:/ left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator %(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue % right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:% left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator <(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue < right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:< left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator >(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue > right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:> left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator <=(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue <= right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:<= left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator >=(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue >= right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:>= left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator ==(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue == right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:== left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator !=(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return new ExpressionValue(left.intValue != right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:!= left:{0} right:{1}", left, right));
        }

        public static bool operator true(ExpressionValue right)
        {
            if (right.type == ValueType.IntValue) return right.intValue != 0;
            if (right.type == ValueType.StringValue) return !string.IsNullOrEmpty(right.stringValue);
            throw new System.Exception(string.Format("計算できない式です op:true right:{0}", right));
        }

        public static bool operator false(ExpressionValue right)
        {
            if (right.type == ValueType.IntValue) return right.intValue == 0;
            if (right.type == ValueType.StringValue) return string.IsNullOrEmpty(right.stringValue);
            throw new System.Exception(string.Format("計算できない式です op:false right:{0}", right));
        }

        public static ExpressionValue operator &(ExpressionValue left, ExpressionValue right)
        {
            return new ExpressionValue((bool)left && (bool)right);
        }

        public static ExpressionValue operator |(ExpressionValue left, ExpressionValue right)
        {
            return new ExpressionValue((bool)left || (bool)right);
        }
    }
}
