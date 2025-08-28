using LBV_GTM_Basics;
using LBV_GTM_Basics.Models;
using LBV_GTM_Basics.Models.Common;

namespace LBV_GTM_Database_API.Services
{
    public class FullService<ModelType> where ModelType : class, IBaseModel, new()
    {
        public readonly Dictionary<Type, dynamic> Services = [];

        public FullService(
        UserService UserService,
        TicketService TicketService)
        {
            Services[typeof(User)] = UserService;
            Services[typeof(Ticket)] = TicketService;
        }

        public async Task ForceDelete(Type modelType, dynamic obj)
        {
            foreach (Type _modelType in GlobalValues.ModelTypeList)
            {
                var list = await Services[_modelType].GetChildObjects(modelType, obj.Id, false);
                foreach (var item in list) { await ForceDelete(_modelType, item); }
            }
            await Services[modelType].Delete(obj);
        }

        public async Task UpdateChildObjects(Type modelType, dynamic obj)
        {
            if (!Scripts.IsCompositeKey(modelType.Name))
            {
                foreach (Type _modelType in GlobalValues.ModelTypeList)
                {
                    var list = await Services[_modelType].GetChildObjects(modelType, obj.Id, false);
                    foreach (var item in list)
                    {
                        bool isValid = Services[_modelType].Validate(item);
                        bool isValidUniqProps = await Services[_modelType].ValidateUniqProps(item);
                        if (item is null || !isValidUniqProps) { await ForceDelete(_modelType, item); }
                        else if (!isValid)
                        {
                            await Services[_modelType].Update(item);
                            await UpdateChildObjects(_modelType, item);
                        }
                    }
                }
            }
        }

        public async Task<bool> HasChildObjects(int id, bool ignoreCompositeKeys)
        {
            foreach (Type modelType in GlobalValues.ModelTypeList)
            {
                if ((await Services[modelType].GetChildObjects(typeof(ModelType), id, ignoreCompositeKeys)).Count > 0) { return true; }
            }
            return false;
        }
    }
}
