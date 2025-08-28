using Microsoft.AspNetCore.Mvc;

using LBV_Basics.Models;
using LBV_GTM_Database_API.Services;
using LBV_Basics.Models.DTOs;

namespace LBV_GTM_Database_API.Controllers
{
    [ApiController]
    [Route(nameof(User))]
    public class UserController(UserService service, BaseService<User> baseService, FullService<User> fullService) : BaseController<User>(baseService, fullService)
    {
        [HttpPut("Get/ByUniqProps/0")] public async Task<ActionResult<User?>> GetByUniqProps(UserUniqPropsDto0 objDto)
        {
            UniqPropsDto<User> _objDto = new() { Index = 0, Dto = objDto };
            User? obj = await service.GetByUniqProps(_objDto);
            if (obj is null) { return NotFound(obj); }
            else { return Ok(obj); }
        }

        [HttpPut("Get/ByProps")] public async Task<ActionResult<List<User>>> GetByProps(UserAddDto objDto)
        {
            AddDto<User> _objDto = new() { Dto = objDto };
            return Ok(await service.GetByProps(_objDto));
        }

        [HttpPut("Get/ByFilter")] public async Task<ActionResult<List<User>>> GetByFilter(UserFilterDtos objDto)
        {
            FilterDtos<User> _objDto = new() { Dto = objDto };
            return Ok(await service.GetByFilter(_objDto.Filter, _objDto.FilterMin, _objDto.FilterMax));
        }

        [HttpGet("Get/Temp")] public async Task<ActionResult<User?>> GetTemp()
        {
            User? obj = await service.GetTemp();
            if (obj is null) { return BadRequest(obj); }
            else { return Ok(obj); }
        }

        [HttpPost("Add")] public async Task<ActionResult<User?>> Add(UserAddDto objDto)
        {
            User? obj = objDto.Dto2Model();
            bool isValid = service.Validate(obj);
            bool isValidUniqProps = await service.ValidateUniqProps(obj);
            if (obj is null) { return BadRequest(obj); }
            else if (!isValidUniqProps) { return StatusCode(208, obj); }
            else if (!isValid) { return StatusCode(406, obj); }
            else
            {
                await service.Add(obj);
                UniqPropsDto<User> uniqPropsDto = new();
                uniqPropsDto.Dto.Model2Dto(obj);
                obj = await service.GetByUniqProps(uniqPropsDto);
                if (obj is null) { return NotFound(obj); }
                else { return Ok(obj); }
            }
        }

        [HttpPut("Update")] public async Task<ActionResult<User?>> Update(UserUpdateDto objDto)
        {
            User? obj = await service.GetById(objDto.Id);
            if (obj is null) { return NotFound(obj); }
            else
            {
                obj = objDto.Dto2Model(obj);
                bool isValid = service.Validate(obj);
                bool isValidUniqProps = await service.ValidateUniqProps(obj);
                if (obj is null) { return BadRequest(await service.GetById(objDto.Id)); }
                else if (!isValidUniqProps) { return StatusCode(208, obj); }
                else if (!isValid) { return StatusCode(406, obj); }
                else
                {
                    await service.Update(obj);
                    await fullService.UpdateChildObjects(typeof(User), obj);
                    return Ok(obj);
                }
            }
        }
    }
}
