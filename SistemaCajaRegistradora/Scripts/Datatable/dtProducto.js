$(document).ready(function () {
    let tabla1 = !!document.getElementById('tablaProducto');
    let tabla2 = !!document.getElementById('findTable');

    if (tabla2) {
        generarTabla2();
    }
    if (tabla1) {
        generarTabla1();
    }
});

function generarTabla1() {
    let idProd = 0;
    let sprod = 0;
    let smin = 0;
    let smax = 0;
    let list_smin = [];
    let prioridad = '';
    let count = 0;
    let slicelist = false;
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
        let check = $("#text_viewstockmin").prop('checked') ? true : false;
        let min = parseInt($('#text_preciomin').val().replace(/[$.]/g, ''), 10);
        let max = parseInt($('#text_preciomax').val().replace(/[$.]/g, ''), 10);
        let precio = data[5].replace(/[$.]/g, '');
        if (!slicelist) {
            list_smin = list_smin.slice(0, Math.floor(list_smin.length / 2));
            slicelist = true;
        }
        let smn = list_smin[count]; //stock min
        let lengh = list_smin.length;
        count++;

        if (count == lengh) {
            count = 0;
        }

        if (check) {
            if (smn > data[6]) {
                if ((isNaN(min) && isNaN(max)) ||
                    (isNaN(min) && precio <= max) ||
                    (min <= precio && isNaN(max)) ||
                    (min <= precio && precio <= max)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }
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
        responsive: true,
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
                target: 10,
                render: DataTable.render.date()
            }
        ],
        columns: [
            {
                data: 'id',
                render: function (data) {
                    idProd = data;
                    return data;
                },
                searchable: false,
                orderable: false,
                visible: false
            },
            {
                data: 'codigobarra',
                render: function (data) {
                    return `<img class="barcode mx-auto d-block"
                                jsbarcode-format="ean13" jsbarcode-value="${data}"
                                style="width: 100%; min-width: 100px; min-height: 50px; height: 80px;" />`;
                },
                orderable: false
            },
            {
                data: 'rutaimg',
                render: function (data) {
                    return `<a href="javascript:void(0);" onclick="accion(3,${idProd})" data-bs-toggle="tooltip" title="Subir/Cambiar imagen">
                               <img class="rounded mx-auto d-block shadow" src="${data}" alt="Imagen del producto"
                               style="width: 100px; min-width: 100px; min-height: 50px; height: 80px;" />
                            </a>`;
                },
                searchable: false,
                orderable: false,
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
                    list_smin.push(smin);
                    return data;
                },
                visible: false,
                searchable: false
            },
            {
                data: 'stockmax',
                render: function (data) {
                    smax = data;
                    return data;
                },
                visible: false,
                searchable: false
            },
            {
                data: 'prioridad',
                render: function (data) {
                    prioridad = data;
                    return data;
                },
                type: 'prioridad'
            },
            {data: 'fechacreacion'},
            {
                data: 'id',
                'render': function (id) {
                    return `<div class="d-grid gap-2 d-md-flex justify-content-md-center">
                                <button class="btn btn-warning" type="button" onclick="accion(1,${id})" data-bs-toggle="tooltip" title="Editar producto">
                                   <i class="fas fa-pen float-left"></i>
                                </button>
                                <button class="btn btn-danger" type="button" onclick="accion(2, ${id})" data-bs-toggle="tooltip" title="Borrar producto">
                                   <i class="fas fa-trash float-left"></i>
                                </button>
                             </div>`
                },
                searchable: false,
                orderable: false
            },
            {
                data: 'id', 
                'render': function (id) {
                    if (smin != 0 && smax != 0) {
                        if (sprod > 0 && sprod > smin && sprod <= smax) {
                            let mensaje = "Producto dentro del rango stock minimo y maximo";
                            return `<div class="d-md-flex justify-content-md-center">
                                <button type="button" class="btn btn-secondary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                    <i class="fas fa-info-circle"></i>
                                </button> 
                            </div>`
                        }
                        else if (sprod > 0 && sprod > smax) {
                            let result = sprod - smax;
                            if (prioridad.includes("Baja")) {
                                let mensaje = "Producto sobrepasado en " + result + " existencias maximas";
                                return `<div class="d-md-flex justify-content-md-center">
                                    <button type="button" class="btn btn-secondary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle"></i>
                                    </button>
                                </div>`
                            }
                            else if (prioridad.includes("Medio") || prioridad.includes("Alta")) {
                                let mensaje = "Buena existencia de producto";
                                return `<div class="d-md-flex justify-content-md-center">
                                    <button type="button" class="btn btn-secondary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle"></i>
                                    </button>
                                </div>`
                            }
                        }
                        else if (sprod > 0 && sprod <= smin) {
                            let result = smin - sprod;
                            if (result == 0) {
                                if (prioridad.includes("Baja")) {
                                    let mensaje = "Producto en su existencia minima";
                                    return `<div class="d-md-flex justify-content-md-center">
                                        <button type="button" class="btn bg-gradient-primary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                            <i class="fas fa-info-circle text-white"></i>
                                        </button>
                                    </div>`
                                }
                                else if (prioridad.includes("Medio")) {
                                    let mensaje = "Producto en su existencia minima";
                                    return `<div class="d-md-flex justify-content-md-center">
                                        <button type="button" class="btn bg-gradient-warning btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                            <i class="fas fa-info-circle text-white"></i>
                                         </button>
                                    </div>`
                                }
                                else {
                                    let mensaje = "Producto en su existencia minima";
                                    return `<div class="d-md-flex justify-content-md-center">
                                        <button type="button" class="btn bg-gradient-danger btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                            <i class="fas fa-info-circle text-white"></i>
                                        </button>
                                    </div>`
                                }
                            }
                            else {
                                if (prioridad.includes("Baja")) {
                                    let mensaje = "Producto por debajo de " + result + " existencia del stock minimo";
                                    return `<div class="d-md-flex justify-content-md-center">
                                        <button type="button" class="btn bg-gradient-primary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                            <i class="fas fa-info-circle text-white"></i>
                                         </button>
                                    </div>`
                                }
                                else if (prioridad.includes("Medio")) {
                                    let mensaje = "Producto por debajo de " + result + " existencia del stock minimo";
                                    return `<div class="d-md-flex justify-content-md-center">
                                        <button type="button" class="btn bg-gradient-warning btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                            <i class="fas fa-info-circle text-white"></i>
                                        </button>
                                    </div>`
                                }
                                else {
                                    let mensaje = "Producto por debajo de " + result + " existencia del stock minimo";
                                    return `<div class="d-md-flex justify-content-md-center">
                                        <button type="button" class="btn bg-gradient-danger btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                            <i class="fas fa-info-circle text-white"></i>
                                        </button>
                                    </div>`
                                }
                            }
                        }
                        else if (sprod == 0) {
                            if (prioridad.includes("Baja")) {
                                let mensaje = "Producto sin existencia minima";
                                return `<div class="d-md-flex justify-content-md-center">
                                    <button type="button" class="btn bg-gradient-primary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle text-white"></i>
                                    </button>
                                </div>`

                            }
                            else if (prioridad.includes("Medio")) {
                                let mensaje = "Producto sin existencia minima";
                                `<div class="d-md-flex justify-content-md-center">
                                    <button type="button" class="btn bg-gradient-warning btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle text-white"></i>
                                    </button>
                                 </div>`
                            }
                            else {
                                let mensaje = "Producto sin existencia minima";
                                return `<div class="d-md-flex justify-content-md-center">
                                    <button type="button" class="btn bg-gradient-danger btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                        <i class="fas fa-info-circle text-white"></i>
                                    </button>
                                </div>`
                            }
                        }
                    }
                    else {
                        let mensaje = "Stock minimo y maximo en 0";
                        return `<div class="d-md-flex justify-content-md-center">
                            <button type="button" class="btn bg-gradient-primary btn-circle" data-bs-toggle="tooltip" data-bs-placement="bottom" title="${mensaje}">
                                <i class="fas fa-info-circle text-white"></i>
                            </button>
                        </div>`
                    }
                    return ``
                },
                searchable: false,
                orderable: false,
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
    $("#text_viewstockmin").change(function () {
        table.draw();
    });
    table.on("init", function () {
        var data = table.rows().data().toArray();
        //barcode_codigobarra img-barcode
        for (var i = 0; i < data.length; i++) {
            let check = !!document.getElementById('barcode_'+data[i].codigobarra)
            console.log(check)
            try {
                JsBarcode(".barcode").init();
            } catch (e) {
                console.error('El codigo de barra no es valido: ' + e);
            }
            console.log(data[i]);
        }
        initTooltip();
    });
}

function generarTabla2() {
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
function initTooltip() {
    var tooltipList1 = [].slice.call(document.querySelectorAll('[data-bs-toggle = "tooltip"]'))
    var tooltipList2 = tooltipList1.map(function (tooltipTriggerfun) {
        return new bootstrap.Tooltip(tooltipTriggerfun)
    });
}