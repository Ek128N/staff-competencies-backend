using Microsoft.EntityFrameworkCore;
using staff_competencies_backend.Models;

namespace staff_competencies_backend.Storage;

public class CompetenciesDbContext(DbContextOptions<CompetenciesDbContext> options) : DbContext(options)
{
    public DbSet<Skill> Skills { get; set; }
    public DbSet<PersonSkill> PersonSkills { get; set; }
    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Skill>().HasIndex(s => s.Name).IsUnique();

        modelBuilder.Entity<PersonSkill>().HasKey(s => new { s.PersonId, s.SkillId });
    }
}