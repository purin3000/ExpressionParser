# ExpressionParser

## Overview

実行時の速度を重視した式解釈の仕組み。Luaの10倍以上早いはず。

外部から関数を追加が可能で、整数もしくは文字列を返すことができます。

最初に構文木を作る時以外はヒープのアロケートが入りません。

以下のような用途に向いています。

- 発生条件チェックを毎フレーム行う
- ダメージ計算式を文字列化して外部に出したい



A system of expression interpretation that emphasizes the speed at execution time. Should be 10 times faster than Lua.

External functions can be added and can return integers or strings.

There is no heap allocation except when you first create a parse tree.

It is suitable for the following applications.

-Perform an occurrence condition check every frame
-I want to convert the damage formula into a string and put it out

## Description

| Type | Description | Example |
| ---- | ---- | ---- |
| Value type | int | 10, 0xA |
| String type | string | "str" |
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
