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
    var rutaImg = '';
    var solrespass = false;

    let isValidCampo = true;
    isValidCampo = validarCampos(nombre, apellido, nombreUsuario, clave);

    if (isValidCampo) {
        var usuario = {
            "id": idusuario,
            "nombre": nombre,
            "apellido": apellido,
            "nombreUsuario": nombreUsuario,
            "clave": clave,
            "rutaImg": rutaImg,
            "rolid": null,
            "solrespass": solrespass,
            "fecha_creacion": null,
            "fecha_modifiacion": null
        }
        $.post(urlAgregar + '/', usuario, function (data) {
            if (data > 0) {
                sessionStorage.clear();
                sessionStorage.nombre = usuario.nombreUsuario;
                sessionStorage.mensaje = 'Usuario creado correctamente: ';
                location = location.href;
            } else if (data == -1) {
                mensaje = "Error: Solo se puede crear 1 administrador en el sistema. Ya existe un usuario en el sistema";
                showMenssage('error', mensaje, true);
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
function formsEditar(urlEditarForms, id) {
    $.get(urlEditarForms + '/' + id, function (data) {
        abrirModal(data);
    });
}
function editarUsuario(urlEditar) {
    var idusuario = $('#id').val()
    var nombre = $('#inputname').val();
    var apellido = $('#inputapellido').val();
    var nombreUsuario = $('#nombreUsuario').val();
    var clave = $('#clave').val();
    var rolid = $('#rolid').val();
    var rutaImg = $('#rutaImg').val();
    var solrespass = $('#solrespass').val();

    let isValidCampo = true;
    isValidCampo = validarCampos(nombre, apellido, nombreUsuario, clave);

    if (isValidCampo) {
        var usuario = {
            "id": idusuario,
            "nombre": nombre,
            "apellido": apellido,
            "nombreUsuario": nombreUsuario,
            "clave": clave,
            "rutaImg": rutaImg,
            "rolid": rolid,
            "solrespass": solrespass,
            "fecha_creacion": null,
            "fecha_modifiacion": null
        }
        $.post(urlEditar + '/', usuario, function (data) {
            if (data > 0) {
                sessionStorage.clear();
                sessionStorage.nombre = usuario.nombreUsuario;
                sessionStorage.mensaje = 'Usuario modificado correctamente: ';
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
function formsEliminar(urlFormsEliminar, id) {
    $.get(urlFormsEliminar + '/' + id, function (data) {
        abrirModal(data);
    });
}
function eliminarUsuario(urlEliminar, id) {
    $.post(urlEliminar + '/' + id, function (data) {
        if (data > 0) {
            sessionStorage.clear();
            sessionStorage.mensaje = 'Usuario eliminado correctamente';
            location = location.href;
        } else {
            mensaje = "Error: El Usuario no se elimino";
            showMenssage('error', mensaje, true);
        }
    }, 'json');
}
function cambiarClave(urlChange, pass) {
    let usuario = {
        "id": 0,
        "nombre": 'n',
        "apellido": 'a',
        "nombreUsuario": 'new',
        "clave": pass,
        "rutaImg": 'r',
        "rolid": 0,
        "solrespass": solrespass,
        "fecha_creacion": null,
        "fecha_modifiacion": null
    }
    $.post(urlChange+'/', usuario, function (data) {
        if (data > 0) {
            sessionStorage.clear();
            sessionStorage.mensaje = 'Se cambio la contraseña correctamente';
            location = location.href;
        } else {
            mensaje = "Error: El Usuario no se elimino debido a problemas de servicio";
            showMenssage('error', mensaje, true);
        }
    });
}
function formsImagenU(urlImagen,id) {
    $.get(urlImagen + '/' + id, function (data) {
        abrirModal(data);
    });
}
function subirImagenU(urlSubirIMG) {
    var inputArchivoId = document.getElementById('idArchivo');
    var archivo = inputArchivoId.files[0];
    var dataForm = new FormData();
    dataForm.append('archivo', archivo);

    $.ajax({
        url: urlSubirIMG,
        type: 'POST',
        data: dataForm,
        contentType: false,
        processData: false,
        success: function (data) {
            sessionStorage.clear();
            sessionStorage.mensaje = data.mensaje;
            location = location.href;
        },
        error: function (data) {
            mensaje = data.mensaje;
            showMenssage('error', mensaje);
        },
    });
}

const abrirModal = (data) => {
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
}
const validarCampos = (nombre, apellido, nombreUsuario, clave) =>  {
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
    if (rolid == "") {
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
function desabilitar() {
    var inputArchivoId = document.getElementById('idArchivo');
    var btn = document.getElementById('btnSubir');
    if (inputArchivoId.files[0] == undefined) {
        btn.disabled = true;
    } else {
        btn.disabled = false;
    }
}