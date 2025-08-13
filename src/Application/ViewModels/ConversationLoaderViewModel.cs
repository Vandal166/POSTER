namespace Application.ViewModels;

public sealed class ConversationLoaderViewModel
{
    public IEnumerable<ConversationViewModel> Conversations { get; set; }
    public string NextUrl { get; set; } //setting the url for the next page of conversations, if empty, there are no more conversations
    public bool HasMore { get; set; }
}