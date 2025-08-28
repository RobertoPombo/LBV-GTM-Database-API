using LBV_Basics;
using LBV_Basics.Models;
using LBV_Basics.Models.DTOs;
using LBV_GTM_Database_API.Services.Interfaces;

namespace LBV_GTM_Database_API.Services
{
    public class UserService(IUserContext iUserContext,
        IBaseContext<User> iBaseContext) : BaseService<User>(iBaseContext)
    {
        public bool Validate(User? obj)
        {
            bool isValid = true;
            if (obj is null) { return false; }

            obj.FirstName = Scripts.RemoveSpaceStartEnd(obj.FirstName);
            obj.LastName = Scripts.RemoveSpaceStartEnd(obj.LastName);
            obj.EmailAdress = Scripts.RemoveSpaceStartEnd(obj.EmailAdress);
            obj.PhoneNumber = Scripts.RemoveSpaceStartEnd(obj.PhoneNumber);

            return isValid;
        }

        public async Task<bool> ValidateUniqProps(User? obj)
        {
            bool isValidUniqProps = true;
            if (obj is null) { return false; }

            obj.WindowsUserId = Scripts.RemoveSpaceStartEnd(obj.WindowsUserId);
            if (obj.WindowsUserId == string.Empty) { obj.WindowsUserId = nameof(obj.WindowsUserId); isValidUniqProps = false; }

            int nr = 1;
            string delimiter = " #";
            string defId = obj.WindowsUserId;
            string[] defNameList = defId.Split(delimiter);
            if (defNameList.Length > 1 && int.TryParse(defNameList[^1], out _)) { defId = defId[..^(defNameList[^1].Length + delimiter.Length)]; }
            while (!await IsUnique(obj))
            {
                isValidUniqProps = false;
                obj.WindowsUserId = defId + delimiter + nr.ToString();
                nr++;
                if (nr == int.MaxValue) { obj = null; return false; }
            }

            Validate(obj);
            return isValidUniqProps;
        }

        public async Task<User?> GetTemp() { User obj = new(); await ValidateUniqProps(obj); return obj; }
    }
}
