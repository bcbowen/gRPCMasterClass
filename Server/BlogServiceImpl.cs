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

        public override async Task<ReadBlogResponse> ReadBlog(ReadBlogRequest request, ServerCallContext context)
        {
            var blogId = request.BlogId;
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = _mongoCollection.Find(filter).FirstOrDefault();
            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Blog Id {blogId} not found, man"));
            }

            Blog.Blog blog = new Blog.Blog() 
            {
                AuthorId = result.GetValue("author_id").AsString,
                Title = result.GetValue("title").AsString,
                Content = result.GetValue("content").AsString
            };
            
            return new ReadBlogResponse() { Blog = blog};
        }

        public override async Task<UpdateBlogResponse> UpdateBlog(UpdateBlogRequest request, ServerCallContext context)
        {
            var blogId = request.Blog.Id;
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = _mongoCollection.Find(filter).FirstOrDefault();
            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Blog Id {blogId} not found, man"));
            }

            var doc = new BsonDocument("author_id", request.Blog.AuthorId)
                .Add("title", request.Blog.Title)
                .Add("content", request.Blog.Content);
            _mongoCollection.ReplaceOne(filter, doc);

            var blog = new Blog.Blog()
            {
                AuthorId = doc.GetValue("author_id").AsString, 
                Title = doc.GetValue("title").AsString, 
                Content = doc.GetValue("content").AsString, 
                Id = blogId
            };

            return new UpdateBlogResponse() { Blog = blog };
        }
    }
}
