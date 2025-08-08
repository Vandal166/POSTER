using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using FluentValidation;

namespace Application.Services;

public sealed class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversations;
    private readonly IValidator<CreateConversationDto> _createConversationValidator;
    private readonly IUnitOfWork _uow;
    
    public ConversationService(IConversationRepository conversations, IValidator<CreateConversationDto> createConversationValidator, IUnitOfWork uow)
    {
        _conversations = conversations;
        _createConversationValidator = createConversationValidator;
        _uow = uow;
    }

    
    public async Task<Result<Guid>> CreateConversationAsync(Guid currentUserID, List<Guid>? participantIDs, CreateConversationDto dto, CancellationToken ct = default)
    {
        if (participantIDs is null || participantIDs.Count < 2)
            return Result.Fail<Guid>("At least two participants are required to create a conversation.");
        
        var validation = await _createConversationValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail<Guid>(validation.Errors.Select(e => e.ErrorMessage));
        
        var conversation = Conversation.Create(dto.Name, dto.ProfilePictureFileID, participantIDs.Count > 2 ? currentUserID : null); // setting an admin only if its an group conv
        if (conversation.IsFailed)
            return Result.Fail<Guid>(conversation.Errors.Select(e => e.Message));
        
        await _conversations.AddAsync(conversation.Value, ct);
        //TODO upon creating, add an default Message impersonating the creator so that the covnersation.Message is correctly indexed via CreatedAt and shown up top
        
        var conversationUsers = participantIDs.Select(id => new ConversationUser
        {
            ConversationID = conversation.Value.ID,
            UserID = id,
            JoinedAt = DateTime.UtcNow
        }).ToList();

        foreach (var user in conversationUsers)
        {
            await _conversations.AddParticipantsAsync(user, ct);
        }
        await _uow.SaveChangesAsync(ct);
        
        return Result.Ok(conversation.Value.ID);
    }

    public async Task<IPagedList<ConversationDto>> GetAllAsync(Guid currentUserID, DateTime? lastMessageAt, int pageSize,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ConversationDto?> GetConversationAsync(Guid conversationID, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> DeleteConversationAsync(Guid conversationID, Guid currentUserID, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public sealed class PostService : IPostService
{
    private readonly IPostRepository _posts;
    private readonly IPostImageRepository _postImages;
    private readonly IValidator<CreatePostDto> _createPostValidator;
    private readonly IUnitOfWork _uow;

    public PostService(IPostRepository posts, IPostImageRepository postImages, IValidator<CreatePostDto> createPostValidator, IUnitOfWork uow)
    {
        _posts = posts;
        _postImages = postImages;
        _createPostValidator = createPostValidator;
        _uow = uow;
    }

    public async Task<Result<Guid>> CreatePostAsync(CreatePostDto dto, Guid userID, CancellationToken cancellationToken)
    {
        var validation = await _createPostValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return Result.Fail<Guid>(validation.Errors.Select(e => e.ErrorMessage));
        
        var post = Post.Create(userID, dto.Content, dto.VideoFileID);
        var postImages = new List<PostImage>();
        foreach (var imageIDs in dto.ImageFileIDs ?? Enumerable.Empty<Guid>())
        {
            var postImageResult = PostImage.Create(post.Value.ID, imageIDs);
            if (postImageResult.IsFailed)
                return Result.Fail<Guid>(postImageResult.Errors.Select(e => e.Message));
            postImages.Add(postImageResult.Value);
        }
        if (post.IsFailed)
            return Result.Fail<Guid>(post.Errors.Select(e => e.Message));

        await _posts.AddAsync(post.Value, cancellationToken);
        await _postImages.AddRangeAsync(postImages, cancellationToken);
        
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(post.Value.ID);
    }
    
    public async Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        //return await _posts.GetPostAsync(id, cancellationToken);
        return null;
    }
    
    public async Task<IPagedList<PostDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        //return await _posts.GetAllAsync(page, pageSize, cancellationToken);
        return null;
    }
    
    public async Task<Result<bool>> DeletePostAsync(Guid id, Guid currentUserID, CancellationToken cancellationToken = default)
    {
        var post = await _posts.GetPostByIDAsync(id, cancellationToken);
        if (post is null)
            return Result.Fail<bool>("Post not found");
        
        if (post.AuthorID != currentUserID)
            return Result.Fail<bool>("Unable to delete post that does not belong to you");
        
        await _posts.DeleteAsync(post, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(true);
    }
}