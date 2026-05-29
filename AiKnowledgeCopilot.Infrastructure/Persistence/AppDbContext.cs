using AiKnowledgeCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiKnowledgeCopilot.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Chunk> Chunks => Set<Chunk>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.UploadedByUserId)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasMany(x => x.Chunks)
                .WithOne()
                .HasForeignKey(x => x.DocumentId);
        });

        modelBuilder.Entity<Chunk>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.Embedding);
        });
    }
}