using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using expression_parser;

public class SpeedTest : MonoBehaviour
{
    public ExampleParser parser = new ExampleParser();

    public int loopCount = 1000;

    public string expr = "SUM(1, 2) == 3";

    private void Update()
    {
        for (int i = 0; i < loopCount; ++i) {
            parser.Parse(expr);
        }
    }

    private void OnGUI()
    {
        var ret = parser.Parse(expr);
        GUILayout.Label(string.Format("Ans:{0}", ret.intValue));
    }
}

public class ExampleParser : ExpressionParser
{
    public ExampleParser()
    {
        // Add Custom Function
        RegistFunc("Sum", SumFunc);
    }

    public ExpressionValue SumFunc(List<ExpressionValue> args, int argc)
    {
        Profile.Begin("SumFunc");
        int total = 0;
        for (int i = 0; i < argc; ++i) {
            var arg = args[i];
            if (arg.type == ExpressionValue.ValueType.IntValue) {
                total += arg.intValue;
            }
        }
        Profile.End();
        return ExpressionValue.Create(total);
    }

}

