using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using Transacciones.Core.DTOs.Transacciones;
using Transacciones.Core.Entities;
using Transacciones.Core.Exceptions.Cuentas;
using Transacciones.Core.Exceptions.Transacciones;
using Transacciones.Core.Interfaces.IData;
using Transacciones.Core.Interfaces.IRepositories.Transacciones;
using Transacciones.Core.Interfaces.IServices.Transacciones;

namespace Transacciones.Core.Services.Transacciones;

public class CuentaService : ICuentaService
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IMapper _mapper;
    private readonly ITransaccionesDbContext _context;

    public CuentaService(
        ICuentaRepository cuentaRepository,
        IMapper mapper,
        ITransaccionesDbContext context)
    {
        _cuentaRepository = cuentaRepository;
        _mapper = mapper;
        _context = context;
    }

    public async Task<CuentaDto?> GetByIdAsync(int id)
    {
        var cuenta = await _cuentaRepository.GetByIdAsync(id);
        return cuenta == null ? null : _mapper.Map<CuentaDto>(cuenta);
    }

    public async Task<CuentaDto?> GetByNumeroCuentaAsync(string numeroCuenta)
    {
        var cuenta = await _cuentaRepository.GetByNumeroCuentaAsync(numeroCuenta);
        return cuenta == null ? null : _mapper.Map<CuentaDto>(cuenta);
    }

    public async Task<CuentaDto> CreateAsync(CrearCuentaDto crearCuentaDto)
    {
        // Validar saldo inicial >= 0
        if (crearCuentaDto.SaldoInicial < 0)
        {
            throw new SaldoInicialNegativoException();
        }

        // Validar que el número de cuenta no exista
        if (await _cuentaRepository.ExistsByNumeroCuentaAsync(crearCuentaDto.NumeroCuenta))
        {
            throw new NumeroCuentaDuplicadoException(crearCuentaDto.NumeroCuenta);
        }

        var cuenta = _mapper.Map<Cuenta>(crearCuentaDto);
        
        // asegurar que el saldo no sea negativo 
        if (cuenta.Saldo < 0)
        {
            throw new SaldoCuentaNegativoException();
        }

        cuenta = await _cuentaRepository.CreateAsync(cuenta);
        return _mapper.Map<CuentaDto>(cuenta);
    }
}





