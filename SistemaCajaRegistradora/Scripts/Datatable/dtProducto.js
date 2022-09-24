$(document).ready(function () {   
    let tabla1 = !!document.getElementById('tablaProducto');
    let tabla2 = !!document.getElementById('findTable');

    if (tabla2) {
        $("#findTable").DataTable({
            responsive: 'true',
            dom: '<"toolbar">frt',
            ajax: {
                'url': '/Producto/getProductos',
                'type': 'GET',
                'datatype': 'json'
            },
            columnDefs: [
                {//Imagen
                    target: 0,
                    searchable: false,
                },
                {//Nombre
                    target: 1,
                    searchable: true,
                    orderable: true,
                },
                {//Precio
                    target: 2,
                    visible: true,
                    searchable: false,
                },
                {//stock
                    target: 3,
                    visible: true,
                    searchable: false,
                },
                {//Accion add
                    target: 4,
                    searchable: false,
                    orderable: false,
                },
            ],
            columns: [
                {
                    data: 'rutaimg',
                    render: function (data) {
                        return `<img class="rounded mx-auto d-block" src="${data}" alt="Imagen del producto"
                                    style="width: 60px; min-width: 50px; min-height: 50px; height: 50px;" />
                                `;
                    }
                },
                { data: 'nombre' },
                {
                    data: 'precio',
                    render: function (data) {
                        let number = $.fn.dataTable.render
                            .number('.', ',', 0, '$')
                            .display(data);
                        return number;
                    }
                },
                {
                    data: 'stock',
                    render: function (data) {
                        sprod = data;
                        let number = $.fn.dataTable.render
                            .number('.', ',', 0)
                            .display(data);
                        return number;
                    }
                },
                {
                    data: 'codigobarra',
                    render: function (data) {
                        let codigo = data;
                        return `<button class="btn btn-success" type="button" onclick="ingresarCodigo(${codigo})">
                                    <i class="fas fa-check"></i>
                            </button>`;
                    }
                }
            ],
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.12.0/i18n/es-ES.json',
            }
        });
    }
    if (tabla1) {
        let idProd = 0;
        let sprod = 0;
        let smin = 0;
        let smax = 0;
        let prioridad = '';
        //Ordenar dependiendo del contexto del dato (Ordenando de acuerdo a la prioridad)
        $.fn.dataTable.ext.type.order['prioridad-pre'] = function (d) {
            switch (d) {
                case 'Baja':
                    return 1;
                case 'Medio':
                    return 2;
                case 'Alta':
                    return 3;
            }
        };
        //Customizacion del filtro para rango
        $.fn.dataTable.ext.search.push(function (settings, data, dataindex) {
            let min = parseInt($('#text_preciomin').val().replace(/[$.]/g, ''), 10);
            let max = parseInt($('#text_preciomax').val().replace(/[$.]/g, ''), 10);
            let precio = data[5].replace(/[$.]/g, '');

            console.log(min, max, precio);

            if ((isNaN(min) && isNaN(max)) ||
                (isNaN(min) && precio <= max) ||
                (min <= precio && isNaN(max)) ||
                (min <= precio && precio <= max)) {
                return true;
            } else {
                return false;
            }
        });
        let table = $("#tablaProducto").DataTable({
            responsive: 'true',
            lengthMenu: [
                [5, 10, -1],
                [5, 10, 'All'],
            ],
            ajax: {
                'url': '/Producto/getProductos',
                'type': 'GET',
                'datatype': 'json'
            },
            columnDefs: [
                {
                    target: 0,
                    visible: false,
                    searchable: false,
                },
                {
                    target: 2,
                    searchable: false,
                    orderable: false,
                },
                {
                    target: 7,
                    visible: false,
                    searchable: false,
                },
                {
                    target: 8,
                    visible: false,
                    searchable: false,
                },
                {
                    type: 'prioridad',
                    target: 9,
                },
                {
                    targets: 10,
                    render: DataTable.render.date(),
                },
                {
                    target: 11,
                    searchable: false,
                    orderable: false,
                },
                {
                    target: 12,
                    searchable: false,
                    orderable: false,
                },
            ],
            columns: [
                {
                    data: 'id',
                    render: function (data) {
                        idProd = data;
                        return data;
                    }
                },
                { data: 'codigobarra' },
                {
                    data: 'rutaimg',
                    render: function (data) {
                        return `<a href="javascript:void(0);" onclick="accion(3,${idProd})">
                               <img class="rounded mx-auto d-block" src="${data}" alt="Imagen del producto"
                               style="width: 100px; min-width: 100px; min-height: 50px; height: 80px;" />
                            </a>`;
                    }
                },
                { data: 'nombre' },
                { data: 'categoria' },
                {
                    data: 'precio',
                    render: function (data) {
                        let number = $.fn.dataTable.render
                            .number('.', ',', 0, '$')
                            .display(data);
                        return number;
                    }
                },
                {
                    data: 'stock',
                    render: function (data) {
                        sprod = data;
                        let number = $.fn.dataTable.render
                            .number('.', ',', 0)
                            .display(data);
                        return number;
                    }
                },
                {
                    data: 'stockmin',
                    render: function (data) {
                        smin = data;
                        return data;
                    }
                },
                {
                    data: 'stockmax',
                    render: function (data) {
                        smax = data;
                        return data;
                    }
                },
                {
                    data: 'prioridad',
                    render: function (data) {
                        prioridad = data;
                        return data;
                    }
                },
                { data: 'fechacreacion' },
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
                },
                {
                    data: 'id',
                    'render': function (id) {
                        if (smin != 0 && smax != 0) {
                            if (sprod > 0 && sprod > smin && sprod <= smax) {
                                let mensaje = "Producto dentro del rango stock minimo y maximo";
                                return `<button type="button" class="btn btn-secondary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                <i class="fas fa-info-circle"></i>
                            </button>`
                            }
                            else if (sprod > 0 && sprod > smax) {
                                let result = sprod - smax;
                                if (prioridad.includes("Baja")) {
                                    let mensaje = "Producto sobrepasado en " + result + " existencias maximas";
                                    return `<button type="button" class="btn btn-secondary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                    <i class="fas fa-info-circle"></i>
                                </button>`
                                }
                                else if (prioridad.includes("Medio") || prioridad.includes("Alta")) {
                                    let mensaje = "Buena existencia de producto";
                                    return `<button type="button" class="btn btn-secondary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                    <i class="fas fa-info-circle"></i>
                                </button>`
                                }
                            }
                            else if (sprod > 0 && sprod <= smin) {
                                let result = smin - sprod;
                                if (result == 0) {
                                    if (prioridad.includes("Baja")) {
                                        let mensaje = "Producto en su existencia minima";
                                        return `<button type="button" class="btn bg-gradient-primary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle"></i>
                                    </button>`
                                    }
                                    else if (prioridad.includes("Medio")) {
                                        let mensaje = "Producto en su existencia minima";
                                        return `<button type="button" class="btn bg-gradient-warning btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle"></i>
                                     </button>`
                                    }
                                    else {
                                        let mensaje = "Producto en su existencia minima";
                                        return `<button type="button" class="btn bg-gradient-danger btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle"></i>
                                     </button>`
                                    }
                                }
                                else {
                                    if (prioridad.includes("Baja")) {
                                        let mensaje = "Producto por debajo de " + result + " existencia del stock minimo";
                                        return `<button type="button" class="btn bg-gradient-primary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle"></i>
                                     </button>`
                                    }
                                    else if (prioridad.includes("Medio")) {
                                        let mensaje = "Producto por debajo de " + result + " existencia del stock minimo";
                                        return `<button type="button" class="btn bg-gradient-warning btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle"></i>
                                    </button>`
                                    }
                                    else {
                                        let mensaje = "Producto por debajo de " + result + " existencia del stock minimo";
                                        return `<button type="button" class="btn bg-gradient-danger btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle"></i>
                                    </button>`
                                    }
                                }
                            }
                            else if (sprod == 0) {
                                if (prioridad.includes("Baja")) {
                                    let mensaje = "Producto sin existencia minima";
                                    return `<button type="button" class="btn bg-gradient-primary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                    <i class="fas fa-info-circle"></i>
                                </button>`

                                }
                                else if (prioridad.includes("Medio")) {
                                    let mensaje = "Producto sin existencia minima";
                                    `<button type="button" class="btn bg-gradient-warning btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                    <i class="fas fa-info-circle"></i>
                                </button>`
                                }
                                else {
                                    let mensaje = "Producto sin existencia minima";
                                    return `<button type="button" class="btn bg-gradient-danger btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                    <i class="fas fa-info-circle"></i>
                                </button>`
                                }
                            }
                        }
                        else {
                            let mensaje = "Stock minimo y maximo en 0";
                            return `<button type="button" class="btn bg-gradient-primary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                            <i class="fas fa-info-circle"></i>
                        </button>`
                        }
                        return ``
                    }
                }
            ],
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.12.0/i18n/es-ES.json',
            }
        });
        // Event listener to the two range filtering inputs to redraw on input
        $('#text_preciomin, #text_preciomax').keyup(function () {
            table.draw();
        });
    }
});