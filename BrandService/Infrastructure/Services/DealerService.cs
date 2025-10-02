using Application.ExceptionHandler;
using AutoMapper;
using BrandService.DTOs.Requests.DealerDTOs;
using BrandService.DTOs.Responses.DealerDTOs;
using BrandService.Entities;
using BrandService.Infrastructure.Repositories;
using BrandService.Model;

namespace BrandService.Infrastructure.Services
{
    public class DealerService
    {
        private readonly DealerRepository _repo;
        private readonly IMapper _mapper;

        public DealerService(DealerRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PagedResult<DealerResponse>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var paged = await _repo.GetPagedAsync(pageNumber, pageSize);

            return new PagedResult<DealerResponse>
            {
                Items = _mapper.Map<List<DealerResponse>>(paged.Items),
                TotalItems = paged.TotalItems,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
            };
        }

        public async Task<DealerResponse?> GetByIdAsync(Guid id)
        {
            var dealer = await _repo.GetByIdAsync(id);
            return _mapper.Map<DealerResponse>(dealer);
        }

        public async Task<DealerResponse> CreateAsync(DealerRequest dealerRequest)
        {
            var dealer = _mapper.Map<Dealer>(dealerRequest);
            dealer.DealerId = Guid.NewGuid();
            var created = await _repo.AddAsync(dealer);
            return _mapper.Map<DealerResponse>(created);
        }

        public async Task<DealerResponse?> UpdateAsync(Guid id, DealerRequest dealerRequest)
        {
            var dealer = await _repo.GetByIdAsync(id);
            dealer = _mapper.Map(dealerRequest, dealer);
            var updated = await _repo.UpdateAsync(dealer);
            return _mapper.Map<DealerResponse>(updated);
        }

        //public async Task<List<DealerTargetDto>> GetTargetsAsync(Guid dealerId)
        //{
        //    var targets = await _repo.GetTargetsAsync(dealerId);
        //    return _mapper.Map<List<DealerTargetDto>>(targets);
        //}
    }
}
