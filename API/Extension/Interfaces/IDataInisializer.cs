using Application.Extensions;

namespace API.Extension.Interfaces
{
    public interface IDataInitializer : IScopedDependency
    {
        void InitializeData();
    }
}
