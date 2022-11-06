formatearmontos();
calcularDiferencia();
$("#inputMontoRealEfectivo").change(function () {
    desformatearmontos();
    formatearmontos();
    calcularDiferencia();
});
$("#inputMontoRealTransferencia").change(function () {
    desformatearmontos();
    formatearmontos();
    calcularDiferencia();
});

let form_closecaja = document.getElementById('form_closecaja');
form_closecaja.addEventListener('submit', function (e) {
    e.preventDefault();
    Swal.fire({
        title: '¿Cerrar la caja?',
        text: "¿Quieres cerrar la caja?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si, cerrar la caja',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            desformatearmontos();
            form_closecaja.submit();
            formatearmontos();
        } else {
            e.stopImmediatePropagation();
        }
    });
});

function formatearmontos() {
    $("#inputMonto").val(parseInt($("#inputMonto").val()).toLocaleString('es-CL'));
    $("#inputSalesDay").val(parseInt($("#inputSalesDay").val()).toLocaleString('es-CL'));
    $("#inputTotal").val(parseInt($("#inputTotal").val()).toLocaleString('es-CL'));
    $("#inputMontoRealEfectivo").val(parseInt($("#inputMontoRealEfectivo").val()).toLocaleString('es-CL'));
    $("#inputMontoRealTransferencia").val(parseInt($("#inputMontoRealTransferencia").val()).toLocaleString('es-CL'));
    $("#inputDiferencia").val(parseInt($("#inputDiferencia").val()).toLocaleString('es-CL'));
    console.log($("#inputMonto").val(), $("#inputSalesDay").val(), $("#inputTotal").val(), $("#inputMontoReal").val(), $("#inputDiferencia").val());
}
function desformatearmontos() {
    $("#inputMonto").val($("#inputMonto").val().replace(/[.]/g, ''));
    $("#inputSalesDay").val($("#inputSalesDay").val().replace(/[.]/g, ''));
    $("#inputMontoRealEfectivo").val($("#inputMontoRealEfectivo").val().replace(/[.]/g, ''));
    $("#inputMontoRealTransferencia").val($("#inputMontoRealTransferencia").val().replace(/[.]/g, ''));
    $("#inputDiferencia").val($("#inputDiferencia").val().replace(/[.]/g, ''));
    $("#inputTotal").val($("#inputTotal").val().replace(/[.]/g, ''));
}
function calcularDiferencia() {
    let strTotal = $("#inputTotal").val().replace(/[.]/g, '');
    let total = parseInt(strTotal);
    $("#inputDiferencia").val(total);
    let strTotalRealEfectivo = $("#inputMontoRealEfectivo").val().replace(/[.]/g, '');
    let strTotalRealTransferencia = $("#inputMontoRealTransferencia").val().replace(/[.]/g, '');
    let totalrealefectivo = parseInt(strTotalRealEfectivo);
    let totalrealtransferencia = parseInt(strTotalRealTransferencia);
    let diferencia = total - (totalrealefectivo + totalrealtransferencia);

    $("#inputDiferencia").val(diferencia);
    $("#inputDiferencia").val(parseInt($("#inputDiferencia").val()).toLocaleString('es-CL'));
}

