﻿using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DorelAppBackend.Services.Implementation
{
    public class ReviewService: IReviewService
    {
        private DorelDbContext _dorelDbContext;
        private IBlobStorageService _blobStorageService;
        private ILoginService _loginService;

        public ReviewService(DorelDbContext dorelDbContext, IBlobStorageService blobStorageService, ILoginService loginService)
        {
            _dorelDbContext = dorelDbContext;
            _blobStorageService = blobStorageService;
            _loginService = loginService;
        }

        public async Task<Maybe<string>> PostReview(string userEmail, int reviewedUserId, int serviciuId, decimal rating, string description)
        {
            var maybe = new Maybe<string>();
            var user = await _dorelDbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            var reviewee = await _dorelDbContext.Users.AnyAsync(u => u.UserID == reviewedUserId);
            if (user != null && reviewee)
            {
                var entry = await _dorelDbContext.Reviews.AnyAsync(r => r.ReviewerUserId == user.UserID && r.ReviewedUserId == reviewedUserId && r.ServiciuId == serviciuId);
                if (!entry)
                {
                    var data = new DBReviewModel()
                    {
                        ReviewerUserId = user.UserID,
                        ReviewedUserId = reviewedUserId,
                        ServiciuId = serviciuId,
                        Rating = rating,
                        ReviewDescription = description
                    };
                    await _dorelDbContext.Reviews.AddAsync(data);
                    await _dorelDbContext.SaveChangesAsync();
                    maybe.SetSuccess("Success");
                }
                else
                {
                    maybe.SetException("Review already exists");
                }
            }
            else
            {
                maybe.SetException("User or reviewee does not exist");
            }
            return maybe;
        }

        public async Task<Maybe<DBReviewModel[]>> GetReviews(int reviewedUserId, int serviciuId, int pageNumber)
        {
            var maybe = new Maybe<DBReviewModel[]>();
            int PAGE_SIZE = 10;

            var reviewee = await _dorelDbContext.Users.AnyAsync(u => u.UserID == reviewedUserId);
            if (reviewee)
            {
                var list =  await _dorelDbContext.Reviews.Where(r => r.ReviewedUserId == reviewedUserId && r.ServiciuId == serviciuId).Skip(pageNumber * PAGE_SIZE).Take(PAGE_SIZE).ToArrayAsync();
                foreach(var item in list)
                {
                    var reviewer = await _dorelDbContext.Users.FirstOrDefaultAsync(e => e.UserID == item.ReviewerUserId);
                    if(reviewer != null)
                    {
                        item.ReviewerName = reviewer.Name;
                    }
                    
                }
                maybe.SetSuccess(list);
            }
            else
            {
                maybe.SetException("reviewee does not exist");
            }
            return maybe;
        }
    }
}
