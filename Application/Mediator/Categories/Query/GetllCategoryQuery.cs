using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.Categories.DTOs;
using Domain.DTOs.Shared;
using Domain.Entities.Categories;
using Microsoft.EntityFrameworkCore;

namespace Application.Mediator.Categories.Query
{
    public class GetAllCategoryQuery : IQuery<ServiceRespnse<List<CategoryDTO>>>
    {
        public int PageNumber { get; set; }
        public int Count { get; set; }

        public class GetAllCategoryQueryHandler : IQueryHandler<GetAllCategoryQuery, ServiceRespnse<List<CategoryDTO>>>
        {
            private readonly IGenericRepository<Category> _categoryEepository;

            public GetAllCategoryQueryHandler(IGenericRepository<Category> categoryEepository)
            {
                _categoryEepository = categoryEepository;
            }
            public async Task<ServiceRespnse<List<CategoryDTO>>> Handle(GetAllCategoryQuery request, CancellationToken cancellationToken)
            {
                var repo = _categoryEepository.GetAsNoTrackingQuery();
                var cats = await repo.Select(z => new CategoryDTO
                {
                    Id = z.Id,
                    Name = z.Name,
                    parentId = (long)z.parentId,
                    ParentName = z.Parent.Name
                }).Skip((request.PageNumber - 1) * request.Count).Take(request.Count).ToListAsync();
                var totalCount = await repo.CountAsync();
                return new ServiceRespnse<List<CategoryDTO>>().OK(cats, totalCount);
            }
        }
    }
}
