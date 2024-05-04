using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.Categories.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Shared;
using Domain.Entities.Categories;

namespace Application.Mediator.Categories.Query
{
    public class GetCategoryByIdQuery : IQuery<ServiceRespnse<CategoryDTO>>
    {
        public long Id { get; set; }

        public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, ServiceRespnse<CategoryDTO>>
        {
            private readonly IGenericRepository<Category> _categoryRepository;

            public GetCategoryByIdQueryHandler(IGenericRepository<Category> categoryRepository)
            {
                _categoryRepository = categoryRepository;
            }

            public async Task<ServiceRespnse<CategoryDTO>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
            {
                var cat = await _categoryRepository.GetAsNoTrackingQuery().Where(z => z.Id == request.Id).Select(z => new CategoryDTO
                {
                    Id = z.Id,
                    Name = z.Name,
                    parentId = (long)z.parentId
                }).FirstOrDefaultAsync();
                if (cat is null)
                {
                    Hashtable errors = new();
                    errors.Add("Id", "Not Found");
                    return new ServiceRespnse<CategoryDTO>().Failed(System.Net.HttpStatusCode.NotFound, errors);
                }
                return new ServiceRespnse<CategoryDTO>().OK(cat);
            }
        }
    }
}
