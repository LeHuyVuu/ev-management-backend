using AutoMapper;
using DealerService.DTOs.Requests.DealerDTOs;
using DealerService.DTOs.Responses.DealerDTOs;
using DealerService.Infrastructure.Repositories;
using DealerService.Models;
using DealerService.Entities;

namespace DealerService.Infrastructure.Services
{
    public class DealerAppService
    {
        private readonly DealerRepository _dealerRepo;
        private readonly IMapper _mapper;

        public DealerAppService(DealerRepository repo, IMapper mapper)
        {
            _dealerRepo = repo;
            _mapper = mapper;
        }

        public async Task<PagedResult<DealerResponse>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var paged = await _dealerRepo.GetPagedAsync(pageNumber, pageSize);

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
            var dealer = await _dealerRepo.GetByIdAsync(id);
            return _mapper.Map<DealerResponse>(dealer);
        }

        public async Task<DealerResponse> CreateAsync(DealerRequest dealerRequest)
        {
            var dealer = _mapper.Map<Dealer>(dealerRequest);
            dealer.DealerId = Guid.NewGuid();
            var created = await _dealerRepo.AddAsync(dealer);
            return _mapper.Map<DealerResponse>(created);
        }

        public async Task<DealerResponse?> UpdateAsync(Guid id, DealerRequest dealerRequest)
        {
            var dealer = await _dealerRepo.GetByIdAsync(id);
            dealer = _mapper.Map(dealerRequest, dealer);
            var updated = await _dealerRepo.UpdateAsync(dealer);
            return _mapper.Map<DealerResponse>(updated);
        }
    }
}
