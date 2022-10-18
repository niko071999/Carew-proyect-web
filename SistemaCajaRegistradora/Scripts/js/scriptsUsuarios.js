var coreModal = document.getElementById('coreModal');
var storageRef;
coreModal.addEventListener('shown.bs.modal', function () {
    var nombreusuario = $("#nombreUsuario").val();
    sessionStorage.nombreusuarioactual = nombreusuario;
    console.log(sessionStorage.nombreusuarioactual + ';');
});

initialFirebase();

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
        if (data.n > 0) {
            if (data.nameFile != '') {
                if (data.idimg != 2) {
                    eliminarImagenStorageU(data.nameFile);
                    sessionStorage.clear();
                    sessionStorage.mensaje = 'Usuario eliminado correctamente';
                    location = location.href;
                }
                sessionStorage.clear();
                sessionStorage.mensaje = 'Usuario eliminado correctamente';
                location = location.href;
            }
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
        "solrespass": null,
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
    let nameFile = Date.now().toString() + '_usuario';

    // Create a root reference
    storageRef = firebase.storage().ref();

    // Create a reference
    var uploadTask = storageRef.child('usuarios')
        .child(nameFile)
        .put(archivo);

    uploadTask.on(firebase.storage.TaskEvent.STATE_CHANGED, // or 'state_changed'
        (snapshot) => {
            // Get task progress, including the number of bytes uploaded and the total number of bytes to be uploaded
            var progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
            console.log('Upload is ' + progress + '% done');
            switch (snapshot.state) {
                case firebase.storage.TaskState.PAUSED: // or 'paused'
                    console.log('Upload is paused');
                    break;
                case firebase.storage.TaskState.RUNNING: // or 'running'
                    console.log('Upload is running');
                    break;
            }
        },
        (error) => {
            switch (error.code) {
                case 'storage/canceled':
                    console.error('La carga a sido cancelada')
                    break;
                case 'storage/unknown':
                    console.error('A ocurrido un error desconocido')
                    break;
            }
        },
        () => {
            // Upload completed successfully, now we can get the download URL
            uploadTask.snapshot.ref.getDownloadURL().then((downloadURL) => {
                console.log('File available at', downloadURL);
                var dataForm = new FormData();
                dataForm.append('downloadURL', downloadURL);
                dataForm.append('nameFile', nameFile);
                $.ajax({
                    url: urlSubirIMG,
                    type: 'POST',
                    data: dataForm,
                    contentType: false,
                    processData: false,
                    success: function (data) {
                        console.log(data);
                        switch (data.status) {
                            case 'success':
                                if (data.idimg != 2) {
                                    eliminarImagenStorageU(data.nombreImg);

                                    sessionStorage.clear();
                                    sessionStorage.mensaje = data.mensaje;
                                    location = location.href;
                                } else {
                                    sessionStorage.clear();
                                    sessionStorage.mensaje = data.mensaje;
                                    location = location.href;
                                }                                
                                break;
                            case 'error':
                                //Mostra un mensaje cuando llegue un error y eliminar la imagen de Firebase
                                showMenssage('error', data.mensaje, true);
                                eliminarImagenStorageU(nameFile);
                        }
                    },
                    error: function (data) {
                        console.log(data);
                        data.mensaje = "Error: La imagen no se a cambiado o no se a subido al servidor";
                        showMenssage('error', mensaje, true);
                        eliminarImagenStorageU(nameFile);
                    },
                });
            });
        }
    );    
}
function eliminarImagenStorageU(namefile) {
    if (storageRef == undefined) {
        storageRef = firebase.storage().ref();
    }
    var usuariosRef = storageRef.child('usuarios');
    var usuarioRef = usuariosRef.child(namefile);
    usuarioRef.delete().then(() => {
        //Success
    }).catch((error) => {
        // Uh-oh, an error occurred!
        console.error(error);
        let mensaje = 'Ocurrio un error al eliminar la imagen';
        showMenssage('error', mensaje, true);
    });
}
function restablecerClave(user) {
    const swal = Swal.mixin({
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-secundary'
        },
        buttonsStyling: false
    });
    swal.fire({
        title: 'Importante',
        text: 'La contraseña se restablecerá a "cajero". ¿Desea continuar con el proceso?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Si',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            try {
                $.ajax({
                    url: '/Usuario/RestablecerPass',
                    type: 'POST',
                    contentType: 'application/json;',
                    data: JSON.stringify({ user: user, clave: 'cajero' }),
                    success: function (res) {
                        sessionStorage.clear();
                        sessionStorage.mensaje = 'Contraseña restablecida con exito';
                        location = location.href;
                    }
                });
            } catch (e) {
                mensaje = "Error: Ocurrio un error en el servidor, intentelo nuevamente";
                showMenssage('error', mensaje, true);
            }
        }
        return;
    });
}
function obtenerCodigoBarra(id) {
    console.log(id);
    $.get('/Usuario/obtenerBarcodeUser/' + id, function (data) {
        if (data != null) {
            abrirModal(data);
        }
    })
}
function initialFirebase() {
    const firebaseConfig = {
        apiKey: "AIzaSyDaMMADA0YUdGIt1AVXAp1Z_PQYIYcOBQY",
        authDomain: "carewservices-0799.firebaseapp.com",
        projectId: "carewservices-0799",
        storageBucket: "carewservices-0799.appspot.com",
        messagingSenderId: "836513019982",
        appId: "1:836513019982:web:5d75e528d15578998eab1f"
    };

    firebase.initializeApp(firebaseConfig);
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