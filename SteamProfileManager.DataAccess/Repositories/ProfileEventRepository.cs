using System.Collections.Generic;
using System.Linq;

using SteamProfileManager.DataAccess.DataObjects;
using SteamProfileManager.DataAccess.Exceptions;
using SteamProfileManager.DataAccess.IO;
using SteamProfileManager.DataAccess.Repositories.Interfaces;

namespace SteamProfileManager.DataAccess.Repositories
{
    public class ProfileEventRepository : IProfileEventRepository
    {
        readonly XmlDatabase<ProfileEventEntity> xmlDatabase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileEventRepository"/> class.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public ProfileEventRepository(string fileName)
        {
            xmlDatabase = new XmlDatabase<ProfileEventEntity>(fileName);
        }

        public void Add(ProfileEventEntity profileEventEntity)
        {
            List<ProfileEventEntity> profileEventEntities = xmlDatabase.LoadEntities().ToList();
            profileEventEntities.Add(profileEventEntity);

            try
            {
                xmlDatabase.SaveEntities(profileEventEntities);
            }
            catch
            {
                throw new DuplicateEntityException(profileEventEntity.Id, nameof(ProfileEventEntity).Replace("Entity", ""));
            }
        }

        public ProfileEventEntity Get(string id)
        {
            List<ProfileEventEntity> profileEventEntities = xmlDatabase.LoadEntities().ToList();
            ProfileEventEntity profileEventEntity = profileEventEntities.FirstOrDefault(x => x.Id == id);

            if (profileEventEntity == null)
            {
                throw new EntityNotFoundException(id, nameof(ProfileEventEntity).Replace("Entity", ""));
            }

            return profileEventEntity;
        }

        public IEnumerable<ProfileEventEntity> GetAll()
        {
            List<ProfileEventEntity> profileEventEntities = xmlDatabase.LoadEntities().ToList();

            return profileEventEntities;
        }

        public void Update(ProfileEventEntity profileEventEntity)
        {
            List<ProfileEventEntity> profileEventEntities = xmlDatabase.LoadEntities().ToList();
            ProfileEventEntity profileEventEntityToUpdate = profileEventEntities.FirstOrDefault(x => x.Id == profileEventEntity.Id);

            if (profileEventEntityToUpdate == null)
            {
                throw new EntityNotFoundException(profileEventEntity.Id, nameof(ProfileEventEntity).Replace("Entity", ""));
            }

            profileEventEntityToUpdate.Trigger = profileEventEntity.Trigger;
            profileEventEntityToUpdate.Action = profileEventEntity.Action;

            xmlDatabase.SaveEntities(profileEventEntities);
        }

        public void Remove(string id)
        {
            List<ProfileEventEntity> profileEventEntities = xmlDatabase.LoadEntities().ToList();
            profileEventEntities.RemoveAll(e => e.Id == id);

            try
            {
                xmlDatabase.SaveEntities(profileEventEntities);
            }
            catch
            {
                throw new DuplicateEntityException(id, nameof(ProfileEventEntity).Replace("Entity", ""));
            }
        }
    }
}
