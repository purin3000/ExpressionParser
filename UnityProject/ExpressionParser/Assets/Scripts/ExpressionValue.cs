#define PROFILE_ENABLE

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
        public static void BeginSample(string label)
        {
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void EndSample()
        {
        }
#endif

    }

    public struct ExpressionValue
    {
        public enum ValueType
        {
            None,
            IntValue,
            StringValue,
            FuncName,
        }

        public static readonly ExpressionValue None = new ExpressionValue();

        public int intValue;
        public object objValue;

        public ValueType type;

        public int argCount { get { return intValue; } }

        public string stringValue { get { return (string)objValue; } }

        public ExpressionParserFunc func { get { return (ExpressionParserFunc)objValue; } }

        public static ExpressionValue Create()
        {
            return None;
        }

        public static ExpressionValue Create(bool val)
        {
            Profile.Begin("Create(bool val)");
            ExpressionValue ret;

            ret.type = ValueType.IntValue;
            ret.intValue = val? 1:0;
            ret.objValue = string.Empty;

            Profile.End();
            return ret;
        }

        public static ExpressionValue Create(int val)
        {
            Profile.Begin("Create(int val)");
            ExpressionValue ret;

            ret.type = ValueType.IntValue;
            ret.intValue = val;
            ret.objValue = string.Empty;

            Profile.End();
            return ret;
        }

        public static ExpressionValue Create(string str)
        {
            Profile.Begin("Create(string str)");
            ExpressionValue ret;

            ret.type = ValueType.StringValue;
            ret.intValue = 0;
            ret.objValue = str;

            Profile.End();
            return ret;
        }

        public static ExpressionValue Create(ExpressionParserFunc func, int argCount)
        {
            Profile.Begin("Create(ExpressionParserFunc func, int argCount)");
            ExpressionValue ret;

            ret.type = ValueType.FuncName;
            ret.intValue = argCount;
            ret.objValue = func;

            Profile.End();
            return ret;
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

        //public override bool Equals(object obj)
        //{
        //    if (obj != null && obj is ExpressionValue) {
        //        ExpressionValue dst = (ExpressionValue)obj; //boxing発生するが仕方がない
        //        return (type == dst.type && intValue == dst.intValue && stringValue == dst.stringValue && argCount == dst.argCount);
        //    }
        //    return base.Equals(obj);
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

        //public static ExpressionValue operator !(ExpressionValue right)
        //{
        //    if (right.type == ValueType.IntValue) return Create(right.intValue == 0? 1:0);
        //    throw new System.Exception(string.Format("計算できない式です op:! right:{0}", right));
        //}

        public static ExpressionValue operator-(ExpressionValue right)
        {
            if (right.type == ValueType.IntValue) {
                return Create(-right.intValue);
            }
            throw new System.Exception(string.Format("計算できない式です op:- right:{0}", right));
        }

        public static ExpressionValue operator +(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return Create(left.intValue + right.intValue);
            }

            if (left.type == ValueType.StringValue && right.type == ValueType.StringValue) {
                return Create(left.stringValue + right.stringValue);
            }

            if (left.type == ValueType.StringValue && right.type == ValueType.IntValue) {
                return Create(left.stringValue + right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:+ left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator -(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return Create(left.intValue - right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:- left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator *(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return Create(left.intValue * right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:* left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator /(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return Create(left.intValue * right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:/ left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator <(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return Create(left.intValue < right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:< left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator >(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return Create(left.intValue > right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:> left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator <=(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return Create(left.intValue <= right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:<= left:{0} right:{1}", left, right));
        }

        public static ExpressionValue operator >=(ExpressionValue left, ExpressionValue right)
        {
            if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
                return Create(left.intValue >= right.intValue);
            }

            throw new System.Exception(string.Format("計算できない式です op:>= left:{0} right:{1}", left, right));
        }

        //public static ExpressionValue operator ==(ExpressionValue left, ExpressionValue right)
        //{
        //    if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
        //        return Create(left.intValue == right.intValue);
        //    }

        //    throw new System.Exception(string.Format("計算できない式です op:== left:{0} right:{1}", left, right));
        //}

        //public static ExpressionValue operator !=(ExpressionValue left, ExpressionValue right)
        //{
        //    if (left.type == ValueType.IntValue && right.type == ValueType.IntValue) {
        //        return Create(left.intValue == right.intValue);
        //    }

        //    throw new System.Exception(string.Format("計算できない式です op:!= left:{0} right:{1}", left, right));
        //}

        // && || 演算子の実装。C#だとtrueやoperator&の実装を行う形になるようだ
        //public static bool operator true(ExpressionValue right)
        //{
        //    if (right.type == ValueType.IntValue) return right.intValue != 0;
        //    if (right.type == ValueType.StringValue) return !string.IsNullOrEmpty(right.stringValue);
        //    throw new System.Exception(string.Format("計算できない式です op:true right:{0}", right));
        //}

        //public static bool operator false(ExpressionValue right)
        //{
        //    if (right.type == ValueType.IntValue) return right.intValue == 0;
        //    if (right.type == ValueType.StringValue) return string.IsNullOrEmpty(right.stringValue);
        //    throw new System.Exception(string.Format("計算できない式です op:false right:{0}", right));
        //}
        //public static ExpressionValue operator &(ExpressionValue left, ExpressionValue right)
        //{
        //    return Create((bool)left && (bool)right);
        //}
        //public static ExpressionValue operator |(ExpressionValue left, ExpressionValue right)
        //{
        //    return Create((bool)left || (bool)right);
        //}
    }
}
