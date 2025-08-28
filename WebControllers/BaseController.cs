using Microsoft.AspNetCore.Mvc;

using LBV_GTM_Database_API.Services;
using LBV_GTM_Basics.Models.Common;
using LBV_GTM_Basics;

namespace LBV_GTM_Database_API.Controllers
{
    [ApiController]
    [Route(nameof(ModelType))]
    public class BaseController<ModelType>(BaseService<ModelType> service, FullService<ModelType> fullService) : ControllerBase where ModelType : class, IBaseModel, new()
    {
        [HttpGet("Get")] public async Task<ActionResult<List<ModelType>>> GetAll() { return Ok(await service.GetAll()); }

        [HttpGet("Get/{id}")] public async Task<ActionResult<ModelType?>> GetById(int id)
        {
            ModelType? obj = await service.GetById(id);
            if (obj is null) { return NotFound(obj); }
            else { return Ok(obj); }
        }

        [HttpDelete("Delete/{id}/{force}")] public async Task<ActionResult> Delete(int id, bool force = false)
        {
            ModelType? obj = await service.GetById(id);
            if (obj is null) { return NotFound(); }
            else if (!force && await fullService.HasChildObjects(obj.Id, true)) { return StatusCode(405); }
            else { await fullService.ForceDelete(typeof(ModelType), obj); return Ok(); }
        }

        [HttpGet("Get/HasChildObjects/{id}/{ignoreCompositeKeys}")] public async Task<bool> HasChildObjects(int id, bool ignoreCompositeKeys = true)
        {
            return await fullService.HasChildObjects(id, ignoreCompositeKeys);
        }

        [HttpGet("Get/{modelTypeName}/{id}")] public async Task<ActionResult<List<dynamic>>> GetChildObjects(string modelTypeName, int id, bool ignoreCompositeKeys = false)
        {
            foreach (Type modelType in GlobalValues.ModelTypeList)
            {
                if (modelType.Name == modelTypeName)
                {
                    return Ok(await fullService.Services[modelType].GetChildObjects(typeof(ModelType), id, ignoreCompositeKeys));
                }
            }
            return NotFound(new List<dynamic>());
        }
    }
}
