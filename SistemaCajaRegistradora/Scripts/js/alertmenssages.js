$(document).ready(function () {
    console.log(sessionStorage);
    if (sessionStorage.length > 0) {
        let codigo = sessionStorage.codigo_barra;
        let nombre = sessionStorage.nombre;
        let mensaje = sessionStorage.mensaje;
        console.log(codigo, nombre, mensaje);
        if (mensaje != undefined) {
            if (codigo == undefined && nombre == undefined) {
                Swal.fire({
                    title: 'Bien!',
                    text: mensaje,
                    icon: 'success',
                });
            }
            if (codigo != undefined) {
                Swal.fire({
                    title: 'Bien!',
                    text: mensaje + codigo + ' ' + nombre,
                    icon: 'success',
                });
            } else {
                Swal.fire({
                    title: 'Bien!',
                    text: mensaje + ' ' + nombre,
                    icon: 'success',
                });
            }
        }
        sessionStorage.clear();
    }
});

function loadingModal(mensaje, loading) {
    if (loading) {
        $("#loadingModal").show();
        $("#text_mensaje").text(mensaje);
    } else {
        $("#loadingModal").hide();
    }
    
}