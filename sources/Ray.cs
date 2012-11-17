using System;
using Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeTanks2012.DevKit.CSharpCgdk
{
	class Ray : Point
	{
		public double Angle { get; set; }
		public long OriginId { get; set; }

		public Ray(double x, double y, double angle)
			: base(x, y)
		{
			Angle = angle;
		}

		public Ray(long id, double x, double y, double angle)
			: base(x, y)
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
}
