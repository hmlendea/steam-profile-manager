using System;

using SteamProfileManager.DataAccess.DataObjects.ProfileTriggerEntities;
using SteamProfileManager.Models.ProfileTriggers;

namespace SteamProfileManager.BusinessLogic.Mapping
{
    internal static class SpecificDateProfileTriggerMappingExtensions
    {
        internal static SpecificDateProfileTrigger ToDomainModel(this SpecificDateProfileTriggerEntity specificDateProfileTriggerEntity)
        {
            SpecificDateProfileTrigger specificDateProfileTrigger = new SpecificDateProfileTrigger
            {
                Date = DateTime.Parse(specificDateProfileTriggerEntity.Date)
            };

            return specificDateProfileTrigger;
        }

        internal static SpecificDateProfileTriggerEntity ToEntity(this SpecificDateProfileTrigger specificDateProfileTrigger)
        {
            SpecificDateProfileTriggerEntity specificDateProfileTriggerEntity = new SpecificDateProfileTriggerEntity
            {
                Date = specificDateProfileTrigger.Date.ToString()
            };

            return specificDateProfileTriggerEntity;
        }
    }
}
