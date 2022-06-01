$(document).ready(function () {
    console.log(sessionStorage);
    if (sessionStorage.length > 0) {
        let codigo = sessionStorage.codigo_barra;
        let nombre = sessionStorage.nombre;
        let mensaje = sessionStorage.mensaje;
        console.log(codigo, nombre, mensaje);
        if (codigo != undefined || nombre != undefined
            && mensaje != undefined) {
            if (mensaje != undefined) {
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