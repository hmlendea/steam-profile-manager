using System.Collections.Generic;
using System.Linq;

using SteamProfileManager.DataAccess.DataObjects;
using SteamProfileManager.Models;

namespace SteamProfileManager.BusinessLogic.Mapping
{
    static class ProfileEventMappingExtensions
    {
        internal static ProfileEvent ToDomainModel(this ProfileEventEntity profileEventEntity)
        {
            ProfileEvent profileEvent = new ProfileEvent
            {
                Id = profileEventEntity.Id,
                Trigger = profileEventEntity.Trigger.ToDomainModel()
            };

            return profileEvent;
        }

        internal static ProfileEventEntity ToEntity(this ProfileEvent profileEvent)
        {
            ProfileEventEntity profileEventEntity = new ProfileEventEntity
            {
                Id = profileEvent.Id,
                Trigger = profileEvent.Trigger.ToEntity(),
                Action = profileEvent.Action.ToEntity()
            };

            return profileEventEntity;
        }

        internal static IEnumerable<ProfileEvent> ToDomainModels(this IEnumerable<ProfileEventEntity> profileEventEntities)
        {
            IEnumerable<ProfileEvent> profileEvents = profileEventEntities.Select(profileEventEntity => profileEventEntity.ToDomainModel());

            return profileEvents;
        }

        internal static IEnumerable<ProfileEventEntity> ToEntities(this IEnumerable<ProfileEvent> profileEvents)
        {
            IEnumerable<ProfileEventEntity> profileEventEntities = profileEvents.Select(profileEvent => profileEvent.ToEntity());

            return profileEventEntities;
        }
    }
}
