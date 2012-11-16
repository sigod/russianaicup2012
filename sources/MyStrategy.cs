using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	public sealed class MyStrategy : IStrategy
	{
		public void Move(Tank self, World world, Move move)
		{
			move.LeftTrackPower = -1.0D;
			move.RightTrackPower = 1.0D;
			move.TurretTurn = Math.PI;
			move.FireType = FireType.PremiumPreferred;
		}

		public TankType SelectTank(int tankIndex, int teamSize)
		{
			return TankType.Medium;
		}
	}
}
