using System.Collections.Generic;
using ExpressionParser;
using UnityEngine;

public class SpeedTest : MonoBehaviour
{
	private readonly ExampleParser parser = new();

	public int loopCount = 1000;

	public string expr = "SUM(1, 2) == 3";

	private void Update()
	{
		for (int i = 0; i < loopCount; ++i)
		{
			parser.Parse(expr);
		}
	}

	private void OnGUI()
	{
		var ret = parser.Parse(expr);
		GUILayout.Label($"Ans:{ret.IntValue}");
	}
}

public class ExampleParser : ExpressionParser.ExpressionParser
{
	public ExampleParser()
	{
		// Add Custom Function
		AddCustomFunc("Sum", SumFunc);
	}

	public ExpressionValue SumFunc(List<ExpressionValue> args, int argc)
	{
		Profile.Begin("SumFunc");
		int total = 0;
		for (int i = 0; i < argc; ++i)
		{
			var arg = args[i];
			if (arg.Type == ExpressionValue.ValueType.IntValue)
			{
				total += arg.IntValue;
			}
		}

		Profile.End();
		return new ExpressionValue(total);
	}
}
