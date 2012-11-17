using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	class Point
	{
		public double X { get; set; }
		public double Y { get; set; }

		public Point() { }

		public Point(double x, double y)
		{
			X = x;
			Y = y;
		}
	}
}
