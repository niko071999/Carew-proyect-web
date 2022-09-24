function moreDetail(id) {
    $.get('/Venta/getMoreDetail/' + id, function (data) {
        if (data != null) {
            abrirModal(data);
        }
    });
}
function generarBoleta(id) {
    $.get('/Venta/viewBoletaVenta/' + id, function (data) {
        abrirModal(data);
    });
}

