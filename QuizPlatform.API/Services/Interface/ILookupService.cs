using QuizPlatform.API.Models.Entity;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface ILookupService
    {
        List<Category> GetAllCategories();

        void CreateCategory(Category category);

        List<Level> GetAllLevels();

        void CreateLevel(Level level);

        List<Difficulty> GetAllDifficulties();

        void CreateDifficulty(Difficulty difficulty);
    }
}