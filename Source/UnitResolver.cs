using System.Text.RegularExpressions;

namespace MongoLogger
{
	public class UnitResolver
	{
		public long Resolve(string valueWithUnit)
		{
			if (valueWithUnit == null) return 0;
			if (int.TryParse(valueWithUnit, out var result)) return result;
			var match = new Regex("^(\\d+)(k|MB){0,1}$").Match(valueWithUnit);
			if (match.Success)
				result = int.Parse(match.Groups[1].Value) * GetMultiplier(match.Groups[2].Value);
			return result;
		}

		private static int GetMultiplier(string unit)
		{
			switch (unit)
			{
				case "k":
					return 1000;
				case "MB":
					return 1048576;
				default:
					return 0;
			}
		}
	}
}
