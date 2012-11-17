using System;
using System.Collections;
using System.Collections.Generic;
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

			Ray ray = new Ray(self.Id, self.X, self.Y, self.Angle + self.TurretRelativeAngle);
			Location expectedLocation = selectedTank.GetExpectedPositionForFire<Location>(self, world);

			double expectedAngle = self.GetTurretAngleTo(expectedLocation.X, expectedLocation.Y);
			double distance = self.GetDistanceTo(selectedTank);

			// if we can shoot enemy
			if (ray.IsCollide(selectedTank) && !selectedTank.IsCovered(ray, world))
			{
				if (distance < self.Width)
				{
					move.FireType = FireType.PremiumPreferred;
				}
				else if (distance < 3.0d * self.Width && Math.Abs(expectedAngle) < 2.0d * Const.MIN_ANGLE)
				{
					move.FireType = FireType.PremiumPreferred;
				}
				else if (Math.Abs(expectedAngle) < Const.MIN_ANGLE)
				{
					move.FireType = FireType.Regular;
				}
			}
			if (expectedAngle > 0)
			{
				move.TurretTurn = self.TurretTurnSpeed;
			}
			else
			{
				move.TurretTurn = -self.TurretTurnSpeed;
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



	static class UnitExtensions
	{
		public static bool IsCovered(this Unit unit, Ray ray, World world)
		{
			return unit.IsCovered(ray, world.Obstacles)
				|| unit.IsCovered(ray, world.Tanks)
				|| unit.IsCovered(ray, world.Bonuses);
		}

		public static bool IsCovered<T>(this Unit unit, Ray ray, T by)
			where T : ICloneable, IList, ICollection, IEnumerable
		{
			foreach (Unit e in by)
			{
				if (e.Id != ray.OriginId && e.Id != unit.Id && ray.IsCollide(e))
					return true;
			}

			return false;
		}
	}


	#region Types

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

	class Ray : Point
	{
		public double Angle { get; set; }
		public long OriginId { get; set; }

		public Ray(double x, double y, double angle) : base(x, y)
		{
			Angle = angle;
		}

		public Ray(long id, double x, double y, double angle) : base(x, y)
		{
			OriginId = id;
			Angle = angle;
		}

		public bool IsCollide(Location with, bool isEnemy = false)
		{
			const double ray_length = 10000.0d;
			double this_angle = Helpers.ToNormalRadians(this.Angle);

			Point ray_start = new Point(this.X, this.Y);
			Point ray_end = new Point(
				this.X + Math.Cos(this_angle) * ray_length,
				this.Y + Math.Sin(this_angle) * ray_length
			);


			double with_angle = Helpers.ToNormalRadians(with.Angle);
			double cos = Math.Cos(with_angle);
			double sin = Math.Sin(with_angle);
			double half_width = with.Width * (isEnemy ? 0.95d : 1.05d) / 2.0d;
			double half_height = with.Height * (isEnemy ? 0.95d : 1.05d) / 2.0d;

			Point B1 = new Point(
				with.X + cos * (half_width + half_height),
				with.Y + sin * (half_width - half_height)
			);
			Point B2 = new Point(
				with.X + cos * (-half_width + half_height),
				with.Y + sin * (-half_width - half_height)
			);
			Point B3 = new Point(
				with.X + cos * (-half_width - half_height),
				with.Y + sin * (half_width - half_height)
			);
			Point B4 = new Point(
				with.X + cos * (half_width - half_height),
				with.Y + sin * (half_width + half_height)
			);

			return Helpers.IsIntersect(ray_start, ray_end, B1, B2)
				|| Helpers.IsIntersect(ray_start, ray_end, B2, B3)
				|| Helpers.IsIntersect(ray_start, ray_end, B3, B4)
				|| Helpers.IsIntersect(ray_start, ray_end, B4, B1);
		}

		public bool IsCollide(Unit with)
		{
			return this.IsCollide(new Location(with.X, with.Y, with.Angle, with.Width, with.Height), with.GetType().Name == "Tank");
		}
	}

	class Location : Ray
	{
		public double Width { get; set; }
		public double Height { get; set; }

		public Location(double x, double y, double angle, double width, double height) : base(x, y, angle)
		{
			Width = width;
			Height = height;
		}
	}

	#endregion


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