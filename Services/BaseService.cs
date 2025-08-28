using System.Reflection;

using LBV_GTM_Basics;
using LBV_GTM_Basics.Models.Common;
using LBV_GTM_Basics.Models.DTOs;
using LBV_GTM_Database_API.Services.Interfaces;

namespace LBV_GTM_Database_API.Services
{
    public class BaseService<ModelType> where ModelType : class, IBaseModel, new()
    {
        public readonly Dictionary<DtoType, Type> DtoModels = [];
        public readonly List<Type> UniqPropsDtoModels = [];
        public readonly List<List<PropertyInfo>> UniqProps = [[]];
        public IBaseContext<ModelType> iBaseContext;

        public BaseService(IBaseContext<ModelType> _iBaseContext)
        {
            if (GlobalValues.DictDtoModels.ContainsKey(typeof(ModelType))) { DtoModels = GlobalValues.DictDtoModels[typeof(ModelType)]; }
            if (GlobalValues.DictUniqPropsDtoModels.ContainsKey(typeof(ModelType)))
            {
                UniqProps = [];
                UniqPropsDtoModels = GlobalValues.DictUniqPropsDtoModels[typeof(ModelType)];
                foreach (Type uniqPropDto in UniqPropsDtoModels)
                {
                    List<PropertyInfo> propertyList = [];
                    foreach (PropertyInfo property in uniqPropDto.GetProperties())
                    {
                        foreach (PropertyInfo baseProperty in typeof(ModelType).GetProperties())
                        {
                            if (property.Name == baseProperty.Name) { propertyList.Add(baseProperty); }
                        }
                    }
                    UniqProps.Add(propertyList);
                }
            }
            iBaseContext = _iBaseContext;
        }

        public async Task<bool> IsUnique(ModelType obj)
        {
            for (int index = 0; index < UniqProps.Count; index++)
            {
                if (!await IsUnique(obj, index)) { return false; }
            }
            return true;
        }

        public async Task<bool> IsUnique(ModelType obj, int index)
        {
            if (UniqProps.Count > index && UniqProps[index].Count > 0)
            {
                int objIndex0 = -1;
                List<ModelType> list = await GetAll();
                if (list.Contains(obj)) { objIndex0 = list.IndexOf(obj); }
                for (int objIndex = 0; objIndex < list.Count; objIndex++)
                {
                    if (objIndex != objIndex0)
                    {
                        bool identical = true;
                        foreach (PropertyInfo property in UniqProps[index])
                        {
                            if (Scripts.GetCastedValue(obj, property) != Scripts.GetCastedValue(list[objIndex], property)) { identical = false; break; }
                        }
                        if (identical) { return false; }
                    }
                }
            }
            return true;
        }

        public async Task<ModelType?> GetByUniqProps(UniqPropsDto<ModelType> objDto)
        {
            if (UniqProps.Count > objDto.Index && UniqProps[objDto.Index].Count > 0 && UniqProps[objDto.Index].Count == Scripts.GetPropertyList(objDto.Dto.GetType()).Count)
            {
                AddDto<ModelType> addDto = new();
                addDto.Dto = Scripts.Map(objDto.Dto, addDto.Dto, true);
                List<ModelType> list = await GetByProps(addDto, false, objDto.Index);
                if (list.Count == 1) { return list[0]; }
                else { return null; }
            }
            return null;
        }

        public async Task<List<ModelType>> GetByProps(AddDto<ModelType> objDto, bool firstOnly = false, int indexUniqProps = -1)
        {
            List<ModelType> _list = [];
            List<ModelType> list = await GetAll();
            List<PropertyInfo> listProps = Scripts.GetPropertyList(typeof(ModelType));
            if (firstOnly && indexUniqProps >= 0 && indexUniqProps < UniqProps.Count) { listProps = UniqProps[indexUniqProps]; }
            foreach (ModelType obj in list)
            {
                bool found = true;
                foreach (PropertyInfo property in listProps)
                {
                    foreach (PropertyInfo filterProperty in objDto.Dto.GetType().GetProperties())
                    {
                        if (filterProperty.Name == property.Name && filterProperty.GetValue(objDto.Dto) is not null && property.GetValue(obj) is not null)
                        {
                            if (Scripts.GetCastedValue(obj, property) != Scripts.GetCastedValue(objDto.Dto, filterProperty)) { found = false; break; }
                            break;
                        }
                    }
                }
                if (found) { _list.Add(obj); if (firstOnly) { return _list; } }
            }
            return _list;
        }

        public async Task<List<ModelType>> GetByFilter(FilterDto<ModelType> objFilter, FilterDto<ModelType> objFilterMin, FilterDto<ModelType> objFilterMax)
        {
            List<ModelType> list = await GetAll();
            List<ModelType> filteredList = [];
            List<PropertyInfo> listModelProps = Scripts.GetPropertyList(typeof(ModelType));
            List<PropertyInfo> listFilterProps = Scripts.GetPropertyList(objFilter.Dto.GetType());
            foreach (ModelType obj in list)
            {
                bool isInList = true;
                foreach (PropertyInfo filterProperty in listFilterProps)
                {
                    var filter = filterProperty.GetValue(objFilter.Dto);
                    var filterMin = filterProperty.GetValue(objFilterMin.Dto);
                    var filterMax = filterProperty.GetValue(objFilterMax.Dto);
                    if (filter is not null || filterMin is not null || filterMax is not null)
                    {
                        string strFilter = filter?.ToString()?.ToLower() ?? string.Empty;
                        string strFilterMin = filterMin?.ToString()?.ToLower() ?? string.Empty;
                        string strFilterMax = filterMax?.ToString()?.ToLower() ?? string.Empty;
                        foreach (PropertyInfo property in listModelProps)
                        {
                            if (filterProperty.Name == property.Name)
                            {
                                var castedValue = Scripts.GetCastedValue(obj, property);
                                string strValue = castedValue?.ToString().ToLower() ?? 0;
                                if (!strValue.Contains(strFilter)) { isInList = false; }
                                else if (GlobalValues.numericalTypes.Contains(property.PropertyType))
                                {
                                    var castedFilterMin = Scripts.CastValue(property, filterMin);
                                    var castedFilterMax = Scripts.CastValue(property, filterMax);
                                    if (castedFilterMin is not null) { if (castedValue is null || castedValue < castedFilterMin) { isInList = false; } }
                                    if (castedFilterMax is not null) { if (castedValue is null || castedValue > castedFilterMax) { isInList = false; } }
                                }
                                else if ((strFilterMin.Length > 0 && string.Compare(strValue, strFilterMin) == -1)
                                    || (strFilterMax.Length > 0 && string.Compare(strValue, strFilterMax) == 1))
                                {
                                    isInList = false;
                                }
                                break;
                            }
                        }
                        if (!isInList) { break; }
                    }
                }
                if (isInList) { filteredList.Add(obj); }
            }
            return filteredList;
        }

        public async Task SaveChanges() { await iBaseContext.SaveChanges(); }

        public async Task<List<ModelType>> GetAll() { return await iBaseContext.GetAll(); }

        public async Task<ModelType?> GetById(int id) { return await iBaseContext.GetById(id); }

        public async Task Add(ModelType obj) { await iBaseContext.Add(obj); }

        public async Task Delete(ModelType obj) { await iBaseContext.Delete(obj); }

        public async Task Update(ModelType obj) { await iBaseContext.Update(obj); }

        public async Task<List<ModelType>> GetChildObjects(Type modelType, int id, bool ignoreCompositeKeys = false)
        {
            PropertyInfo? property = GlobalValues.DictDtoModels[typeof(ModelType)][DtoType.Add].GetProperty(modelType.Name + GlobalValues.Id);
            if (property is not null && (!ignoreCompositeKeys || !typeof(ModelType).Name.Contains(modelType.Name)))
            {
                Type addDtoType = typeof(AddDto<>).MakeGenericType(modelType);
                AddDto<ModelType> dto = new();
                property.SetValue(dto.Dto, Scripts.CastValue(property, id));
                return await GetByProps(dto);
            }
            return [];
        }
    }
}
