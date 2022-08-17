$(document).ready(init);

let stockProd = document.getElementById('stockProd');
let borrarCod = document.getElementById('btnBorrar');
stockProd.addEventListener("change", verificarStock);
borrarCod.addEventListener("click", borrarCodigo);

function getMiVentas() {
    $.get('/Venta/getMiVentas/', function (data) {
        if (data != null) {
            console.log(data);
        }
    });
}

function init() {
    $('#codigoAdd').focus();
    $('#stockProd').val(1);
}

function borrarCodigo() {
    $("#codigoAdd").val('');
    $('#codigoAdd').focus();
}

function verificarStock() {
    let cantidadProd = stockProd.value;
    if (cantidadProd < 1) {
        stockProd.value = 1;
    }
}

