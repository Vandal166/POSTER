using Application.Contracts.Persistence;
using Application.DTOs;
using Bogus;
using Domain.Entities;
using Infrastructure.Auth;

namespace Web.Seeder;


public sealed class DataSeeder : IDataSeeder
{
    private readonly IAuthService _authService;
    private readonly IPostService _postService;
    private readonly IPostCommentService _postCommentService;
    private readonly IFollowService _followService;
    private readonly IPostLikeService _postLikeService;
    private readonly ICommentLikeService _commentLikeService;
    private readonly IConversationService _conversationService;
    private readonly IBlobService _blobService;
    private readonly IUserRepository _userRepo;
    
    public DataSeeder(IAuthService authService, IPostService postService, IPostCommentService postCommentService,
        IFollowService followService, IPostLikeService postLikeService, ICommentLikeService commentLikeService,
        IConversationService conversationService, IBlobService blobService, IUserRepository userRepo)
    {
        _authService = authService;
        _postService = postService;
        _postCommentService = postCommentService;
        _followService = followService;
        _postLikeService = postLikeService;
        _commentLikeService = commentLikeService;
        _conversationService = conversationService;
        _blobService = blobService;
        _userRepo = userRepo;
    }

    private static readonly string CurrentDirectory = Directory.GetCurrentDirectory();
    
    private readonly Dictionary<string, string> VideoPaths = new Dictionary<string, string>
    {
        { "default-video1.mp4", Path.Combine(CurrentDirectory, "wwwroot/uploads/defaults/videos/default-video1.mp4") },
        { "default-video2.mp4", Path.Combine(CurrentDirectory, "wwwroot/uploads/defaults/videos/default-video2.mp4") },
        { "default-video3.mp4", Path.Combine(CurrentDirectory, "wwwroot/uploads/defaults/videos/default-video3.mp4") }
    };
    
    private readonly Dictionary<string, string> ImagePaths = new Dictionary<string, string>
    {
        { "default-image1.jpg", Path.Combine(CurrentDirectory, "wwwroot/uploads/defaults/images/default-image1.jpg") },
        { "default-image2.jpg", Path.Combine(CurrentDirectory, "wwwroot/uploads/defaults/images/default-image2.jpg") },
        { "default-image3.jpg", Path.Combine(CurrentDirectory, "wwwroot/uploads/defaults/images/default-image3.jpg") }
    };
    private class UserFakerAggregate
    {
        public User User { get; set; }
        
        public List<Post> Posts { get; set; } = new List<Post>();
    }
    public async Task SeedAsync(int userCount, CancellationToken ct = default)
    {
        //Uploading shared resources that will be used across users
        var userImageIDs = new List<Guid>();
        var userVideoIDs = new List<Guid>();

        // images
        foreach (var imagePath in ImagePaths.Values)
        {
            var imageStream = File.OpenRead(imagePath);
            var imageID = await _blobService.UploadFileAsync(imageStream, "image/jpg", "images", ct);
            userImageIDs.Add(imageID);
        }

        // videos
        foreach (var videoPath in VideoPaths.Values)
        {
            var videoStream = File.OpenRead(videoPath);
            var videoID = await _blobService.UploadFileAsync(videoStream, "video/mp4", "videos", ct);
            userVideoIDs.Add(videoID);
        }
        
        
        // User seeding, registering and syncing with db <with random avatars>
        var userFaker = new Faker<RegisterUserDto>()
            .CustomInstantiator(f => {
                var email = f.Internet.Email().ToLower();
                return new RegisterUserDto(email, email);
            });
        
        var registerUserDtos = userFaker.Generate(userCount);
        var userAggregates = new List<UserFakerAggregate>();
        
        // registering users and user aggregates
        foreach (var registerDto in registerUserDtos)
        {
            var result = await _authService.RegisterAsync(registerDto, ct);
            if (result.IsFailed)
            {
                Console.WriteLine($"Failed to register user {registerDto.Email}: {string.Join(", ", result.Errors.Select(e => e.Message))}");
                continue;
            }

            var userProfileDto = await _userRepo.GetUserProfileDtoByNameAsync(registerDto.Email.Split('@')[0], ct);
            if (userProfileDto is null)
            {
                Console.WriteLine($"User profile not found for {registerDto.Email.Split('@')[0]} Skipping.");
                continue;
            }

            var userEntity = await _userRepo.GetUserAsync(userProfileDto.Id, ct);
            if (userEntity is null)
            {
                Console.WriteLine($"User entity not found for {registerDto.Email}. Skipping.");
                continue;
            }

            // Create user aggregate
            var userAggregate = new UserFakerAggregate
            {
                User = userEntity
            };
            userAggregates.Add(userAggregate);
        }

        Console.WriteLine($"Created {userAggregates.Count} user aggregates.");
        
        var postFaker = new Faker<CreatePostDto>()
            .CustomInstantiator(f => {
                var video = f.Random.Bool() ? f.PickRandom(userVideoIDs) : (Guid?)null;
                var imageCount = f.Random.Int(0, 3);
                var images = imageCount > 0 ? f.PickRandom(userImageIDs, imageCount).ToArray() : null;
                return new CreatePostDto(
                    f.Lorem.Sentence(10),
                    video,
                    images
                );
            });
        
        
        // creating rand posts <with random images/video> for each user
        foreach (var userAggregate in userAggregates)
        {
            var posts = postFaker.Generate(new Random().Next(1, 3)); //between 1 and 3 posts per user
        
            foreach (var postDto in posts)
            {
                var postResult = await _postService.CreatePostAsync(postDto, userAggregate.User.ID, ct);
                if (postResult.IsFailed)
                {
                    Console.WriteLine($"Failed to create post for user {userAggregate.User.Username}: {string.Join(", ", postResult.Errors.Select(e => e.Message))}");
                    continue;
                }

                var createdPost = await _postService.GetPostAsync(postResult.Value, ct);
                if (createdPost != null)
                {
                    userAggregate.Posts.Add(createdPost);
                    Console.WriteLine($"Post created for user {userAggregate.User.Username} with ID: {postResult.Value}");
                }
            }
        }
        
        
        var commentFaker = new Faker<CreateCommentDto>()
            .CustomInstantiator(f => new CreateCommentDto(f.Lorem.Sentence(5)));
        
        // for each post, create rand comments on other users' posts
        foreach (var commenterAggregate in userAggregates)
        {
            var otherUserPosts = userAggregates
                .Where(ua => ua.User.ID != commenterAggregate.User.ID)
                .SelectMany(ua => ua.Posts)
                .OrderBy(_ => Guid.NewGuid())
                .Take(new Random().Next(1, 4)) // rnd (between 1 and 3)
                .ToList();

            foreach (var post in otherUserPosts)
            {
                var commentDto = commentFaker.Generate();
                var commentResult = await _postCommentService.CreateCommentAsync(post.ID, commenterAggregate.User.ID, commentDto, ct);
            
                if (commentResult.IsFailed)
                {
                    Console.WriteLine($"Failed to create comment for post {post.ID} by user {commenterAggregate.User.Username}: {string.Join(", ", commentResult.Errors.Select(e => e.Message))}");
                    continue;
                }
            }
        }

        // for each user, follow random users
        
        foreach (var followerAggregate in userAggregates)
        {
            var otherUsers = userAggregates
                .Where(ua => ua.User.ID != followerAggregate.User.ID)
                .OrderBy(_ => Guid.NewGuid())
                .Take(userCount / 10) // follow 10% of users
                .ToList();

            foreach (var otherUser in otherUsers)
            {
                if (otherUser.User.ID == followerAggregate.User.ID) continue; // skip self
                await _followService.FollowUserAsync(followerAggregate.User.ID, otherUser.User.ID, ct);
            }
        }

        // for each user, like random posts
        foreach (var likerAggregate in userAggregates)
        {
            var postsToLike = userAggregates
                .Where(ua => ua.User.ID != likerAggregate.User.ID)
                .SelectMany(ua => ua.Posts)
                .OrderBy(_ => Guid.NewGuid())
                .Take(new Random().Next(3, 8))
                .ToList();

            foreach (var post in postsToLike)
            {
                await _postLikeService.ToggleLikeAsync(postID: post.ID, userID: likerAggregate.User.ID, ct);
                Console.WriteLine($"User {likerAggregate.User.Username} liked a post from another user");
            }
        }

        // for each user, like random comments
        foreach (var likerAggregate in userAggregates)
        {
            var postsWithComments = userAggregates
                .SelectMany(ua => ua.Posts)
                .Where(p => p.Comments.Any())
                .ToList();

            var commentsToLike = postsWithComments
                .SelectMany(p => p.Comments)
                .OrderBy(_ => Guid.NewGuid())
                .Take(new Random().Next(2, 6))
                .ToList();

            foreach (var comment in commentsToLike)
            {
                await _commentLikeService.ToggleLikeAsync(commentID: comment.ID, userID: likerAggregate.User.ID, ct);
                Console.WriteLine($"User {likerAggregate.User.Username} liked a comment");
            }
        }

        // for each user, create conversations with other users
        var conversationFaker = new Faker<CreateConversationDto>()
            .CustomInstantiator(f => new CreateConversationDto(
                f.Lorem.Sentence(3),
                new Guid("4fdd2f9f-bca8-4f90-8e27-ed432cbc39e0") // default conversation image
            ));

        foreach (var initiatorAggregate in userAggregates)
        {
            // select 1-3 random conversations to create
            int conversationsToCreate = new Random().Next(1, 4);
            
            for (int i = 0; i < conversationsToCreate; i++)
            {
                // select participants (1-3 other users)
                var participants = userAggregates
                    .Where(ua => ua.User.ID != initiatorAggregate.User.ID)
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(new Random().Next(1, 4))
                    .Select(ua => ua.User.ID)
                    .Append(initiatorAggregate.User.ID) // include creator
                    .ToList();
                    
                if (!participants.Any()) continue; // skip if no participants selected
                
                var dto = conversationFaker.Generate();
                
                await _conversationService.CreateConversationAsync(
                    currentUserID: initiatorAggregate.User.ID, 
                    participantIDs: participants, 
                    dto: dto, 
                    ct);
                    
                Console.WriteLine($"User {initiatorAggregate.User.Username} created a conversation with {participants.Count} other users");
            }
        }
    }
}