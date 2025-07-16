using Microsoft.EntityFrameworkCore;
using MinimalApi.Model;

namespace MinimalApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<TodoModel> Todo => Set<TodoModel>();
        public DbSet<NotesModel> Notes => Set<NotesModel>();
        public DbSet<UserModel> UserModels => Set<UserModel>();
    }
}
