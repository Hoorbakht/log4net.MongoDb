using System;
using System.Collections;
using log4net.Core;
using MongoDB.Bson;

namespace MongoLogger
{
	public class BackwardCompatibility
	{
		public static BsonDocument BuildBsonDocument(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null) return null;
			var bsonDocument1 = new BsonDocument
			{
				{
					"timestamp",
					loggingEvent.TimeStamp
				},
				{
					"level",
					loggingEvent.Level.ToString()
				},
				{
					"thread",
					loggingEvent.ThreadName
				},
				{
					"userName",
					loggingEvent.UserName
				},
				{
					"message",
					loggingEvent.RenderedMessage
				},
				{
					"loggerName",
					loggingEvent.LoggerName
				},
				{
					"domain",
					loggingEvent.Domain
				},
				{
					"machineName",
					Environment.MachineName
				}
			};
			if (loggingEvent.LocationInformation != null)
			{
				bsonDocument1.Add("fileName", loggingEvent.LocationInformation.FileName);
				bsonDocument1.Add("method", loggingEvent.LocationInformation.MethodName);
				bsonDocument1.Add("lineNumber", loggingEvent.LocationInformation.LineNumber);
				bsonDocument1.Add("className", loggingEvent.LocationInformation.ClassName);
			}
			if (loggingEvent.ExceptionObject != null) bsonDocument1.Add("exception", BuildExceptionBsonDocument(loggingEvent.ExceptionObject));
			var properties = loggingEvent.GetProperties();
			if (properties == null || properties.Count <= 0) return bsonDocument1;
			var bsonDocument2 = new BsonDocument();
			foreach (DictionaryEntry dictionaryEntry in properties)
				bsonDocument2.Add(dictionaryEntry.Key.ToString(), dictionaryEntry.Value.ToString());
			bsonDocument1.Add("properties", bsonDocument2);
			return bsonDocument1;
		}

		private static BsonDocument BuildExceptionBsonDocument(Exception ex)
		{
			var bsonDocument = new BsonDocument
			{
				{
					"message",
					ex.Message
				},
				{
					"source",
					ex.Source
				},
				{
					"stackTrace",
					ex.StackTrace
				}
			};
			if (ex.InnerException != null)
				bsonDocument.Add("innerException", BuildExceptionBsonDocument(ex.InnerException));
			return bsonDocument;
		}
	}
}
