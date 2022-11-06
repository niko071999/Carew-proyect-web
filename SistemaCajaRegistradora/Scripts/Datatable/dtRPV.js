$(document).ready(function () {
    let tabla = $("#tablaRPV").DataTable({
        order: [[4, "desc"]],
        buttons: [
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i> Exportar tabla a Excel',
                className: 'btn btn-success',
                exportOptions: { orthogonal: 'export' }
            }
        ],
        responsive: true,
        dom: 'Brtp',
        lengthMenu: [5],
        ajax: {
            'url': '/Venta/getProductosVendidos',
            'type': 'GET',
            'datatype': 'json'
        },
        columns: [
            {
                data: 'codigobarra',
                render: function (data) {
                    return `<canvas id="${data}_barcode" class="barcode mx-auto d-block"
                                    jsbarcode-format="ean13" jsbarcode-value="${data}"
                                    style="width: 100%; min-width: 100px; min-height: 50px; height: 80px;">
                            </canvas>
                            <span class="invisible">${data}</span>`;
                },
                ordering: false
            },
            {
                data: 'ruta_imagen',
                render: function (data) {
                    return `<img class="rounded mx-auto d-block shadow" src="${data}" alt="Imagen del producto"
                                 style="width: 100px; min-width: 100px; min-height: 50px; height: 80px;" />
                            <span class="invisible" style="max-width: 80px;">${data}</span>`;
                }
            },
            { data: 'nombre_producto' },
            {
                data: 'precio',
                render: function (data, type) {
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return type === 'export' ?
                        data.toString().replace(/[$.]/g, '') :
                        number;
                },
                ordering: false
            },
            {
                data: 'cantidad_vendido',
                render: function (data, type) {
                    let number = $.fn.dataTable
                        .render
                        .number('.', ',', 0)
                        .display(data);
                    if (type === 'export') {
                        return data.toString().replace(/[$.]/g, '');
                    } else {
                        return number;
                    }
                }
            }
        ]
    });
    tabla.on('draw', function () {
        $("span.invisible").hide();
        JsBarcode(".barcode").init();
    });
});