using System;
using System.Collections;
using System.Collections.Generic;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
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
}
