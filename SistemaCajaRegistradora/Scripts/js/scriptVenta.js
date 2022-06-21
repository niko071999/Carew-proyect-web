function moreDetail(id) {
    $.get('/Venta/getMoreDetail/' + id, function (data) {
        if (data != null) {
            abrirModal(data);
        }
    });
}
