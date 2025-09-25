using AutoMapper;
using BrandService.DTOs;
using BrandService.Entities;
using BrandService.Infrastructure.Repositories;

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

        public async Task<List<DealerDto.DealerResponse>> GetAllAsync()
        {
            var dealers = await _repo.GetAllAsync();
            return _mapper.Map<List<DealerDto.DealerResponse>>(dealers);
        }

        public async Task<DealerDto.DealerResponse?> GetByIdAsync(Guid id)
        {
            var dealer = await _repo.GetByIdAsync(id);
            return dealer == null ? null : _mapper.Map<DealerDto.DealerResponse>(dealer);
        }

        public async Task<DealerDto.DealerResponse> CreateAsync(DealerDto.DealerRequest dealerRequest)
        {
            var dealer = _mapper.Map<Dealer>(dealerRequest);
            dealer.DealerId = Guid.NewGuid();
            var created = await _repo.AddAsync(dealer);
            return _mapper.Map<DealerDto.DealerResponse>(created);
        }

        public async Task<DealerDto.DealerResponse> UpdateAsync(Guid id, DealerDto.DealerRequest dealerRequest)
        {
            var dealer = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Dealer not found");
            dealer = _mapper.Map(dealerRequest, dealer);
            var updated = await _repo.UpdateAsync(dealer);
            return _mapper.Map<DealerDto.DealerResponse>(updated);
        }

        public async Task<List<DealerTargetDto>> GetTargetsAsync(Guid dealerId)
        {
            var targets = await _repo.GetTargetsAsync(dealerId);
            return _mapper.Map<List<DealerTargetDto>>(targets);
        }
    }
}
