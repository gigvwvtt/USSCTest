using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Project;

public class VkDataRepository : IVkDataRepository
{
    private readonly VkDataDbContext db;

    public VkDataRepository(VkDataDbContext db)
    {
        this.db = db;
    }

    public PostData? GetPost(PostData postData) => db.Posts.FirstOrDefault(x => x.id == postData.id) as PostData;
    

    public void PutPost(PostData postData)
    {
        var found = GetPost(postData);
        if (found == null) throw new Exception("Нельзя обновить данные поста, которого нет в базе данных!");
        
        found.id = postData.id;
        found.owner_id = postData.owner_id;
        found.text = postData.text;
        found.lettersInfo = postData.lettersInfo;
        
        db.Entry(found).State = EntityState.Modified;
        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
        }
    }

    public void AddPosts(PostData postData)
    {
        try
        {
            db.Posts.AddRange(postData);
            db.SaveChanges();
        }
        catch (DbException exception)
        {
        }
    }
}