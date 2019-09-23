using log4net.Layout;

namespace MongoLogger
{
	public class MongoAppenderFileld
	{
		public string Name { get; set; }

		public IRawLayout Layout { get; set; }
	}
}
