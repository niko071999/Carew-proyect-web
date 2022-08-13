$(document).ready(function () {
    $("#tablaCategoria").DataTable({
        responsive: 'true',
        ajax: {
            'url': '/Categoria/getCategorias',
            'type': 'GET',
            'datatype': 'json'
        },
        columns: [
            { data: 'nombre' },
            { data: 'descripcion' },
            {
                data: 'id',
                'render': function (id) {
                    return `<div class="d-grid gap-2 d-md-flex justify-content-md-center">
                                <button class="btn btn-warning btn-icon-split" type="button" onclick="accion(1,${id})">
                                     <span class="icon text-white-50"><i class="fas fa-pen float-left"></i></span>
                                     <span class="text">Editar</span>
                                </button>
                                <button class="btn btn-danger btn-icon-split" type="button" onclick="accion(2, ${id})">
                                     <span class="icon text-white-50"><i class="fas fa-trash float-left"></i></span>
                                     <span class="text">Eliminar</span>
                                </button>
                            </div>`
                                            
                }
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.12.0/i18n/es-ES.json',
        }
    });
});