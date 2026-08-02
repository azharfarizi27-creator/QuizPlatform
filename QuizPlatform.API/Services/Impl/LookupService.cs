using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class LookupService : BaseServices, ILookupService
    {
        public LookupService()
            : base()
        {

        }

        public List<Category> GetAllCategories()
        {
            return context.Categories
                .OrderBy(x => x.Name)
                .ToList();
        }

        public void CreateCategory(Category category)
        {
            if (category == null)
                throw new Exception("Data category kosong");

            if (string.IsNullOrWhiteSpace(category.Name))
                throw new Exception("Nama category wajib diisi");

            category.CreatedAt =
                DateTime.Now;

            context.Categories.Add(category);
            context.SaveChanges();
        }

        public List<Level> GetAllLevels()
        {
            return context.Levels
                .OrderBy(x => x.Id)
                .ToList();
        }

        public void CreateLevel(Level level)
        {
            if (level == null)
                throw new Exception("Data level kosong");

            context.Levels.Add(level);
            context.SaveChanges();
        }

        public List<Difficulty> GetAllDifficulties()
        {
            return context.Difficulties
                .OrderBy(x => x.Id)
                .ToList();
        }

        public void CreateDifficulty(Difficulty difficulty)
        {
            if (difficulty == null)
                throw new Exception("Data difficulty kosong");

            context.Difficulties.Add(difficulty);
            context.SaveChanges();
        }
    }
}