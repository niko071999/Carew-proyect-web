$(document).ready(getMiVentas);
function getMiVentas() {
    $.get('/Venta/getMiVentas/', function (data) {
        if (data != null) {
            console.log(data);
        }
    });
}

