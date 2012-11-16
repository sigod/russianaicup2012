using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	public sealed class MyStrategy : IStrategy
	{
		private readonly double MIN_ANGLE = Math.PI / 180.0;

		public void Move(Tank self, World world, Move move)
		{
			double angleToTank = double.MaxValue;
			Tank selectedTank = null;

			foreach (Tank tank in world.Tanks)
			{
				if (tank.IsTeammate) continue;
				if (tank.CrewHealth < 1) continue;

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

			if (selectedBonus != null)
			{
				double angle = self.GetAngleTo(selectedBonus);

				if (angle > MIN_ANGLE)
				{
					move.LeftTrackPower = 1.0d;
					move.RightTrackPower = 1.0d - Math.Abs(angle) * 2d;
				}
				else if (angle < -MIN_ANGLE)
				{
					move.LeftTrackPower = 1.0d - Math.Abs(angle) * 2d;
					move.RightTrackPower = 1.0d;
				}
				else
				{
					move.LeftTrackPower = 1.0d;
					move.RightTrackPower = 1.0d;
				}

				if (Math.Abs(angle) > Math.PI / 2)
				{
					double tmp = move.LeftTrackPower;
					move.LeftTrackPower = -move.RightTrackPower;
					move.RightTrackPower = -tmp;
				}
			}
		}

		public TankType SelectTank(int tankIndex, int teamSize)
		{
			return TankType.Medium;
		}
	}
}