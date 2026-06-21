using Mapster;
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Application.Common.Interfaces;
using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Domain.Entities;
using SpaceExplorer.Infrastructure.Data;

namespace SpaceExplorer.Infrastructure.Common;

public abstract class GenericRepository<TEntity, TDto, TKey>(AppDbContext context)
    : IGenericRepository<TDto, TKey>
    where TEntity : BaseEntity
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public virtual async Task<TDto?> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
        var entity = await DbSet.FindAsync([id], ct);
        return entity is null ? default : entity.Adapt<TDto>();
    }

    public virtual async Task<PagedResult<TDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var total = await DbSet.CountAsync(ct);
        var items = await DbSet
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<TDto>()
            .ToListAsync(ct);

        return new PagedResult<TDto>(items, total, request.Page, request.PageSize);
    }

    public virtual async Task<TDto> CreateAsync(TDto dto, CancellationToken ct = default)
    {
        var entity = dto!.Adapt<TEntity>();
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        await DbSet.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entity.Adapt<TDto>();
    }

    public virtual async Task<TDto> UpdateAsync(TDto dto, CancellationToken ct = default)
    {
        var entity = dto!.Adapt<TEntity>();
        entity.UpdatedAt = DateTime.UtcNow;

        var tracked = await DbSet.FindAsync([entity.Id], ct);
        if (tracked is not null)
        {
            Context.Entry(tracked).CurrentValues.SetValues(entity);
            await Context.SaveChangesAsync(ct);
            return tracked.Adapt<TDto>();
        }

        DbSet.Update(entity);
        await Context.SaveChangesAsync(ct);
        return entity.Adapt<TDto>();
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken ct = default)
    {
        var entity = await DbSet.FindAsync([id], ct);
        if (entity is not null)
        {
            DbSet.Remove(entity);
            await Context.SaveChangesAsync(ct);
        }
    }
}
