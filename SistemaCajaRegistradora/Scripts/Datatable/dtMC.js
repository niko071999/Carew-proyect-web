let movimientoscaja = []
let minDate;
let maxDate;
let tabla;

$(document).ready(function () {
    tabla = generarTablaMC(movimientoscaja);
    switchDisableInputs(true);
});

$("#cajerosid").change(function () {
    let idcajero = $(this).val();

    if (idcajero != '') {
        switchDisableInputs(false);
        let id = parseInt(idcajero);
        $.get('/MovimientoCaja/getMovCaja/' + id, function (data) {
            $("#tablaMC").DataTable().destroy();
            generarTablaMC(data.movimientos);
        }, 'json');
    } else {
        switchDisableInputs(true);
        $("#tablaMC").DataTable().destroy();
        generarTablaMC(movimientoscaja);
    }
});

function generarTablaMC(mc) {
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            let min = null;
            let max = null;
            let date;
            let minStr = $('#input_desde').val();
            let maxStr = $('#input_hasta').val();
            let radioValue = $("input[name='radio_fecha']:checked").val();
            console.log(radioValue);
            if (radioValue == 1) { //fecha de apertura
                date = transformarFecha(data[3]);
                console.log(date);
                if (minDate.val() != null) {
                    min = transformarFecha(minStr);
                }
                if (maxDate.val() != null) {
                    max = transformarFecha(maxStr);
                }
            }
            if (radioValue == 2) {//fecha de cierre
                date = transformarFecha(data[4]);
                console.log(date);
                if (minDate.val() != null) {
                    min = transformarFecha(minStr);
                }
                if (maxDate.val() != null) {
                    max = transformarFecha(maxStr);
                }
            }
            if (
                (min === null && max === null) ||
                (min === null && date <= max) ||
                (min <= date && max === null) ||
                (min <= date && date <= max)
            ) {
                return true;
            }
            return false;
        }
    );
    tabla = $("#tablaMC").DataTable({
        responsive: true,
        dom: '<"toolbar">Bfrtp',
        order: [[3, "desc"]],
        lengthMenu: [6],
        data: mc,
        footerCallback: function () {
            var api = this.api();

            //Quitar el formato al dato de la celda
            var intVal = function (i) {
                console.log(i);
                return typeof i === 'string' ? i.replace(/[\$,]/g, '') * 1 :
                    typeof i === 'number' ? i : 0;
            };

            //Total de toda la pagina pagina
            let total = api.column(8)
                .data().reduce(function (a, b) {
                    return intVal(a) + intVal(b);
                }, 0);

            //Actualizar footer
            $(api.column(8).footer())
                .html('$' + total.toString()
                    .toLocaleString('es-CL'));
        },
        buttons: [
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i> Exportar tabla a excel',
                className: 'btn btn-success',
                exportOptions: { orthogonal: 'export' }
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.12.0/i18n/es-ES.json',
        },
        columnDefs: [
            {
                target: 3,
                render: DataTable.render.date('DD/MM/YYYY')
            },
            {
                target: 4,
                render: DataTable.render.date('DD/MM/YYYY')
            }
        ],
        columns: [
            {
                data: 'id',
                searchable: false,
                orderable: false,
                visible: false
            },
            {
                data: 'num_caja',
                orderable: false
            },
            {
                data: 'monto_apertura',
                render: function (data, type) {
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return type === 'export' ?
                        data.toString().replace(/[$.]/g, '') :
                        number
                }
            },
            { data: 'fecha_apertura' },
            { data: 'fecha_cierre' },
            {
                data: 'total_venta_diaria',
                render: function (data, type) {
                    if (data == null) {
                        return 'Caja no cerrada';
                    }
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return type === 'export' ?
                        data.toString().replace(/[$.]/g, '') :
                        number
                }
            },
            {
                data: 'total_real_efectivo',
                render: function (data, type) {
                    if (data == null) {
                        return 'Caja no cerrada';
                    }
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return type === 'export' ?
                        data.toString().replace(/[$.]/g, '') :
                        number
                },
                searchable: false
            },
            {
                data: 'total_real_transferencia',
                render: function (data, type) {
                    if (data == null) {
                        return 'Caja no cerrada';
                    }
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return type === 'export' ?
                        data.toString().replace(/[$.]/g, '') :
                        number
                },
                searchable: false
            },
            {
                data: 'diferencia',
                render: function (data, type) {
                    if (data == null) {
                        return 'Caja no cerrada';
                    }
                    let number = $.fn.dataTable.render
                        .number('.', ',', 0, '$')
                        .display(data);
                    return type === 'export' ?
                        data.toString().replace(/[$.]/g, '') :
                        number
                },
                searchable: false
            },
        ]
    });
    

    //INPUTS FORMATER
    minDate = new DateTime($('#input_desde'), {
        format: 'DD/MM/yyyy',
        buttons: {
            today: true,
        },
        firstDay: 1,
        yearRange: 50,
        i18n: {
            today: 'Hoy',
            next: '&gt;',
            previous: '&lt;',
            weekdays: ['Dom', 'Lun', 'Mar', 'Mie', 'Jue', 'Vie', 'Sab'],
            months: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic']
        }
    });
    maxDate = new DateTime($('#input_hasta'), {
        format: 'DD/MM/yyyy',
        buttons: {
            today: true,
        },
        firstDay: 1,
        yearRange: 50,
        i18n: {
            today: 'Hoy',
            next: '&gt;',
            previous: '&lt;',
            weekdays: ['Dom', 'Lun', 'Mar', 'Mie', 'Jue', 'Vie', 'Sab'],
            months: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic']
        }
    });
}

// Refilter the table
$('#input_desde, #input_hasta')
    .on('change', function () {
        tabla.draw();
    });

function switchDisableInputs(check) {
    $("#input_desde").prop("disabled", check);
    $("#input_hasta").prop("disabled", check);
}

function transformarFecha(date) {
    let fecha = date.split('/');
    let day = fecha[0];
    let month = fecha[1];
    let year = fecha[2];
    return new Date(`${month}/${day}/${year}`)
}


