using Microsoft.EntityFrameworkCore;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

public abstract class TaggedDbContext<TTag>(DbContextOptions options) : DbContext(options);

public abstract class TaggedDbContext<TContext, TTag>(DbContextOptions<TContext> options) : TaggedDbContext<TTag>(options)
where TContext : TaggedDbContext<TContext, TTag>;
