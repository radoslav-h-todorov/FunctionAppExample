﻿using System.Threading.Tasks;
using FunctionAppExample.Models;
using FunctionAppExample.ResponseDtos;

namespace FunctionAppExample.Services;

public interface ICategoriesService
{
    Task<string> AddCategoryAsync(string name, string userId);
    Task<DeleteCategoryResult> DeleteCategoryAsync(string categoryId, string userId);
    Task<UpdateCategoryResult> UpdateCategoryAsync(string categoryId, string userId, string name);
    Task<CategoryDetails> GetCategoryAsync(string categoryId, string userId);
    Task<CategorySummaries> ListCategoriesAsync(string userId);
    Task<bool> UpdateCategoryImageAsync(string categoryId, string userId);
}