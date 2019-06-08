# ExpressionParser

## Overview

実行時の速度を重視した式解釈の仕組み。

外部から関数を追加が可能。

最初に構文木を作る時以外はアロケートは一切入りません。

A system of expression interpretation that emphasizes the speed at execution time.

External functions can be added.

It does not contain any allocate except when creating the syntax tree first.

## Description

| Type | Description | Example |
| ---- | ---- | ---- |
| Value type | int | 10, 0xA |
| String type | string | "str" |
| Operator | +,-,*,/,%,!,(,) | (-1 + 2) * 3 |
| Comparative expressions | ==,! =, <, <=,>,> = | 2 < 4 |

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
