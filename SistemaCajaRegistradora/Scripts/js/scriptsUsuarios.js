function AgregarForms(urlForms) {
    $.get(urlForms + '/', function (data) {
        abrirModal(data);
    })
}
function AgregarUsuario(urlAgregar) {
    var idusuario = $('#id').val()
    var nombre = $('#inputname').val();
    var apellido = $('#inputapellido').val();
    var nombreUsuario = $('#nombreUsuario').val();
    var clave = $('#clave').val();
    var rolid = $('#rolesId').val();
    var rutaImg = $('#rutaImg').val();

    let isValidCampo = true;
    isValidCampo = validarCampos(nombre, apellido, nombreUsuario, clave, rolid);

    if (isValidCampo) {
        var usuario = {
            "id": idusuario,
            "nombre": nombre,
            "apellido": apellido,
            "nombreUsuario": nombreUsuario,
            "clave": clave,
            "rutaImg": rutaImg,
            "rolid": rolid
        }
        $.post(urlAgregar + '/', usuario, function (data) {
            if (data > 0) {
                sessionStorage.clear();
                sessionStorage.nombre = usuario.nombreUsuario;
                sessionStorage.mensaje = 'Usuario creado correctamente: ';
                location = location.href;
            } else {
                mensaje = "Error: Ocurrio un error en el servidor, intentelo nuevamente";
                showMenssage('error', mensaje, true);
            }
        });
    } else {
        mensaje = "Error: Los campos estan vacios o la clave no cumple los requisitos";
        showMenssage('error', mensaje, true);
    }

}

const abrirModal = (data) => {
    $('#coreModal').html(data);
    $('#coreModal').modal('show');
}
const validarCampos = (nombre, apellido, nombreUsuario, clave, rolid) =>  {
    let valid = true;
    let atr = '';
    let pass = clave.trim();
    if (nombre.trim() == "") {
        atr = $('#text_nombre').attr('class');
        $('#text_nombre').addClass(atr + ' text-danger');
        $('#inputname').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_nombre').removeClass('text-danger');
        $('#inputname').css('border-color', '');
    }
    if (apellido.trim() == "") {
        atr = $('#text_apellido').attr('class');
        $('#text_apellido').addClass(atr + ' text-danger');
        $('#inputapellido').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_nombre').removeClass('text-danger');
        $('#inputapellido').css('border-color', '');
    }
    if (nombreUsuario.trim() == "") {
        atr = $('#text_nombreUsuario').attr('class');
        $('#text_nombreUsuario').addClass(atr + ' text-danger');
        $('#nombreUsuario').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_nombreUsuario').removeClass('text-danger');
        $('#nombreUsuario').css('border-color', '');
    }
    if (pass == "" || pass.length < 6) {
        atr = $('#text_clave').attr('class');
        $('#text_clave').addClass(atr + ' text-danger');
        atr = $('#passwordInfo').attr('class');
        $('#passwordInfo').addClass(atr + ' text-danger');
        $('#clave').css('border-color', 'red');

        valid = false;
    } else {
        $('#passwordInfo').removeClass('text-danger')
        $('#text_clave').removeClass('text-danger');
        $('#clave').css('border-color', '');
    }
    if (rolid.trim() == "") {
        atr = $('#text_rol').attr('class');
        $('#text_rol').addClass(atr + ' text-danger');
        $('#rolesId').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_rol').removeClass('text-danger');
        $('#rolesId').css('border-color', '');
    }
    return valid;
}
const showMenssage = (type, mensaje, toast) => {
    if (type == 'error') {
        Swal.fire({
            icon: 'error',
            title: 'Error!',
            text: mensaje,
            toast: toast,
            position: 'top-end'
        });
        return;
    } else if (type == 'success') {
        Swal.fire({
            icon: 'success',
            title: 'Exito!',
            text: mensaje,
            toast: toast,
            position: 'top-end',
        });
        return;
    } else if (type == 'warning') {
        const swal = Swal.mixin({
            customClass: {
                confirmButton: 'btn btn-success',
                cancelButton: 'btn btn-danger'
            },
            buttonsStyling: false
        });
        swal.fire({
            title: 'Informacion!',
            text: mensaje,
            icon: type,
            showCancelButton: true,
            confirmButtonText: 'Si',
            cancelButtonText: 'No',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                verificarOpcion(true)
            }
            return;
        })
    }
    console.log('Type not found')
}