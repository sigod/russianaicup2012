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
			return TankType.Medium;
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
}