using Microsoft.AspNetCore.Mvc;

using LBV_Basics.Models;
using LBV_GTM_Database_API.Services;
using LBV_Basics.Models.DTOs;
using LBV_Basics;

namespace LBV_GTM_Database_API.Controllers
{
    [ApiController]
    [Route(nameof(Ticket))]
    public class TicketController(TicketService service, BaseService<Ticket> baseService, FullService<Ticket> fullService) : BaseController<Ticket>(baseService, fullService)
    {
        [HttpPut("Get/ByUniqProps/0")] public async Task<ActionResult<Ticket?>> GetByUniqProps(TicketUniqPropsDto0 objDto)
        {
            UniqPropsDto<Ticket> _objDto = new() { Index = 0, Dto = objDto };
            Ticket? obj = await service.GetByUniqProps(_objDto);
            if (obj is null) { return NotFound(obj); }
            else { return Ok(obj); }
        }

        [HttpPut("Get/ByProps")] public async Task<ActionResult<List<Ticket>>> GetByProps(TicketAddDto objDto)
        {
            AddDto<Ticket> _objDto = new() { Dto = objDto };
            return Ok(await service.GetByProps(_objDto));
        }

        [HttpPut("Get/ByFilter")] public async Task<ActionResult<List<Ticket>>> GetByFilter(TicketFilterDtos objDto)
        {
            FilterDtos<Ticket> _objDto = new() { Dto = objDto };
            return Ok(await service.GetByFilter(_objDto.Filter, _objDto.FilterMin, _objDto.FilterMax));
        }

        [HttpGet("Get/Temp")] public async Task<ActionResult<Ticket?>> GetTemp()
        {
            Ticket? obj = await service.GetTemp();
            if (obj is null) { return BadRequest(obj); }
            else { return Ok(obj); }
        }

        [HttpPost("Add")] public async Task<ActionResult<Ticket?>> Add(TicketAddDto objDto)
        {
            Ticket? obj = objDto.Dto2Model();
            bool isValid = service.Validate(obj);
            bool isValidUniqProps = await service.ValidateUniqProps(obj);
            if (obj is null) { return BadRequest(obj); }
            else if (!isValidUniqProps) { return StatusCode(208, obj); }
            else if (!isValid) { return StatusCode(406, obj); }
            else
            {
                await service.Add(obj);
                UniqPropsDto<Ticket> uniqPropsDto = new();
                uniqPropsDto.Dto.Model2Dto(obj);
                obj = await service.GetByUniqProps(uniqPropsDto);
                if (obj is null) { return NotFound(obj); }
                else { return Ok(obj); }
            }
        }

        [HttpPut("Update")] public async Task<ActionResult<Ticket?>> Update(TicketUpdateDto objDto)
        {
            Ticket? obj = await service.GetById(objDto.Id);
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
                    await fullService.UpdateChildObjects(typeof(Ticket), obj);
                    return Ok(obj);
                }
            }
        }
    }
}
