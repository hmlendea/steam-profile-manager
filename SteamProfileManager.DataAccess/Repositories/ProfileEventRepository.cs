using System.Collections.Generic;
using System.Linq;

using SteamProfileManager.DataAccess.Exceptions;
using SteamProfileManager.DataAccess.IO;
using SteamProfileManager.DataAccess.Repositories.Interfaces;
using SteamProfileManager.Models;

namespace SteamProfileManager.DataAccess.Repositories
{
    public class ProfileEventRepository : IProfileEventRepository
    {
        readonly XmlDatabase<ProfileEvent> xmlDatabase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileEventRepository"/> class.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public ProfileEventRepository(string fileName)
        {
            xmlDatabase = new XmlDatabase<ProfileEvent>(fileName);
        }

        public void Add(ProfileEvent profileEventEntity)
        {
            List<ProfileEvent> profileEventEntities = xmlDatabase.LoadEntities().ToList();
            profileEventEntities.Add(profileEventEntity);

            try
            {
                xmlDatabase.SaveEntities(profileEventEntities);
            }
            catch
            {
                throw new DuplicateEntityException(profileEventEntity.Id, nameof(ProfileEvent).Replace("Entity", ""));
            }
        }

        public ProfileEvent Get(string id)
        {
            List<ProfileEvent> profileEventEntities = xmlDatabase.LoadEntities().ToList();
            ProfileEvent profileEventEntity = profileEventEntities.FirstOrDefault(x => x.Id == id);

            if (profileEventEntity == null)
            {
                throw new EntityNotFoundException(id, nameof(ProfileEvent).Replace("Entity", ""));
            }

            return profileEventEntity;
        }

        public IEnumerable<ProfileEvent> GetAll()
        {
            List<ProfileEvent> profileEventEntities = xmlDatabase.LoadEntities().ToList();

            return profileEventEntities;
        }

        public void Update(ProfileEvent profileEventEntity)
        {
            List<ProfileEvent> profileEventEntities = xmlDatabase.LoadEntities().ToList();
            ProfileEvent profileEventEntityToUpdate = profileEventEntities.FirstOrDefault(x => x.Id == profileEventEntity.Id);

            if (profileEventEntityToUpdate == null)
            {
                throw new EntityNotFoundException(profileEventEntity.Id, nameof(ProfileEvent).Replace("Entity", ""));
            }

            profileEventEntityToUpdate.Trigger = profileEventEntity.Trigger;
            profileEventEntityToUpdate.Action = profileEventEntity.Action;

            xmlDatabase.SaveEntities(profileEventEntities);
        }

        public void Remove(string id)
        {
            List<ProfileEvent> profileEventEntities = xmlDatabase.LoadEntities().ToList();
            profileEventEntities.RemoveAll(e => e.Id == id);

            try
            {
                xmlDatabase.SaveEntities(profileEventEntities);
            }
            catch
            {
                throw new DuplicateEntityException(id, nameof(ProfileEvent).Replace("Entity", ""));
            }
        }
    }
}
