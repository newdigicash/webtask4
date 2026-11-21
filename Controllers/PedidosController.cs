using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CrudPedido.Models;

namespace CrudPedido.Controllers
{
    public class PedidosController : Controller
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var pedidos = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Producto)
                    .ThenInclude(pr => pr.Categoria);
            return View(await pedidos.ToListAsync());
        }

        // GET: Pedidos/Todos
        [HttpGet]
        public async Task<IActionResult> Todos()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Producto)
                    .ThenInclude(pr => pr.Categoria)
                .Select(p => new
                {
                    id = p.Id,
                    fecha = p.Fecha.ToString("yyyy-MM-dd"),
                    cantidad = p.Cantidad,
                    total = p.Total,
                    clienteId = p.ClienteId,
                    cliente = new
                    {
                        id = p.Cliente.Id,
                        nombre = p.Cliente.Nombre,
                        apellido = p.Cliente.Apellido,
                        nombreCompleto = p.Cliente.Nombre + " " + p.Cliente.Apellido
                    },
                    productoId = p.ProductoId,
                    producto = new
                    {
                        id = p.Producto.Id,
                        nombre = p.Producto.Nombre,
                        precio = p.Producto.Precio,
                        categoriaId = p.Producto.CategoriaId,
                        categoriaNombre = p.Producto.Categoria != null ? p.Producto.Categoria.Nombre : ""
                    }
                })
                .ToListAsync();

            return Json(pedidos);
        }

        // GET: Pedidos/GetProductosPorCategoria?categoriaId=1
        [HttpGet]
        public async Task<IActionResult> GetProductosPorCategoria(int categoriaId)
        {
            var productos = await _context.Productos
                .Where(p => p.CategoriaId == categoriaId)
                .Select(p => new
                {
                    id = p.Id,
                    nombre = p.Nombre,
                    precio = p.Precio,
                    stock = p.Stock
                })
                .ToListAsync();

            return Json(productos);
        }

        // GET: Pedidos/GetProductoInfo?productoId=1
        [HttpGet]
        public async Task<IActionResult> GetProductoInfo(int productoId)
        {
            var producto = await _context.Productos
                .Where(p => p.Id == productoId)
                .Select(p => new
                {
                    id = p.Id,
                    nombre = p.Nombre,
                    precio = p.Precio,
                    stock = p.Stock,
                    categoriaId = p.CategoriaId
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound();
            }

            return Json(producto);
        }

        // GET: Pedidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Producto)
                    .ThenInclude(pr => pr.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // GET: Pedidos/Create
        public IActionResult Create()
        {
            CargarSelectLists();
            return View(new Pedido { Fecha = DateTime.Today, Cantidad = 1 });
        }

        // POST: Pedidos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,Cantidad,Total,ClienteId,ProductoId")] Pedido pedido)
        {
            ModelState.Remove(nameof(Pedido.Cliente));
            ModelState.Remove(nameof(Pedido.Producto));
            ModelState.Remove(nameof(Pedido.Total));

            var producto = await _context.Productos.FindAsync(pedido.ProductoId);
            if (producto == null)
            {
                ModelState.AddModelError(nameof(Pedido.ProductoId), "El producto seleccionado no existe.");
            }
            else if (pedido.Cantidad > producto.Stock)
            {
                ModelState.AddModelError(nameof(Pedido.Cantidad), $"La cantidad supera el stock disponible ({producto.Stock}).");
            }

            if (producto != null && pedido.Cantidad >= 1)
            {
                pedido.Total = producto.Precio * pedido.Cantidad;
            }

            if (ModelState.IsValid && producto != null)
            {
                producto.Stock -= pedido.Cantidad;
                _context.Add(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CargarSelectLists(pedido.ClienteId, producto?.CategoriaId, pedido.ProductoId);
            return View(pedido);
        }

        // GET: Pedidos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            CargarSelectLists(pedido.ClienteId, pedido.Producto?.CategoriaId, pedido.ProductoId);
            return View(pedido);
        }

        // POST: Pedidos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fecha,Cantidad,Total,ClienteId,ProductoId")] Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Pedido.Cliente));
            ModelState.Remove(nameof(Pedido.Producto));
            ModelState.Remove(nameof(Pedido.Total));

            var pedidoAnterior = await _context.Pedidos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedidoAnterior == null)
            {
                return NotFound();
            }

            var productoNuevo = await _context.Productos.FindAsync(pedido.ProductoId);
            if (productoNuevo == null)
            {
                ModelState.AddModelError(nameof(Pedido.ProductoId), "El producto seleccionado no existe.");
            }
            else
            {
                var stockDisponible = productoNuevo.Stock;
                if (pedidoAnterior.ProductoId == pedido.ProductoId)
                {
                    stockDisponible += pedidoAnterior.Cantidad;
                }

                if (pedido.Cantidad > stockDisponible)
                {
                    ModelState.AddModelError(nameof(Pedido.Cantidad), $"La cantidad supera el stock disponible ({stockDisponible}).");
                }

                if (pedido.Cantidad >= 1)
                {
                    pedido.Total = productoNuevo.Precio * pedido.Cantidad;
                }
            }

            if (ModelState.IsValid && productoNuevo != null)
            {
                try
                {
                    if (pedidoAnterior.ProductoId == pedido.ProductoId)
                    {
                        productoNuevo.Stock = (productoNuevo.Stock + pedidoAnterior.Cantidad) - pedido.Cantidad;
                    }
                    else
                    {
                        var productoAnterior = await _context.Productos.FindAsync(pedidoAnterior.ProductoId);
                        if (productoAnterior != null)
                        {
                            productoAnterior.Stock += pedidoAnterior.Cantidad;
                        }
                        productoNuevo.Stock -= pedido.Cantidad;
                    }

                    _context.Update(pedido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedido.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            CargarSelectLists(pedido.ClienteId, productoNuevo?.CategoriaId, pedido.ProductoId);
            return View(pedido);
        }

        // GET: Pedidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Producto)
                    .ThenInclude(pr => pr.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // POST: Pedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(pedido.ProductoId);
            if (producto != null)
            {
                producto.Stock += pedido.Cantidad;
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok(200);
            }

            return RedirectToAction(nameof(Index));
        }

        private void CargarSelectLists(int? clienteId = null, int? categoriaId = null, int? productoId = null)
        {
            var clientes = _context.Clientes
                .Select(c => new
                {
                    c.Id,
                    NombreCompleto = c.Nombre + " " + c.Apellido
                })
                .ToList();

            ViewData["ClienteId"] = new SelectList(clientes, "Id", "NombreCompleto", clienteId);
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", categoriaId);

            var productosQuery = _context.Productos.AsQueryable();
            if (categoriaId.HasValue)
            {
                productosQuery = productosQuery.Where(p => p.CategoriaId == categoriaId.Value);
            }

            ViewData["ProductoId"] = new SelectList(productosQuery.ToList(), "Id", "Nombre", productoId);
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.Id == id);
        }
    }
}
