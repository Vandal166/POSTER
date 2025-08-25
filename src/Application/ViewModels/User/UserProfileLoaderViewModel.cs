using Application.DTOs;

namespace Application.ViewModels;

public sealed class UserProfileLoaderViewModel
{
    public UserProfileDto User { get; set; }
    
    public bool IsFollowing { get; set; } // indicates if the current user is following this user
    
    public IReadOnlyCollection<UserDto> Followers { get; set; } // list of users who follow this user
    public IReadOnlyCollection<UserDto> Following { get; set; } // list of users this user is following
    
    public IReadOnlyCollection<Guid> CurrentUserFollowingIDs { get; set; } // IDs of users the current user is following
    
    public PostLoaderViewModel UserPosts { get; set; }
}