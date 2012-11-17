using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	static class Const
	{
		public const double MIN_ANGLE = Math.PI / 180.0d;
		public const double HALF_PI = Math.PI / 2.0d;
		public const double BulletSpeed = 15;
	}

	public sealed class MyStrategy : IStrategy
	{
		public void Move(Tank self, World world, Move move)
		{
			Fire(self, world, move);
			MoveToBonus(self, world, move);
		}

		public TankType SelectTank(int tankIndex, int teamSize)
		{
			return TankType.Heavy;
		}

		private void Fire(Tank self, World world, Move move)
		{
			double angleToTank = double.MaxValue;
			Tank selectedTank = null;

			foreach (Tank tank in world.Tanks)
			{
				if (tank.IsTeammate) continue;
				if (tank.CrewHealth < 1 || tank.HullDurability < 1) continue;

				double angle = self.GetTurretAngleTo(tank);

				if (Math.Abs(angle) < Math.Abs(angleToTank))
				{
					angleToTank = angle;
					selectedTank = tank;
				}
			}

			if (selectedTank == null) return;

			double expectedAngle = selectedTank.GetExpectedAngle(self, world);

			if (expectedAngle > Const.MIN_ANGLE)
			{
				move.TurretTurn = self.TurretTurnSpeed;
			}
			else if (expectedAngle < -Const.MIN_ANGLE)
			{
				move.TurretTurn = -self.TurretTurnSpeed;
			}
			else
			{
				move.FireType = FireType.PremiumPreferred;
			}
		}

		private void MoveToBonus(Tank self, World world, Move move)
		{
			double distanceToBonus = double.MaxValue;
			Bonus selectedBonus = null;

			foreach (Bonus bonus in world.Bonuses)
			{
				double distance = self.GetDistanceTo(bonus);

				if (distance < distanceToBonus)
				{
					distanceToBonus = distance;
					selectedBonus = bonus;
				}
			}

			if (selectedBonus == null) return;

			double angle = self.GetAngleTo(selectedBonus);
			double absAngle = Math.Abs(angle);

			if (absAngle < Const.MIN_ANGLE) // move forward
			{
				move.LeftTrackPower = 1.0d;
				move.RightTrackPower = 1.0d;
			}
			else if (absAngle > Math.PI - Const.MIN_ANGLE) // move back
			{
				move.LeftTrackPower = -1.0d;
				move.RightTrackPower = -1.0d;
			}
			else if (absAngle > Const.HALF_PI - Const.MIN_ANGLE
				&& absAngle < Const.HALF_PI + Const.MIN_ANGLE) // turn
			{
				move.LeftTrackPower = 1.0d;
				move.RightTrackPower = -1.0d;
			}
			else if (absAngle < Const.HALF_PI)
			{
				move.LeftTrackPower = 1.0d;
				move.RightTrackPower = 1.0d - absAngle * 2d;

				if (angle < 0.0d)
				{
					var tmp = move.LeftTrackPower;
					move.LeftTrackPower = move.RightTrackPower;
					move.RightTrackPower = tmp;
				}
			}
			else // if (absAngle > HALF_PI)
			{
				move.LeftTrackPower = -1.0d;
				move.RightTrackPower = -1.0d - absAngle * 2d;

				if (angle < 0.0d)
				{
					var tmp = move.LeftTrackPower;
					move.LeftTrackPower = move.RightTrackPower;
					move.RightTrackPower = tmp;
				}
			}
		}
	}



	static class TankExtensions
	{
		public static Point GetExpectLocationForFire(this Tank tank, Tank self, World world)
		{
			double time = self.GetDistanceTo(tank) / Const.BulletSpeed;

			Point expected = new Point(tank.X + tank.SpeedX * time, tank.Y + tank.SpeedY * time);

			return new Point(
				expected.X < 0 ? 0 : (expected.X > world.Width ? world.Width - 1 : expected.X),
				expected.Y < 0 ? 0 : (expected.Y > world.Height ? world.Height - 1 : expected.Y)
			);
		}

		public static double GetExpectedAngle(this Tank tank, Tank self, World world)
		{
			Point location = tank.GetExpectLocationForFire(self, world);

			return self.GetTurretAngleTo(location.X, location.Y);
		}
	}



	static class UnitExtensions
	{
		public static bool IsCollide(this Unit unit, Unit with)
		{
			const double ray_length = 1000.0d;

			Point ray_start = new Point(unit.X, unit.Y);
			Point ray_end = new Point(
				unit.X + Math.Cos(unit.Angle) * ray_length,
				unit.Y + Math.Sin(unit.Angle) * ray_length
			);


			double half_width = with.Width / 2.0d;
			double half_height = with.Height / 2.0d;

			Point B1 = new Point(
				with.X + Math.Cos(with.Angle) * (half_width + half_height),
				with.Y + Math.Sin(with.Angle) * (half_width - half_height)
			);
			Point B2 = new Point(
				with.X + Math.Cos(with.Angle) * (-half_width + half_height),
				with.Y + Math.Sin(with.Angle) * (-half_width - half_height)
			);
			Point B3 = new Point(
				with.X + Math.Cos(with.Angle) * (-half_width - half_height),
				with.Y + Math.Sin(with.Angle) * (half_width - half_height)
			);
			Point B4 = new Point(
				with.X + Math.Cos(with.Angle) * (half_width - half_height),
				with.Y + Math.Sin(with.Angle) * (half_width + half_height)
			);

			return Helpers.IsIntersect(ray_start, ray_end, B1, B2)
				|| Helpers.IsIntersect(ray_start, ray_end, B2, B3)
				|| Helpers.IsIntersect(ray_start, ray_end, B3, B4)
				|| Helpers.IsIntersect(ray_start, ray_end, B4, B1);
		}
	}



	struct Point
	{
		public double X, Y;

		public Point(double x, double y)
		{
			X = x;
			Y = y;
		}
	}

	static class Helpers
	{
		public static void Exchange<T>(ref T A, ref T B)
		{
			T tmp = A;
			A = B;
			B = A;
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