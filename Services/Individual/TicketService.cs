using LBV_GTM_Basics;
using LBV_GTM_Basics.Models;
using LBV_GTM_Database_API.EfcContext;
using LBV_GTM_Database_API.Services.Interfaces;

namespace LBV_GTM_Database_API.Services
{
    public class TicketService(ITicketContext iTicketContext,
        IBaseContext<User> iUserContext,
        IBaseContext<Ticket> iBaseContext) : BaseService<Ticket>(iBaseContext)
    {
        public bool Validate(Ticket? obj)
        {
            bool isValid = true;
            if (obj is null) { return false; }

            if (obj.DateAnswer < obj.DateQuestion) { obj.DateAnswer = obj.DateQuestion; isValid = false; }
            else if (obj.DateAnswer > DateTime.UtcNow) { obj.DateAnswer = DateTime.UtcNow; isValid = false; }
            if (obj.UserIdAnswer is not null && obj.UserAnswer is not null) { obj.UserAnswer = iUserContext.GetById((int)obj.UserIdAnswer).Result; }
            ;
            obj.Question = Scripts.RemoveSpaceStartEnd(obj.Question);
            obj.Answer = Scripts.RemoveSpaceStartEnd(obj.Answer);
            if (obj.Question == string.Empty) { obj.Question = nameof(obj.Question); isValid = false; }
            if (obj.UserIdAnswer is null) { obj.UserAnswer = null; obj.Answer = string.Empty; isValid = false; }
            else if (obj.UserAnswer is null) { obj.UserIdAnswer = null; obj.Answer = string.Empty; isValid = false; }
            else if (obj.Answer == string.Empty) { obj.Answer = nameof(obj.Answer); isValid = false; }

            return isValid;
        }

        public async Task<bool> ValidateUniqProps(Ticket? obj)
        {
            bool isValidUniqProps = true;
            if (obj is null) { return false; }

            obj.Title = Scripts.RemoveSpaceStartEnd(obj.Title);
            if (obj.Title == string.Empty) { obj.Title = nameof(obj.Title); isValidUniqProps = false; }
            if (obj.DateQuestion > DateTime.UtcNow) { obj.DateQuestion = DateTime.UtcNow; isValidUniqProps = false; }
            else if (obj.DateQuestion < GlobalValues.DateTimeMinValue) { obj.DateQuestion = GlobalValues.DateTimeMinValue; isValidUniqProps = false; }
            User? user = null;
            if (obj.UserQuestion is not null) { user = iUserContext.GetById(obj.UserIdQuestion).Result; }
            ;
            if (user is null)
            {
                List<User> list = iUserContext.GetAll().Result;
                if (list.Count == 0) { obj = null; return false; }
                else { obj.UserQuestion = list[0]; obj.UserIdQuestion = list[0].Id; isValidUniqProps = false; }
            }
            else { obj.UserQuestion = user; }

            int nr = 1;
            string delimiter = " #";
            string defName = obj.Title;
            string[] defNameList = defName.Split(delimiter);
            if (defNameList.Length > 1 && int.TryParse(defNameList[^1], out _)) { defName = defName[..^(defNameList[^1].Length + delimiter.Length)]; }

            DateTime startDate = obj.DateQuestion;
            while (!await IsUnique(obj))
            {
                isValidUniqProps = false;
                obj.Title = defName + delimiter + nr.ToString();
                nr++;
                if (nr == int.MaxValue)
                {
                    if (obj.DateQuestion < DateTime.UtcNow.AddSeconds(-1)) { obj.DateQuestion = obj.DateQuestion.AddSeconds(1); }
                    else { obj.DateQuestion = GlobalValues.DateTimeMinValue; }
                    if (obj.DateQuestion == startDate) { obj = null; return false; }
                }
            }

            Validate(obj);
            return isValidUniqProps;
        }

        public async Task<Ticket?> GetTemp() { Ticket obj = new(); await ValidateUniqProps(obj); return obj; }
    }
}
