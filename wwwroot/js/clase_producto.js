class Clase_Producto {
    eliminar(id) {
        $.post('/Productos/Delete', { id: id }, async (respuesta) => {
            if (respuesta == 200 || respuesta == "200") {
                await Swal.fire("Productos", "Se eliminó con éxito", "success");
                this.todos();
            } else {
                Swal.fire("Productos", "Ocurrió un error al eliminar", "error");
            }
        }).fail(() => {
            Swal.fire("Productos", "Ocurrió un error al eliminar", "error");
        });
    }

    todos() {
        $.get('/Productos/Todos', (listaProductos) => {
            let html = "";
            $.each(listaProductos, (index, producto) => {
                const categoriaTexto = producto.categoria
                    ? (producto.categoria.nombre || producto.categoria.descripcion || "")
                    : "";

                html += `<tr>
                    <td>${producto.nombre}</td>
                    <td>${producto.precio}</td>
                    <td>${producto.stock}</td>
                    <td>${categoriaTexto}</td>
                    <td>
                        <a class="btn btn-outline-dark btn-sm" href="/Productos/Edit/${producto.id}">Editar</a> |
                        <a class="btn btn-outline-secondary btn-sm" href="/Productos/Details/${producto.id}">Detalles</a> |
                        <a href="javascript:void(0)" onclick="eliminar(${producto.id})" class="btn btn-outline-danger btn-sm">Eliminar</a>
                    </td>
                </tr>`;
            });

            $("#tablaproducto").html(html);
        });
    }
}
