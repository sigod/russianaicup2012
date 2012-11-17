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

	#region Types

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

		public bool IsCollide(Location with)
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
			double half_width = with.Width / 2.0d;
			double half_height = with.Height / 2.0d;

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
			return this.IsCollide(new Location(with.X, with.Y, with.Angle, with.Width, with.Height));
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
}