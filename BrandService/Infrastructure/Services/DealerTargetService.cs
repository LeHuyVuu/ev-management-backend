using AutoMapper;
using BrandService.DTOs.Requests.DealerTargetDTOs;
using BrandService.DTOs.Responses.DealerTargetDTOs;
using BrandService.Entities;
using BrandService.Infrastructure.Repositories;
using BrandService.Model;

namespace BrandService.Infrastructure.Services
{
    public class DealerTargetService
    {
        private readonly DealerTargetRepository _repo;
        private readonly IMapper _mapper;

        public DealerTargetService(DealerTargetRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PagedResult<DealerTargetResponse>> GetTargetsByDealerAsync(Guid dealerId, int pageNumber, int pageSize)
        {
            var paged = await _repo.GetPagedByDealerAsync(dealerId, pageNumber, pageSize);

            return new PagedResult<DealerTargetResponse>
            {
                Items = _mapper.Map<List<DealerTargetResponse>>(paged.Items),
                TotalItems = paged.TotalItems,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            };
        }


        public async Task<DealerTargetResponse> GetByIdAsync(Guid dealerId, Guid targetId)
        {
            var target = await _repo.GetByIdAsync(dealerId, targetId);
            return _mapper.Map<DealerTargetResponse>(target);
        }

        public async Task<DealerTargetResponse> CreateAsync(Guid dealerId, DealerTargetRequest request)
        {
            var entity = _mapper.Map<DealerTarget>(request);
            entity.DealerId = dealerId;
            entity.AchievedAmount = 0; // mặc định khi tạo
            var created = await _repo.AddAsync(entity);
            return _mapper.Map<DealerTargetResponse>(created);
        }

        public async Task<DealerTargetResponse> UpdateAsync(Guid dealerId, Guid targetId, DealerTargetRequest request)
        {
            var target = await _repo.GetByIdAsync(dealerId, targetId);
            _mapper.Map(request, target);
            var updated = await _repo.UpdateAsync(target);
            return _mapper.Map<DealerTargetResponse>(updated);
        }

        public async Task DeleteAsync(Guid dealerId, Guid targetId)
        {
            var target = await _repo.GetByIdAsync(dealerId, targetId);
            await _repo.DeleteAsync(target);
        }

        public async Task<DealerTargetResponse> UpdateAchievedAmountAsync(Guid dealerId, Guid targetId)
        {
            var target = await _repo.GetByIdAsync(dealerId, targetId);

            var achieved = await _repo.CalculateAchievedAmountAsync(
                target.DealerId,
                target.StartDate,
                target.Period
            );

            target.AchievedAmount = achieved;
            var updated = await _repo.UpdateAsync(target);

            return _mapper.Map<DealerTargetResponse>(updated);
        }
    }

}
