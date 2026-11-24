
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venta_Autopar_TPI.Core.DTOs;
using Venta_Autopar_TPI.Core.Repositories.Interfaces;
using Venta_Autopar_TPI.Core.Services.Interfaces;
using Venta_Autopar_TPI.Models;

namespace Venta_Autopar_TPI.Core.Services.Implementations
{
    public class AutoparteService : IAutoparteService
    {
        private readonly IAutoparteRepository _autoparteRepository;
        private readonly IHistorialPrecioRepository _historialPrecioRepository;

        public AutoparteService(
            IAutoparteRepository autoparteRepository,
            IHistorialPrecioRepository historialPrecioRepository)
        {
            _autoparteRepository = autoparteRepository;
            _historialPrecioRepository = historialPrecioRepository;
        }

        public async Task<List<AutoparteResponseDto>> GetAllAsync()
        {
            var autopartes = await _autoparteRepository.GetAllAsync();

            return autopartes.Select(a => new AutoparteResponseDto
            {
                AutoparteId = a.AutoparteId,
                CategoriaAutoparteId = a.CategoriaAutoparteId,
                Codigo = a.Codigo,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Marca = a.Marca,
                AplicacionModelo = a.AplicacionModelo,
                Estado = a.Estado,
                FechaAlta = a.FechaAlta,
                StockActual = a.Inventario?.StockActual ?? 0,
                StockMinimo = a.Inventario?.StockMinimo ?? 0,
                PrecioActual = a.HistorialPrecios
                    .OrderByDescending(h => h.FechaDesde)
                    .Select(h => h.Precio)
                    .FirstOrDefault()

            }).ToList();
        }
        public async Task<AutoparteResponseDto?> GetByIdAsync(int id)
        {
            var autoparte = await _autoparteRepository.GetByIdAsync(id);

            if (autoparte == null)
                return null;

            return new AutoparteResponseDto
            {
                AutoparteId = autoparte.AutoparteId,
                CategoriaAutoparteId = autoparte.CategoriaAutoparteId,
                Codigo = autoparte.Codigo,
                Nombre = autoparte.Nombre,
                Descripcion = autoparte.Descripcion,
                Marca = autoparte.Marca,
                AplicacionModelo = autoparte.AplicacionModelo,
                Estado = autoparte.Estado,
                FechaAlta = autoparte.FechaAlta,
                StockActual = autoparte.Inventario?.StockActual ?? 0,
                StockMinimo = autoparte.Inventario?.StockMinimo ?? 0,
                PrecioActual = autoparte.HistorialPrecios
                    .OrderByDescending(h => h.FechaDesde)
                    .Select(h => h.Precio)
                    .FirstOrDefault()
            };
        }
        public async Task CreateAsync(CreateAutoparteDto autoparteDto)
        {
            if (autoparteDto == null)
                throw new ArgumentNullException(nameof(autoparteDto));

            if (autoparteDto.CategoriaAutoparteId <= 0)
                throw new ArgumentException("La categoría seleccionada no es válida.");

            if (string.IsNullOrWhiteSpace(autoparteDto.Codigo))
                throw new ArgumentException("El código de la autoparte es obligatorio.");

            if (string.IsNullOrWhiteSpace(autoparteDto.Nombre))
                throw new ArgumentException("El nombre de la autoparte es obligatorio");

            if (autoparteDto.StockInicial < 0)
                throw new ArgumentException("El stock inicial no puede ser negativo");

            if (autoparteDto.StockMinimo < 0)
                throw new ArgumentException("El stock mínimo no puede ser negativo");

            if (autoparteDto.PrecioInicial <= 0)
                throw new ArgumentException("El precio inicial debe ser mayor que cero");

            var code = autoparteDto.Codigo.Trim();

            if (await _autoparteRepository.CodeExistsAsync(code))
                throw new InvalidOperationException("Ya existe una autoparte con ese código");

            if (!await _autoparteRepository.CategoryExistsAsync(
                autoparteDto.CategoriaAutoparteId))
                throw new ArgumentException("La categoría seleccionada no existe");

            var autoparte = new Autoparte
            {
                CategoriaAutoparteId = autoparteDto.CategoriaAutoparteId,
                Codigo = code,
                Nombre = autoparteDto.Nombre.Trim(),
                Descripcion = autoparteDto.Descripcion,
                Marca = autoparteDto.Marca,
                AplicacionModelo = autoparteDto.AplicacionModelo,
                Estado = true,
                FechaAlta = DateTime.Now
            };

            var inventario = new Inventario
            {
                StockActual = autoparteDto.StockInicial,
                StockMinimo = autoparteDto.StockMinimo,
                UltimaActualizacion = DateTime.Now
            };

            var historialPrecio = new HistorialPrecio
            {
                Precio = autoparteDto.PrecioInicial,
                FechaDesde = DateTime.Now,
                FechaHasta = null
            };

            MovimientoStock? movimientoStock = null;

            if (autoparteDto.StockInicial > 0)
            {
                movimientoStock = new MovimientoStock
                {
                    TipoMovimiento = "E",
                    Cantidad = autoparteDto.StockInicial,
                    FechaMovimiento = DateTime.Now,
                    Observaciones = "Stock inicial por alta de autoparte"
                };
            }

            await _autoparteRepository.AddWithInitialDataAsync(
                autoparte,
                inventario,
                historialPrecio,
                movimientoStock
            );
        }

        public async Task UpdateAsync(int id, UpdateAutoparteDto autoparteDto)
        {
            if (autoparteDto == null)
                throw new ArgumentNullException(nameof(autoparteDto));

            var autoparte = await _autoparteRepository.GetByIdAsync(id);

            if (autoparte == null)
                throw new KeyNotFoundException("La autoparte no existe");

            if (autoparteDto.CategoriaAutoparteId <= 0)
                throw new ArgumentException("La categoría seleccionada no es válida");

            if (string.IsNullOrWhiteSpace(autoparteDto.Codigo))
                throw new ArgumentException("El código es obligatorio");

            if (string.IsNullOrWhiteSpace(autoparteDto.Nombre))
                throw new ArgumentException("El nombre es obligatorio");

            var code = autoparteDto.Codigo.Trim();

            if (await _autoparteRepository.CodeExistsAsync(code, id))
                throw new InvalidOperationException("Ya existe otra autoparte con ese código");

            if (!await _autoparteRepository.CategoryExistsAsync(
                autoparteDto.CategoriaAutoparteId))
                throw new ArgumentException("La categoría seleccionada no existe");

            autoparte.CategoriaAutoparteId = autoparteDto.CategoriaAutoparteId;
            autoparte.Codigo = code;
            autoparte.Nombre = autoparteDto.Nombre.Trim();
            autoparte.Descripcion = autoparteDto.Descripcion;
            autoparte.Marca = autoparteDto.Marca;
            autoparte.AplicacionModelo = autoparteDto.AplicacionModelo;

            await _autoparteRepository.UpdateAsync(autoparte);
        }
        public async Task<bool> ChangeStatusAsync(int id, bool status)
        {
            var autoparte = await _autoparteRepository.GetByIdAsync(id);

            if (autoparte == null)
                return false;

            return await _autoparteRepository.ChangeStatusAsync(id, status);
        }
        public async Task<bool> AddStockAsync(int autoparteId, StockDto stockDto)
        {
            ValidateStockDto(stockDto);

            var autoparte = await _autoparteRepository.GetByIdAsync(autoparteId);

            if (autoparte == null)
                throw new KeyNotFoundException("La autoparte no existe");

            if (!autoparte.Estado)
                throw new InvalidOperationException("No se puede ingresar stock a una autoparte inactiva");

            return await _autoparteRepository.AddStockAsync(
                autoparteId,
                stockDto.Cantidad,
                stockDto.Observaciones.Trim()
            );
        }
        public async Task<bool> RemoveStockAsync(int autoparteId, StockDto stockDto)
        {
            ValidateStockDto(stockDto);

            var autoparte = await _autoparteRepository.GetByIdAsync(autoparteId);

            if (autoparte == null)
                throw new KeyNotFoundException("La autoparte no existe");

            if (!autoparte.Estado)
                throw new InvalidOperationException("No se puede retirar stock de una autoparte inactiva");

            return await _autoparteRepository.RemoveStockAsync(
                autoparteId,
                stockDto.Cantidad,
                stockDto.Observaciones.Trim()
            );
        }

        public async Task<bool> UpdatePriceAsync(int autoparteId, PrecioDto precioDto)
        {
            if (precioDto == null)
                throw new ArgumentNullException(nameof(precioDto));

            if (precioDto.Precio <= 0)
                throw new ArgumentException("El precio debe ser mayor que cero");

            var autoparte = await _autoparteRepository.GetByIdAsync(autoparteId);

            if (autoparte == null)
                throw new KeyNotFoundException("La autoparte no existe");

            if (!autoparte.Estado)
                throw new InvalidOperationException("No se puede actualizar el precio de una autoparte inactiva");

            return await _historialPrecioRepository.AddNewPriceAsync(
                autoparteId,
                precioDto.Precio
            );
        }

        private static void ValidateStockDto(StockDto stockDto)
        {
            if (stockDto == null)
                throw new ArgumentNullException(nameof(stockDto));

            if (stockDto.Cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero");

            if (string.IsNullOrWhiteSpace(stockDto.Observaciones))
                throw new ArgumentException("Las observaciones son obligatorias");

            if (stockDto.Observaciones.Trim().Length > 300)
                throw new ArgumentException("Las observaciones no pueden superar los 300 caracteres");
        }

        public async Task<List<HistorialPrecioDto>> GetPriceHistoryAsync(int autoparteId)
        {
            var autoparte = await _autoparteRepository.GetByIdAsync(autoparteId);

            if (autoparte == null)
                throw new KeyNotFoundException("La autoparte no existe.");

            var history = await _historialPrecioRepository
                .GetByAutoparteIdAsync(autoparteId);

            return history.Select(h => new HistorialPrecioDto
            {
                Precio = h.Precio,
                FechaDesde = h.FechaDesde,
                FechaHasta = h.FechaHasta
            }).ToList();
        }
    }
}

