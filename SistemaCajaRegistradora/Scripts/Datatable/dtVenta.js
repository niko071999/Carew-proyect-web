$(document).ready(function () {
    let tabla = $("#tablaVenta").DataTable({
        responsive: 'true',
        dom: 'Bfrtp',
        order: [[4, "desc"]],
        buttons: [
            {
                text: '<i class="fas fa-cash-register"></i>  Ir a POS',
                className: 'btn color-carew text-white pe-3 ps-3',
                action: function () {
                    location = '/Venta/POS'
                }
            },'spacer',
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i> Exportar tabla a excel',
                className: 'btn btn-success',
                exportOptions: { orthogonal: 'export', columns: ':visible' }
            }
        ],
        ajax: {
            'url': '/Venta/getVentas',
            'type': 'GET',
            'datatype': 'json'
        },
        lengthMenu: [8],
        columnDefs: [
            {
                target: 4,
                render: DataTable.render.date('DD/MM/YYYY')
            }
        ],
        columns: [
            { data: 'cajero' },
            { data: 'metodoPago' },
            { data: 'numboleta' },
            {
                data: 'totalVenta',
                render: function (data) {
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
                    return `<div class="row">
                                <div class="col-12">
                                    <button class="btn btn-secondary" type="button" onclick="generarBoleta(${id})" data-bs-toggle="tooltip" title="Generar Boleta">
                                        <i class="fas fa-print"></i>
                                    </button>
                                    <button class="btn btn-info" type="button" onclick="moreDetail(${id})" data-bs-toggle="tooltip" title="Ver mas detalles">
                                        <i class="fas fa-info-circle"></i>
                                    </button>
                                </div>
                            </div>`
                }
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.12.0/i18n/es-ES.json',
        }
    });

    tabla.on('draw', function () {
        initTooltip();
    });

    function initTooltip() {
        var tooltipList1 = [].slice.call(document.querySelectorAll('[data-bs-toggle = "tooltip"]'))
        tooltipList1.map(function (tooltipTriggerfun) {
            return new bootstrap.Tooltip(tooltipTriggerfun)
        });
    }
});




