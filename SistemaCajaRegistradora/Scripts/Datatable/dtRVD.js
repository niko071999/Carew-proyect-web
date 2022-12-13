$(document).ready(function () {
    //INPUTS FORMATER
    let minDate = new DateTime($('#text_datemin'), {
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
    
    let maxDate = new DateTime($('#text_datemax'), {
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

    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            let min = null;
            let max = null;
            let minStr = $('#text_datemin').val();
            let maxStr = $('#text_datemax').val();
            let date = transformarFecha(data[0]);
            if (minDate.val() != null) {
                min = transformarFecha(minStr);
            }
            if (maxDate.val() != null) {
                max = transformarFecha(maxStr);
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

    let tabla = $("#tablaRVD").DataTable({
        order: [[0, "asc"]],
        columnDefs: [{ targets: 0, render: DataTable.render.date('DD/MM/yyyy') }],
        buttons: [
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i> Exportar tabla a Excel',
                className: 'btn btn-success',
                exportOptions: { orthogonal: 'export', columns: ':visible' }
            }
        ],
        responsive: true,
        dom: 'Brtp',
        lengthMenu: [8],
        ajax: {
            'url': '/Venta/getVentaDiaria',
            'type': 'GET',
            'datatype': 'json'
        },
        columns: [
            { data: 'fecha' },
            {
                data: 'totalventa',
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
                data: 'crecimiento',
                render: function (data, type) {
                    let number = $.fn.dataTable
                        .render
                        .number('.', ',', 0, '$')
                        .display(data);
                    if (type === 'export') {
                        return data.toString().replace(/[$.]/g, '');
                    } else {
                        if (data > 0) {
                            return `<span class="text-success">${number}</span>`;
                        }
                        if (data == 0) {
                            return `<span class="text-warning">${number}</span>`;
                        }
                        if (data < 0) {
                            return `<span class="text-danger">${number}</span>`;
                        }
                        return `<span>${number}</span>`;
                    }
                },
                ordering: false
            },
            {
                data: 'porcentajeCrecimiento',
                render: function (data) {
                    let number = $.fn.dataTable
                        .render
                        .number('.', ',', 0)
                        .display(data);
                    if (data > 0) {
                        return `<span class="text-success">${number}%</span>`;
                    }
                    if (data == 0) {
                        return `<span class="text-warning">${number}%</span>`;
                    }
                    if (data < 0) {
                        return `<span class="text-danger">${number}%</span>`;
                    }
                    return `<span>${number}%</span>`;
                },
                ordering: false
            }
        ]
    });

    // Refilter the table
    $('#text_datemin, #text_datemax').on('change', function () {
        tabla.draw();
    });
});


function transformarFecha(date) {
    let fecha = date.split('/');
    let day = fecha[0];
    let month = fecha[1];
    let year = fecha[2];
    return new Date(`${month}/${day}/${year}`)
}
