using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	static class TankExtensions
	{
		static Point GetExpectedPointForFire(this Tank tank, Tank self, World world)
		{
			double time = self.GetDistanceTo(tank) / Const.BulletSpeed;

			Point expected = new Point(tank.X + tank.SpeedX * time, tank.Y + tank.SpeedY * time);

			return new Point(
				expected.X < 0 ? 0 : (expected.X > world.Width ? world.Width - 1 : expected.X),
				expected.Y < 0 ? 0 : (expected.Y > world.Height ? world.Height - 1 : expected.Y)
			);
		}

		public static T GetExpectedPositionForFire<T>(this Tank tank, Tank self, World world) where T : Point
		{
			Point point = tank.GetExpectedPointForFire(self, world);

			if (typeof(T) == typeof(Point))
				return point as T;
			else if (typeof(T) == typeof(Ray))
				return new Ray(point.X, point.Y, tank.Angle) as T;
			else if (typeof(T) == typeof(Location))
				return new Location(point.X, point.Y, tank.Angle, tank.Width, tank.Height) as T;

			throw new NotImplementedException();
		}

		public static double GetExpectedAngle(this Tank tank, Tank self, World world)
		{
			Point location = tank.GetExpectedPositionForFire<Point>(self, world);

			return self.GetTurretAngleTo(location.X, location.Y);
		}
	}
}
