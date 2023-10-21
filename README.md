# ExpressionParser

## Overview

式を評価します。ランタイムでの条件判定や、動的な計算式の実装に使用することが出来ます。

最初に構文木を作る時以外はアロケートが入らず、実行速度はLuaの10倍以上高速です。

独自の関数を追加することが出来ます。

例えば以下のような用途に向いています。

- 条件チェックを毎フレーム行う
- ダメージ計算式を文字列化して外部に出したい


Evaluates an expression. It can be used to make conditional decisions at runtime and to implement dynamic expressions.

No allocations except when first creating the syntax tree, and execution speed is 10 times faster than Lua.

You can add your own functions.

For example, it is suitable for the following applications

- Perform conditional checks every frame
- To output the damage calculation formula as a string.


## Description

| Type | Description | Example |
| ---- | ---- | ---- |
| Value | int | 10, 0xA |
| Value | bool | true, True, TRUE ... |
| String | string | "str" |
| Operator | +,-,*,/,%,!,(,) | (-1 + 2) * 3 |
| Comparative expressions | ==,!=,<,<=,>,>= | 2 < 4 |

## Example 

    using expression_parser;
    
    public class CalcTest : MonoBehaviour
    {
        public ExampleParser parser = new ExampleParser();
    
        private void OnGUI()
        {
            var ret = parser.Parse("SUM(1, 2) == 3");
            GUILayout.Label(string.Format("Ans:{0}", ret.intValue));
        }
    }
    
    public class ExampleParser : ExpressionParser
    {
        public ExampleParser()
        {
            RegistFunc("Sum", SumFunc);  // Add Custom Function
        }
    
        public ExpressionValue SumFunc(List<ExpressionValue> args, int argc)
        {
            int total = 0;
            for (int i = 0; i < argc; ++i) {
                var arg = args[i];
                if (arg.type == ExpressionValue.ValueType.IntValue) {
                    total += arg.intValue;
                }
            }
            return ExpressionValue.Create(total);
        }
    }
