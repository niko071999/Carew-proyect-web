$(document).ready(function () {
    $("#tablaVenta").DataTable({
        responsive: 'true',
        ajax: {
            'url': '/Venta/getVentas',
            'type': 'GET',
            'datatype': 'json'
        },
        columns: [
            { data: 'cajero' },
            { data: 'metodoPago' },
            {
                data: 'totalVenta',
                render: function (data, type) {
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return number;
                }
            },
            { data: 'fecha' },
            {
                data: 'id',
                'render': function (id) {
                    return `<div class="d-flex justify-content-around">
                                <button class="btn btn-info btn-icon-split" type="button" onclick="moreDetail(${id})">
                                    <span class="icon text-white-50"><i class="fas fa-info-circle"></i></span>
                                    <span class="text">Mas detalle</span>
                                </button>
                                <button class="btn btn-secondary btn-icon-split" type="button" onclick="generarBoleta(${id})">
                                    <span class="icon text-white-50"><i class="fas fa-print"></i></span>
                                    <span class="text">Generar Boleta</span>
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