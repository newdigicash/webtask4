class Clase_Pedido {
    eliminar(id) {
        $.post('/Pedidos/Delete', { id: id }, async (respuesta) => {
            if (respuesta == 200 || respuesta == "200") {
                await Swal.fire("Pedidos", "Se eliminó con éxito", "success");
                this.todos();
            } else {
                Swal.fire("Pedidos", "Ocurrió un error al eliminar", "error");
            }
        }).fail(() => {
            Swal.fire("Pedidos", "Ocurrió un error al eliminar", "error");
        });
    }

    todos() {
        $.get('/Pedidos/Todos', (listaPedidos) => {
            let html = "";
            $.each(listaPedidos, (index, pedido) => {
                const clienteTexto = pedido.cliente
                    ? (pedido.cliente.nombreCompleto || `${pedido.cliente.nombre} ${pedido.cliente.apellido}`)
                    : "";
                const productoTexto = pedido.producto ? pedido.producto.nombre : "";

                html += `<tr>
                    <td>${pedido.fecha}</td>
                    <td>${pedido.cantidad}</td>
                    <td>${pedido.total}</td>
                    <td>${clienteTexto}</td>
                    <td>${productoTexto}</td>
                    <td>
                        <a class="btn btn-outline-dark btn-sm" href="/Pedidos/Edit/${pedido.id}">Editar</a> |
                        <a class="btn btn-outline-secondary btn-sm" href="/Pedidos/Details/${pedido.id}">Detalles</a> |
                        <a href="javascript:void(0)" onclick="eliminar(${pedido.id})" class="btn btn-outline-danger btn-sm">Eliminar</a>
                    </td>
                </tr>`;
            });

            $("#tablapedido").html(html);
        });
    }
}
