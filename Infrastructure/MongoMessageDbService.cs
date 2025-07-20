using Application;
using Core;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure
{
    public class MongoMessageDbService : IMongoMessageDbService
    {
        private readonly IMongoCollection<Message> _messagesCollection;

        public MongoMessageDbService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration.GetConnectionString("Mongo"));
            var mongoDatabase = mongoClient.GetDatabase("chatdb");
            _messagesCollection = mongoDatabase.GetCollection<Message>("Messages");
        }

        public async Task CreateAsync(Message newMessage) =>
            await _messagesCollection.InsertOneAsync(newMessage);

        public async Task<List<Message>> GetMessagesAsync(
            Guid groupId,
            int page,
            int pageSize,
            string? searchText
        )
        {
            var filter = Builders<Message>.Filter.Eq(x => x.GroupId, groupId);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filter &= Builders<Message>.Filter.Text(searchText);
            }

            return await _messagesCollection
                .Find(filter)
                .SortByDescending(x => x.Timestamp)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<Message?> GetByIdAsync(Guid id) =>
            await _messagesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task UpdateAsync(Guid id, Message updatedMessage) =>
            await _messagesCollection.ReplaceOneAsync(x => x.Id == id, updatedMessage);
    }
}
