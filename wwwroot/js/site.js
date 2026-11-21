// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    let precioActual = 0;
    let stockDisponibleActual = null;
    let productoIdOriginal = $("#ProductoId").data("producto-inicial") || null;
    let cantidadOriginal = parseInt($("#Cantidad").data("cantidad-original")) || 0;

    function calcularTotalYValidarStock() {
        const productoId = $("#ProductoId").val();
        const cantidad = parseInt($("#Cantidad").val()) || 0;

        if (!productoId) {
            $("#Total").val("");
            $("#infoStock").text("");
            return;
        }

        let stockMaximo = stockDisponibleActual;
        if (stockMaximo !== null && productoIdOriginal && parseInt(productoId) === parseInt(productoIdOriginal)) {
            stockMaximo += cantidadOriginal;
        }

        if (stockMaximo !== null) {
            $("#infoStock").text(`Stock disponible: ${stockMaximo} | Precio unitario: ${precioActual}`);
            if (cantidad > stockMaximo) {
                Swal.fire(
                    "Stock insuficiente",
                    `La cantidad (${cantidad}) supera el stock disponible (${stockMaximo}).`,
                    "warning"
                );
                $("#Cantidad").val(stockMaximo > 0 ? stockMaximo : 1);
            }
        }

        const cantidadFinal = parseInt($("#Cantidad").val()) || 0;
        if (cantidadFinal >= 1 && precioActual > 0) {
            const total = (cantidadFinal * precioActual).toFixed(2);
            $("#Total").val(total);
        } else {
            $("#Total").val("");
        }
    }

    function cargarInfoProducto(productoId, alTerminar) {
        if (!productoId) {
            precioActual = 0;
            stockDisponibleActual = null;
            $("#Total").val("");
            $("#infoStock").text("");
            if (typeof alTerminar === "function") alTerminar();
            return;
        }

        $.get('/Pedidos/GetProductoInfo', { productoId: productoId }, function (producto) {
            precioActual = parseFloat(producto.precio) || 0;
            stockDisponibleActual = parseInt(producto.stock) || 0;
            calcularTotalYValidarStock();
            if (typeof alTerminar === "function") alTerminar();
        });
    }

    $("#categoriaFiltro").change(function () {
        const categoriaId = $(this).val();
        const productoPreseleccionado = $("#ProductoId").attr("data-producto-inicial");

        $("#ProductoId")
            .empty()
            .append('<option value="">-- Seleccione un producto --</option>');

        precioActual = 0;
        stockDisponibleActual = null;
        $("#Total").val("");
        $("#infoStock").text("");

        if (!categoriaId) {
            return;
        }

        $.get('/Pedidos/GetProductosPorCategoria', { categoriaId: categoriaId }, function (listaProductos) {
            $.each(listaProductos, function (i, prod) {
                $("#ProductoId").append(
                    $('<option>', {
                        value: prod.id,
                        text: prod.nombre
                    })
                );
            });

            if (productoPreseleccionado) {
                $("#ProductoId").val(productoPreseleccionado);
                $("#ProductoId").removeAttr("data-producto-inicial");
                $("#ProductoId").trigger("change");
            }
        });
    });

    $("#ProductoId").change(function () {
        const productoId = $(this).val();
        cargarInfoProducto(productoId);
    });

    $("#Cantidad").on("change input", function () {
        calcularTotalYValidarStock();
    });

    if ($("#ProductoId").length && $("#ProductoId").val()) {
        cargarInfoProducto($("#ProductoId").val());
    }
});

function obtenerInstanciaEntidad() {
    if (window.entidadActual === "producto" && typeof Clase_Producto !== "undefined") {
        return new Clase_Producto();
    }
    if (window.entidadActual === "pedido" && typeof Clase_Pedido !== "undefined") {
        return new Clase_Pedido();
    }
    if (typeof Clase_Pedido !== "undefined") {
        return new Clase_Pedido();
    }
    if (typeof Clase_Producto !== "undefined") {
        return new Clase_Producto();
    }
    return null;
}

var eliminar = (id) => {
    const instancia = obtenerInstanciaEntidad();
    if (!instancia) return;

    Swal.fire({
        title: "¿Está seguro de eliminar el registro?",
        text: "Esta acción eliminará el registro seleccionado.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Eliminar",
        cancelButtonText: "Cancelar"
    }).then((result) => {
        if (result.isConfirmed) {
            instancia.eliminar(id);
        }
    });
};

var todos = () => {
    const instancia = obtenerInstanciaEntidad();
    if (!instancia) return;
    instancia.todos();
};
