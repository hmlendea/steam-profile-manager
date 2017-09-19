using System.Collections.Generic;
using System.Linq;

using SteamProfileManager.DataAccess.DataObjects.ProfileActionEntities;
using SteamProfileManager.Models.ProfileActions;

namespace SteamProfileManager.BusinessLogic.Mapping
{
    static class ProfileActionMappingExtensions
    {
        internal static ProfileAction ToDomainModel(this ProfileActionEntity profileActionEntity)
        {
            ProfileAction profileAction;

            switch (profileActionEntity.Type)
            {
                case nameof(ChangeAvatarProfileAction):
                    profileAction = ((ChangeAvatarProfileActionEntity)profileActionEntity).ToDomainModel();
                    break;

                default:
                    // TODO: Throw exception
                    profileAction = null;
                    break;
            }

            return profileAction;
        }

        internal static ProfileActionEntity ToEntity(this ProfileAction profileAction)
        {
            ProfileActionEntity profileActionEntity;

            switch (profileAction.Type.ToString())
            {
                case nameof(ChangeAvatarProfileAction):
                    profileActionEntity = ((ChangeAvatarProfileAction)profileAction).ToEntity();
                    break;

                default:
                    // TODO: Throw exception
                    profileActionEntity = null;
                    break;
            }

            return profileActionEntity;
        }

        internal static IEnumerable<ProfileAction> ToDomainModels(this IEnumerable<ProfileActionEntity> profileActionEntities)
        {
            IEnumerable<ProfileAction> profileActions = profileActionEntities.Select(profileActionEntity => profileActionEntity.ToDomainModel());

            return profileActions;
        }

        internal static IEnumerable<ProfileActionEntity> ToEntities(this IEnumerable<ProfileAction> profileActions)
        {
            IEnumerable<ProfileActionEntity> profileActionEntities = profileActions.Select(profileAction => profileAction.ToEntity());

            return profileActionEntities;
        }
    }
}
