namespace Application.Mediator.Categories.DTOs
{
    public class CategoryDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long parentId { get; set; }
        public string ParentName { get; set; }
    }
}
