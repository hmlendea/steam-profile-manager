using System.Collections.Generic;

using SteamProfileManager.Models;

namespace SteamProfileManager.DataAccess.Repositories.Interfaces
{
    public interface IProfileEventRepository
    {
        void Add(ProfileEvent profileEventEntity);

        ProfileEvent Get(string id);

        IEnumerable<ProfileEvent> GetAll();

        void Update(ProfileEvent profileEventEntity);

        void Remove(string id);
    }
}
