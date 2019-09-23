using log4net;
using log4net.Config;
using Sample.Contracts;
using System;
using System.Collections.Generic;

namespace Sample
{
	public class SampleJob
	{
		private ILog Logger;

		public SampleJob()
		{
			Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			XmlConfigurator.Configure();
		}

		public void SampleTask()
		{
			// ComplexType is 1 dynamic type and can be everything ! 
			GlobalContext.Properties["ComplexType"] = new SampleContract
			{
				Data = new SampleData
				{
					Title = "Test 1",
					Description = "Description 1"
				}
			};
			Logger.Error($"Log Something Here at {DateTime.Now}");
		}

		public void SampleTask2()
		{

			// ComplexType is 1 dynamic type and can be everything ! 
			GlobalContext.Properties["ComplexType"] = new SampleContract
			{
				Data = new List<SampleData>{
					new SampleData
					{
						Title = "Test 1",
						Description = "Description 1"
					},
					new SampleData
					{
						Title = "Test 2",
						Description = "Description 2"
					}
				}
			};
			Logger.Info($"Log Something Here at {DateTime.Now}");
		}
	}
}
