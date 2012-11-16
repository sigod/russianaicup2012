using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	public sealed class MyStrategy : IStrategy
	{
		private const double MIN_ANGLE = Math.PI / 180.0d;
		private const double HALF_PI = Math.PI / 2.0d;

		public void Move(Tank self, World world, Move move)
		{
			Fire(self, world, move);
			MoveToBonus(self, world, move);
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

			if (selectedTank != null)
			{
				if (angleToTank > MIN_ANGLE)
					move.TurretTurn = 1.0d;
				else if (angleToTank < -MIN_ANGLE)
					move.TurretTurn = -1.0d;
				else
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

			if (absAngle < MIN_ANGLE) // move forward
			{
				move.LeftTrackPower = 1.0d;
				move.RightTrackPower = 1.0d;
			}
			else if (absAngle > Math.PI - MIN_ANGLE) // move back
			{
				move.LeftTrackPower = -1.0d;
				move.RightTrackPower = -1.0d;
			}
			else if (absAngle > HALF_PI - MIN_ANGLE
				&& absAngle < HALF_PI + MIN_ANGLE) // turn
			{
				move.LeftTrackPower = 1.0d;
				move.RightTrackPower = -1.0d;
			}
			else if (absAngle < HALF_PI)
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

		public TankType SelectTank(int tankIndex, int teamSize)
		{
			return TankType.Medium;
		}

		#region Helpers

		private void Exchange<T>(ref T A, ref T B)
		{
			T tmp = A;
			A = B;
			B = A;
		}

		#endregion
	}
}