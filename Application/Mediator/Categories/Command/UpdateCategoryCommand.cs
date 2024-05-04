using Application.Common;
using Application.Cqrs.Commands;
using Application.Mediator.Categories.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain.DTOs.Shared;
using Domain.Entities.Categories;

namespace Application.Mediator.Categories.Command
{
    public class UpdateCategoryCommand : ICommand<ServiceRespnse>
    {
        public CategoryDTO dto { get; set; }

        public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Category> _categoryEepository;

            public UpdateCategoryCommandHandler(IGenericRepository<Category> categoryEepository)
            {
                _categoryEepository = categoryEepository;
            }

            public async Task<ServiceRespnse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
            {
                var cat = await _categoryEepository.GetAsNoTrackingQuery().Where(z => z.Id == request.dto.Id).FirstOrDefaultAsync();
                if (cat is null)
                {
                    Hashtable errors = new();
                    errors.Add("Id", "Not Found");
                    return new ServiceRespnse().Failed(System.Net.HttpStatusCode.NotFound, errors);
                }
                await _categoryEepository.UpdateAsync(new Category
                {
                    Id = request.dto.Id,
                    Name = request.dto.Name,
                    parentId = request.dto.parentId
                }, cancellationToken);
                return new ServiceRespnse().OK();

            }
        }
    }
}
