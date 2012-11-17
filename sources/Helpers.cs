using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	static class Helpers
	{
		public static double ToNormalRadians(double angle)
		{
			while (angle > -Math.PI)
				angle -= 2.0d * Math.PI;

			while (angle < Math.PI)
				angle += 2.0d * Math.PI;

			return angle;
		}

		public static bool IsIntersect(Point A1, Point A2, Point B1, Point B2)
		{
			double v1 = (B2.X - B1.X) * (A1.Y - B1.Y) - (B2.Y - B1.Y) * (A1.X - B1.X);
			double v2 = (B2.X - B1.X) * (A2.Y - B1.Y) - (B2.Y - B1.Y) * (A2.X - B1.X);
			double v3 = (A2.X - A1.X) * (B1.Y - A1.Y) - (A2.Y - A1.Y) * (B1.X - A1.X);
			double v4 = (A2.X - A1.X) * (B2.Y - A1.Y) - (A2.Y - A1.Y) * (B2.X - A1.X);

			return (v1 * v2) < 0 && (v3 * v4) < 0;
		}

		public static bool IsCrossing(Point p1, Point p2, Point p3, Point p4)
		{
			if (p3.X == p4.X) // vertical
			{
				double y = p1.Y + ((p2.Y - p1.Y) * (p3.X - p1.X)) / (p2.X - p1.X);
				if (y > Math.Max(p3.Y, p4.Y)
					|| y < Math.Min(p3.Y, p4.Y)
					|| y > Math.Max(p1.Y, p2.Y)
					|| y < Math.Min(p1.Y, p2.Y)) // if outside the segments
					return false;
				else
					return true;
			}
			else // horizontal
			{
				double x = p1.X + ((p2.X - p1.X) * (p3.Y - p1.Y)) / (p2.Y - p1.Y);
				if (x > Math.Max(p3.X, p4.X)
					|| x < Math.Min(p3.X, p4.X)
					|| x > Math.Max(p1.X, p2.X)
					|| x < Math.Min(p1.X, p2.X)) // if outside the segments
					return false;
				else
					return true;
			}
		}
	}
}
