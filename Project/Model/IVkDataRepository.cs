namespace Project;

public interface IVkDataRepository
{
    void AddPosts(PostData postData);
    PostData? GetPost(PostData postData);
    void PutPost(PostData postData);
}