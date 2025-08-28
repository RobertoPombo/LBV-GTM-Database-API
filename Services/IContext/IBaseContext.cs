using LBV_Basics.Models.Common;

namespace LBV_GTM_Database_API.Services.Interfaces
{
    public interface IBaseContext<ModelType> where ModelType : class, IBaseModel, new()
    {
        public Task SaveChanges();
        public Task<List<ModelType>> GetAll();
        public Task<ModelType?> GetById(int id);
        public Task Add(ModelType obj);
        public Task Delete(ModelType obj);
        public Task Update(ModelType obj);
    }
}
