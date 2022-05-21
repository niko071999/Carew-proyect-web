$(document).ready(function () {
    console.log(sessionStorage);
    if (sessionStorage.length > 0) {
        let codigo = sessionStorage.codigo_barra;
        let nombre = sessionStorage.nombre;
        let mensaje = sessionStorage.mensaje;
        if (sessionStorage.codigo_barra != undefined) {
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
       
        sessionStorage.clear();
    }
});