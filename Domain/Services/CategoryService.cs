﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Servize.Domain.Model.Provider;
using Servize.Domain.Repositories;
using Servize.DTO.PROVIDER;
using Servize.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servize.Domain.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _respository;
        private readonly ContextTransaction _transacation;
        private readonly IMapper _mapper;
      

        public CategoryService(ServizeDBContext dbcontext,
            IMapper mapper, ContextTransaction transaction)
        {
            _respository = new CategoryRepository(dbcontext);
            _transacation = transaction;
            _mapper = mapper;
        }
        public async Task<Response<IList<CategoryDTO>>> GetAllCategoryList()
        {
            try
            {
                Response<IList<Category>> response = await _respository.GetAllServizeCategoryList();

                if (response.IsSuccessStatusCode())
                {
                    IList<CategoryDTO> serviceDTO = _mapper.Map<IList<Category>, IList<CategoryDTO>>(response.Resource);
                    return new Response<IList<CategoryDTO>>(serviceDTO, StatusCodes.Status200OK);
                }

                return new Response<IList<CategoryDTO>>("Failed to Load ServizeCategory List", response.StatusCode);
            }
            catch (Exception e)
            {
                return new Response<IList<CategoryDTO>>($"Failed to Load ServizeCategory List Error:{e.Message}", StatusCodes.Status500InternalServerError);
            }
        }


        public async Task<Response<CategoryDTO>> GetAllCategoryById(int Id)
        {
            try
            {
                Response<Category> response = await _respository.GetAllServizeCategoryById(Id);
                if (response.IsSuccessStatusCode())
                {
                    CategoryDTO serviceDTO = _mapper.Map<Category, CategoryDTO>(response.Resource);
                    return new Response<CategoryDTO>(serviceDTO, StatusCodes.Status200OK);
                }
                return new Response<CategoryDTO>("Failed to Load ServizeCategory With Specific Id", response.StatusCode);
            }
            catch (Exception e)
            {
                return new Response<CategoryDTO>($"Failed to Load ServizeCategory Error:{e.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Response<CategoryDTO>> AddServiceCategory(CategoryDTO servizeCategoryDTO )
        {
            try
            {
                Category serviceCategory = _mapper.Map<CategoryDTO, Category>(servizeCategoryDTO);

                Response<Category> response = await _respository.AddServiceCategory(serviceCategory);
                if (response.IsSuccessStatusCode())
                {
                    await _transacation.CompleteAsync();
                    CategoryDTO serviceDTO = _mapper.Map<Category, CategoryDTO>(response.Resource);
                    return new Response<CategoryDTO>(serviceDTO, StatusCodes.Status200OK);
                }
                return new Response<CategoryDTO>("Failed to Add ServizeCategory With Specific Id", response.StatusCode);
            }
            catch (Exception e)
            {
                return new Response<CategoryDTO>($"Failed to Add ServizeCategory Error:{e.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Response<CategoryDTO>> UpdateServiceCategory(CategoryDTO servizeCategoryDTO)
        {
            try
            {
                Category serviceCategory = _mapper.Map<CategoryDTO, Category>(servizeCategoryDTO);

                Response<Category> response = await _respository.UpdateServiceCategory(serviceCategory);
                if (response.IsSuccessStatusCode())
                {
                    await _transacation.CompleteAsync();
                    CategoryDTO serviceDTO = _mapper.Map<Category, CategoryDTO>(response.Resource);
                    return new Response<CategoryDTO>(serviceDTO, StatusCodes.Status200OK);
                }
                return new Response<CategoryDTO>("Failed to Add ServizeCategory With Specific Id", response.StatusCode);
            }
            catch (Exception e)
            {
                return new Response<CategoryDTO>($"Failed to Add ServizeCategory Error:{e.Message}", StatusCodes.Status500InternalServerError);
            }
        }

    }
}
