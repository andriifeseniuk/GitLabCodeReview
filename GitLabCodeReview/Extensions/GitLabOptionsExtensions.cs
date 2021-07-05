using System;
using System.Collections.Generic;

namespace GitLabCodeReview.Extensions
{
    public static class GitLabOptionsExtensions
    {
        private const string Comma = ",";

        public static bool IsFavoriteProject(this GitLabOptions options, long projectId)
        {
            if (options == null || options.FavoriteProjects == null)
            {
                return false;
            }

            var favoriteStrings = options.FavoriteProjects.Split(new[] { Comma }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var idString in favoriteStrings)
            {
                long tempId;
                if(long.TryParse(idString, out tempId) && tempId == projectId)
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetFavoriteProjects(this GitLabOptions options, IEnumerable<long> ids)
        {
            if (options == null)
            {
                return;
            }

            var favoritesString = string.Join(Comma, ids);
            options.FavoriteProjects = favoritesString;
        }
    }
}
