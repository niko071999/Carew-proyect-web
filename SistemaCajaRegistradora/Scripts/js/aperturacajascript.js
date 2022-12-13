$(document).ready(formatearmonto);
$("#inputMonto").change(formatearmonto);
function formatearmonto() {
    $("#inputMonto").val(parseFloat($("#inputMonto").val()).toLocaleString('es-CL'));
}

let form_opencaja = document.getElementById('form_opencaja');
form_opencaja.addEventListener('submit', function (e) {
    e.preventDefault();
    Swal.fire({
        title: '¿Abrir Caja?',
        text: "¿Quieres abrir la caja?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si, abrir la caja',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            form_opencaja.submit();
        } else {
            e.stopImmediatePropagation();
        }
    });
})