using AutoMapper;
using Domain.Entities.Categories;

namespace Domain.MapperProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Category, NameValueDTO>().ReverseMap();
        }
    }


    //DTO
    public class NameValueDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
