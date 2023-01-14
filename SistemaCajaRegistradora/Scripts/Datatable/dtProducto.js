$(document).ready(function () {
    let tabla1 = !!document.getElementById('tablaProducto');
    let tabla2 = !!document.getElementById('findTable');

    if (tabla1) {
        generarTabla1();
    }
    if (tabla2) {
        generarTabla2();
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
            case `<span class="badge bg-secondary">Baja</span>`:
                return 1;
            case `<span class="badge bg-warning">Medio</span>`:
                return 2;
            case `<span class="badge bg-danger">Alta</span>`:
                return 3;
            case `<span class="badge bg-primary">Prioridad no encontrada!!</span>`:
                return 4;
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
        dom: 'Bfrtp',
        order:[[3,"asc"]],
        lengthMenu: [5],
        ajax: {
            'url': '/Producto/getProductos',
            'type': 'GET',
            'datatype': 'json'
        },
        buttons: [
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i> Exportar tabla a excel',
                className: 'btn btn-success',
                exportOptions: { orthogonal: 'export', columns: ':visible' }
            }
        ],
        columnDefs: [
            {
                target: 10,
                render: DataTable.render.date()
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.12.0/i18n/es-ES.json',
        },
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
                    return `<a href="javascript:void(0);" onclick="descargarBarcode('${data}')" data-bs-toggle="tooltip" title="Descargar codigo de barra">
                                <canvas id="${data}_barcode" class="barcode mx-auto d-block"
                                        jsbarcode-format="ean13" jsbarcode-value="${data}"
                                        style="width: 100%; min-width: 100px; min-height: 50px; height: 80px;">
                                </canvas>
                            </a>
                            <span class="invisible">${data}</span>`;
                },
                orderable: false
            },
            {
                data: 'rutaimg',
                render: function (data) {
                    return `<a href="javascript:void(0);" onclick="accion(3,${idProd})" data-bs-toggle="tooltip" title="Subir/Cambiar imagen">
                               <img class="rounded mx-auto d-block shadow" src="${data}" alt="Imagen del producto"
                               style="width: 100px; min-width: 100px; min-height: 50px; height: 80px;" />
                            </a>
                            <span class="invisible" style="max-width: 80px;">${data}</span>`;
                },
                searchable: false,
                orderable: false,
            },
            { data: 'nombre' },
            { data: 'categoria' },
            {
                data: 'precio',
                render: function (data, type) {
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return type === 'export' ?
                        data.toString().replace(/[$.]/g, '') :
                        number;
                }
            },
            {
                data: 'stock',
                render: function (data, type) {
                    sprod = data;
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0)
                        .display(data);
                    return type === 'export' ?
                        data.toString().replace(/[$.]/g, '') :
                        number;
                }
            },
            {
                data: 'stockmin',
                render: function (data) {
                    smin = data;
                    list_smin.push(data);
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
                    switch (data) {
                        case 'Baja':
                            return `<span class="badge bg-secondary">${data}</span>`;
                            break;
                        case 'Medio':
                            return `<span class="badge bg-warning">${data}</span>`;
                            break;
                        case 'Alta':
                            return `<span class="badge bg-danger">${data}</span>`;
                            break;
                        default:
                            ret `<span class="badge bg-primary">Prioridad no encontrada!!</span>`;
                    }
                },
                type: 'prioridad'
            },
            {data: 'fechacreacion'},
            {
                data: 'id',
                'render': function (id) {
                    return `<div class="d-grid gap-2 d-md-flex justify-content-md-center align-items-center h-100">
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
        ]
    });
    table.on('draw', function () {
        $("span.invisible").hide();
        JsBarcode(".barcode").init();
        initTooltip();
    });
    // Event listener to the two range filtering inputs to redraw on input
    $('#text_preciomin, #text_preciomax').keyup(function () {
        table.order(3,'asc').draw();
    });
    $("#text_viewstockmin").change(function () {
        table.order(3,'asc').draw();
    });
    //Evento escucha cuando la se activa el metodo de exportacion
    table.on('buttons-processing', function (e, indicator) {
        if (indicator) {
            table.columns([11, 12]).visible(false);
            table.columns([0, 7, 8]).visible(true);
        }
        else {
            table.columns([11, 12]).visible(true);
            table.columns([0, 7, 8]).visible(false);
        }
    });
}

function generarTabla2() {
    let codigo;
    $("#findTable").DataTable({
        responsive: 'true',
        paging: false,
        dom: 'frt',
        order: [[1,'asc']],
        ajax: {
            'url': '/Producto/getProductos',
            'type': 'GET',
            'datatype': 'json'
        },
        columns: [
            {
                data: 'codigobarra',
                render: function (data) {
                    codigo = data;
                    return data;
                },
                visible: false
            },
            {
                data: 'rutaimg',
                render: function (data) {
                    return `<img class="rounded mx-auto d-block" src="${data}" alt="Imagen del producto"
                                    style="width: 60px; min-width: 50px; min-height: 50px; height: 50px;" />
                                `;
                },
                searchable: false,
                orderable: false,
            },
            {
                data: 'nombre',
                render: function (data) {
                    console.log(data);
                    return data;
                },
                searchable: true,
                orderable: true,
            },
            {
                data: 'categoria',
                searchable: true,
                orderable: true,
            },
            {
                data: 'precio',
                render: function (data) {
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return number;
                },
                searchable: false,
            },
            {
                data: 'stock',
                render: function (data) {
                    sprod = data;
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0)
                        .display(data);
                    return number;
                },
                searchable: false,
            },
            {
                data: 'id',
                render: function (data) {
                    return `<button class="btn btn-success" type="button" onclick="ingresarCodigo('${codigo}')">
                                    <i class="fas fa-check"></i>
                            </button>`;
                },
                searchable: false,
                orderable: false,
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.12.0/i18n/es-ES.json',
        }
    });
}
function initTooltip() {
    var tooltipList1 = [].slice.call(document.querySelectorAll('[data-bs-toggle = "tooltip"]'))
    tooltipList1.map(function (tooltipTriggerfun) {
        return new bootstrap.Tooltip(tooltipTriggerfun)
    });
}
function descargarBarcode(barcode) {
    let canvas = document.getElementById(barcode+"_barcode");
    let enlace = document.createElement('a');
    //Indicamos el titulo
    enlace.download = barcode + "_barcodeimg.jpg";
    // Convertir la imagen a Base64 y ponerlo en el enlace
    enlace.href = canvas.toDataURL("image/jpg", 1);
    //Descargamos
    enlace.click();
    enlace.remove();
}
function resetTable1() {
    $("#tablaProducto").DataTable().destroy();
    generarTabla1();
}