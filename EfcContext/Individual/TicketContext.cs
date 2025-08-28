using Microsoft.EntityFrameworkCore;

using LBV_GTM_Database_API.Data;
using LBV_GTM_Database_API.Services.Interfaces;

namespace LBV_GTM_Database_API.EfcContext
{
    public class TicketContext(DataContext db) : ITicketContext
    {

    }
}
