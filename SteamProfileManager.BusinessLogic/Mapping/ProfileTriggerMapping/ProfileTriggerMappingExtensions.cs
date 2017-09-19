using System.Collections.Generic;
using System.Linq;

using SteamProfileManager.DataAccess.DataObjects.ProfileTriggerEntities;
using SteamProfileManager.Models.ProfileTriggers;

namespace SteamProfileManager.BusinessLogic.Mapping
{
    static class ProfileTriggerMappingExtensions
    {
        internal static ProfileTrigger ToDomainModel(this ProfileTriggerEntity profileTriggerEntity)
        {
            ProfileTrigger profileTrigger;

            switch (profileTriggerEntity.Type)
            {
                case nameof(SpecificDateProfileTrigger):
                    profileTrigger = ((SpecificDateProfileTriggerEntity)profileTriggerEntity).ToDomainModel();
                    break;

                default:
                    // TODO: Throw exception
                    profileTrigger = null;
                    break;
            }

            return profileTrigger;
        }

        internal static ProfileTriggerEntity ToEntity(this ProfileTrigger profileTrigger)
        {
            ProfileTriggerEntity profileTriggerEntity;

            switch (profileTrigger.Type.ToString())
            {
                case nameof(SpecificDateProfileTrigger):
                    profileTriggerEntity = ((SpecificDateProfileTrigger)profileTrigger).ToEntity();
                    break;

                default:
                    // TODO: Throw exception
                    profileTriggerEntity = null;
                    break;
            }

            return profileTriggerEntity;
        }

        internal static IEnumerable<ProfileTrigger> ToDomainModels(this IEnumerable<ProfileTriggerEntity> profileTriggerEntities)
        {
            IEnumerable<ProfileTrigger> profileTriggers = profileTriggerEntities.Select(profileTriggerEntity => profileTriggerEntity.ToDomainModel());

            return profileTriggers;
        }

        internal static IEnumerable<ProfileTriggerEntity> ToEntities(this IEnumerable<ProfileTrigger> profileTriggers)
        {
            IEnumerable<ProfileTriggerEntity> profileTriggerEntities = profileTriggers.Select(profileTrigger => profileTrigger.ToEntity());

            return profileTriggerEntities;
        }
    }
}
