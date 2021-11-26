using System.Threading.Tasks;

using static Blog.BlogService;
using Grpc.Core;
using Blog;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Server
{
    public class BlogServiceImpl : BlogServiceBase
    {
        private static string _connectionString = "mongodb://localhost:27017/"; 
        private static MongoClient _mongoClient = new MongoClient(_connectionString);
        private static IMongoDatabase _mongoDatabase = _mongoClient.GetDatabase("blogdb");
        private static IMongoCollection<BsonDocument> _mongoCollection = _mongoDatabase.GetCollection<BsonDocument>("blog");        
        
        public override Task<CreateBlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context)
        {
            var blog = request.Blog;
            BsonDocument doc = new BsonDocument("author_id", blog.AuthorId)
                .Add("title", blog.Title)
                .Add("content", blog.Content);
            
            _mongoCollection.InsertOne(doc);

            string id = doc.GetValue("_id").ToString();
            blog.Id = id;
            return Task.FromResult(new CreateBlogResponse()
            {
                Blog = blog
            });
        }
    }
}
