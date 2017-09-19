using System.Collections.Generic;

using SteamProfileManager.DataAccess.DataObjects;

namespace SteamProfileManager.DataAccess.Repositories.Interfaces
{
    public interface IProfileEventRepository
    {
        void Add(ProfileEventEntity profileEventEntity);

        ProfileEventEntity Get(string id);

        IEnumerable<ProfileEventEntity> GetAll();

        void Update(ProfileEventEntity profileEventEntity);

        void Remove(string id);
    }
}
