$(document).ready(function () {
    $.get("/Usuario/getSesion", function (data) {
        if (data != null) {
            document.getElementById('text_nameuser').innerText = data.nombreuser;
            document.getElementById('img_user').src = data.imgruta;
        } else {
            console.log('error null');
        }
    });
});

$("#linksignout").click(function () {
    const swal = Swal.mixin({
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-danger'
        },
        buttonsStyling: false
    });
    swal.fire({
        title: 'Cerrar sesion',
        text: 'Seguro que quiere cerrar la sesion',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Si',
        cancelButtonText: 'No',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            window.location = '/Sesion/SignOut';
        }
        return;
    })
});
$("#linksChangePass").click(function () {
    $.get('/Usuario/formsCambiarPass/', function (data) {
        try {
            $('#coreModal').html(data);
            $('#coreModal').modal('show');
        } catch (e) {
            Swal.fire({
                icon: 'error',
                title: 'Error de autorizacion!',
                text: 'No puede ingresar a este modulo o funcion, ya que no tiene los permisos suficientes'
            });
        }
    });
});

//Cambiar contraseña

function verificarClave() {
    var i_passnew = $("#passNew").val();
    var i_passrepeat = $("#passRepeat").val();
    var i_pass = $("#pass").val();

    if (i_passnew.trim() != ' ' && i_passrepeat.trim() != ' ' && i_pass.trim() != ' ') {
        if (i_passnew.length >= 6 && i_pass.length >= 6) {
            if (i_passnew == i_passrepeat) {
                let usuario = {
                    "id": 0,
                    "nombre": 'n',
                    "apellido": 'a',
                    "nombreUsuario": 'new',
                    "clave": i_pass,
                    "rutaImg": 'r',
                    "rolid": 0
                }
                $.post('/Usuario/verificarClave/', usuario, function (data) {
                    if (data.success) {
                        cambiarClave('/Usuario/cambiarClave', i_passnew);
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error de contraseña',
                            text: 'La contraseña de acceso es incorrecta',
                            toast: true,
                            position: 'top-end'
                        });
                    }
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error de contraseña',
                    text: 'Las contraseñas no son las mismas',
                    toast: true,
                    position: 'top-end'
                });
            }
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error de contraseña',
                text: 'La nueva contraseña o la actual debe tener como minimo 6 caracteres',
                toast: true,
                position: 'top-end'
            });
        }
    } else {
        Swal.fire({
            icon: 'error',
            title: 'Error de contraseña',
            text: 'Campos vacios',
            toast: true,
            position: 'top-end'
        });
    }
};
    