using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	class Location : Ray
	{
		public double Width { get; set; }
		public double Height { get; set; }

		public Location(double x, double y, double angle, double width, double height)
			: base(x, y, angle)
		{
			Width = width;
			Height = height;
		}
	}
}
