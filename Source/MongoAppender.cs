using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Appender;
using log4net.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoLogger
{
	public class MongoAppender : AppenderSkeleton
	{
		#region [Field(s)]

		private readonly List<MongoAppenderFileld> _fields = new List<MongoAppenderFileld>();

		public string ConnectionString { get; set; }

		public string ConnectionStringName { get; set; }

		public string CollectionName { get; set; }

		public long ExpireAfterSeconds { get; set; }

		public string NewCollectionMaxDocs { get; set; }

		public string NewCollectionMaxSize { get; set; }


		#endregion

		#region [Private Method(s)]

		private IMongoCollection<BsonDocument> GetCollection()
		{
			var database = GetDatabase();
			var str = CollectionName ?? "Logs";
			EnsureCollectionExists(database, str);
			return database.GetCollection<BsonDocument>(str);
		}

		private void EnsureCollectionExists(IMongoDatabase db, string collectionName)
		{
			if (CollectionExists(db, collectionName)) return;
			CreateCollection(db, collectionName);
		}

		private static bool CollectionExists(IMongoDatabase db, string collectionName) =>
			db.ListCollectionsAsync(new ListCollectionsOptions
			{ Filter = new BsonDocument("name", collectionName) }).Result
				.ToListAsync().Result.Any();

		private void CreateCollection(IMongoDatabase db, string collectionName)
		{
			var options = new CreateCollectionOptions();
			SetCappedCollectionOptions(options);
			db.CreateCollectionAsync(collectionName, options).GetAwaiter().GetResult();
		}

		private void SetCappedCollectionOptions(CreateCollectionOptions options)
		{
			var unitResolver = new UnitResolver();
			var num1 = unitResolver.Resolve(NewCollectionMaxSize);
			var num2 = unitResolver.Resolve(NewCollectionMaxDocs);
			if (num1 <= 0L) return;
			options.Capped = true;
			options.MaxSize = num1;
			if (num2 <= 0L) return;
			options.MaxDocuments = num2;
		}

		private string GetConnectionString() => ConnectionString;

		private IMongoDatabase GetDatabase()
		{
			var connectionString = GetConnectionString();
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new InvalidOperationException("Must provide a valid connection string");
			var url = MongoUrl.Create(connectionString);
			return new MongoClient(url).GetDatabase(url.DatabaseName ?? "log4net");
		}

		private BsonDocument BuildBsonDocument(LoggingEvent log)
		{
			if (_fields.Count == 0)
				return BackwardCompatibility.BuildBsonDocument(log);
			var bsonDocument = new BsonDocument();
			foreach (var field in _fields)
			{
				var obj = field.Layout.Format(log);
				bsonDocument.Add(field.Name,
					!BsonTypeMapper.TryMapToBsonValue(obj, out var value)
						? new BsonDocument { { obj.GetType().Name, obj.ToBsonDocument() } }
						: value);
			}
			return bsonDocument;
		}

		private void CreateExpiryAfterIndex(IMongoCollection<BsonDocument> collection)
		{
			if (ExpireAfterSeconds <= 0L) return;
			collection.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("timestamp"), new CreateIndexOptions
			{
				Name = "expireAfterSecondsIndex",
				ExpireAfter = new TimeSpan(ExpireAfterSeconds * 10000000L)
			}));
		}

		#endregion

		#region [Public Method(s)]

		public void AddField(MongoAppenderFileld field) => _fields.Add(field);

		#endregion

		#region [Protected Method(s)]

		protected override void Append(LoggingEvent loggingEvent)
		{
			var collection = GetCollection();
			collection.InsertOneAsync(BuildBsonDocument(loggingEvent));
			CreateExpiryAfterIndex(collection);
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			var collection = GetCollection();
			collection.InsertManyAsync(loggingEvents.Select(BuildBsonDocument));
			CreateExpiryAfterIndex(collection);
		}

		#endregion
	}
}
