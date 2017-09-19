using SteamProfileManager.DataAccess.DataObjects.ProfileActionEntities;
using SteamProfileManager.Models.ProfileActions;

namespace SteamProfileManager.BusinessLogic.Mapping
{
    internal static class ChangeAvatarProfileActionMappingExtensions
    {
        internal static ChangeAvatarProfileAction ToDomainModel(this ChangeAvatarProfileActionEntity changeAvatarProfileActionEntity)
        {
            ChangeAvatarProfileAction changeAvatarProfileAction = new ChangeAvatarProfileAction
            {
                FilePath = changeAvatarProfileActionEntity.FilePath
            };

            return changeAvatarProfileAction;
        }

        internal static ChangeAvatarProfileActionEntity ToEntity(this ChangeAvatarProfileAction changeAvatarProfileAction)
        {
            ChangeAvatarProfileActionEntity changeAvatarProfileActionEntity = new ChangeAvatarProfileActionEntity
            {
                FilePath = changeAvatarProfileAction.FilePath
            };

            return changeAvatarProfileActionEntity;
        }
    }
}
