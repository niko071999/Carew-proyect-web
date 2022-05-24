let mensaje = '';

function AgregarForms(urlForms) {
    $.get(urlForms+'/', function (data) {
        abrirModal(data);
    })
}
function AgregarProducto(urlAgregar) {
    var idproducto = $('#id').val();
    var codigo_barra = $('#codigo_barra').val();
    var nombre = $('#nombre').val();
    var precio = $('#precio').val();
    var stock = $('#stock').val();
    var stockmin = $('#stockmin').val();
    var stockmax = $('#stockmax').val();
    var prioridadid = $('#prioridadId').val();
    var categoriaid = $('#categoriaId').val();
    var rutaImg = $('#rutaImg').val();
    var fecha_creacion = "";

    precio = quitarPuntosNumber(precio);
    stock = quitarPuntosNumber(stock);
    stockmin = quitarPuntosNumber(stockmin);
    stockmax = quitarPuntosNumber(stockmax);

    if (!validarCodigoExist) {
        //Validaciones campos vacios
        let isValidCampo = true;
        isValidCampo = validarCampos(codigo_barra, nombre, prioridadid, categoriaid);

        if (isValidCampo) {
            if ((stockmin != "" || stockmax != "") || (stockmin == '0' || stockmax == '0')) {
                if (parseInt(stockmin) <= parseInt(stockmax) || parseInt(stockmax) >= parseInt(stockmin)) {
                    var producto = {
                        "id": idproducto,
                        "codigo_barra": codigo_barra,
                        "nombre": nombre,
                        "precio": precio,
                        "stock": stock,
                        "stockmin": stockmin,
                        "stockmax": stockmax,
                        "fecha_creacion": fecha_creacion,
                        "prioridadid": prioridadid,
                        "categoriaid": categoriaid,
                        "rutaImg": rutaImg
                    };

                    $.post(urlAgregar + '/', producto, function (data) {
                        if (data > 0) {
                            sessionStorage.clear();
                            sessionStorage.codigo_barra = producto.codigo_barra;
                            sessionStorage.nombre = producto.nombre;
                            sessionStorage.mensaje = 'Producto creado correctamente: ';
                            location = location.href;
                        } else {
                            alert("Error");
                        }
                    });
                } else {
                    mensaje = "Error: Stock minimo mayor que el maximo o stock maximo menor que el stock minimo";
                    showMenssage('error', mensaje, true);
                }
            } else {
                var producto = {
                    "id": idproducto,
                    "codigo_barra": codigo_barra,
                    "nombre": nombre,
                    "precio": precio,
                    "stock": stock,
                    "stockmin": stockmin,
                    "stockmax": stockmax,
                    "fecha_creacion": fecha_creacion,
                    "prioridadid": prioridadid,
                    "categoriaid": categoriaid,
                    "rutaImg": rutaImg
                };

                $.post(urlAgregar + '/', producto, function (data) {
                    if (data > 0) {
                        sessionStorage.clear();
                        sessionStorage.codigo_barra = producto.codigo_barra;
                        sessionStorage.nombre = producto.nombre;
                        sessionStorage.mensaje = 'Producto creado correctamente: ';
                        location = location.href;
                    } else {
                        mensaje = "Error: ocurrio un error en el servidor";
                        showMenssage('error', mensaje, true);
                    }
                });
            }
        } else {
            mensaje = "Error: Los campos estan vacios";
            showMenssage('error', mensaje, true);
        }
    }
}
function formsEditar(urlEditarForms, id) {
    $.get(urlEditarForms + '/' + id, function (data) {
        abrirModal(data);
    });
}
function editarProducto(urlEditar) {
    var idproducto = $('#id').val();
    var codigo_barra = $('#codigo_barra').val();
    var nombre = $('#nombre').val();
    var precio = $('#precio').val();
    var stock = $('#stock').val();
    var stockmin = $('#stockmin').val();
    var stockmax = $('#stockmax').val();
    var prioridadid = $('#prioridadId').val();
    var categoriaid = $('#categoriaId').val();
    var rutaImg = $('#rutaImg').val();
    var fecha_creacion = $('#fecha_creacion').val();

    let validarCodigoExist = verificar(codigo_barra);

    if (!validarCodigoExist) {
        var isValidCampo = true;
        isValidCampo = validarCampos(codigo_barra, nombre, prioridadid, categoriaid);

        if (isValidCampo) {
            if (stockmin != "" || stockmax != "" || stockmin == 0 || stockmax == 0) {
                if (parseInt(stockmin) <= parseInt(stockmax) || parseInt(stockmax) >= parseInt(stockmin)) {
                    var producto = {
                        "id": idproducto,
                        "codigo_barra": codigo_barra,
                        "nombre": nombre,
                        "precio": precio,
                        "stock": stock,
                        "stockmin": stockmin,
                        "stockmax": stockmax,
                        "fecha_creacion": fecha_creacion,
                        "prioridadid": prioridadid,
                        "categoriaid": categoriaid,
                        "rutaImg": rutaImg
                    };
                    $.post(urlEditar + '/', producto, function (data) {
                        if (data > 0) {
                            sessionStorage.clear();
                            sessionStorage.codigo_barra = producto.codigo_barra;
                            sessionStorage.nombre = producto.nombre;
                            sessionStorage.mensaje = 'Producto modificado correctamente: ';
                            location = location.href;
                        } else {
                            mensaje = "Error: ocurrio un error en el servidor";
                            showMenssage('error', mensaje, true);
                        }
                    });
                } else {
                    mensaje = "Error: Stock minimo mayor que el maximo o stock maximo menor que el stock minimo";
                    showMenssage('error', mensaje, true);
                }
            } else {
                var producto = {
                    "id": idproducto,
                    "codigo_barra": codigo_barra,
                    "nombre": nombre,
                    "precio": precio,
                    "stock": stock,
                    "stockmin": stockmin,
                    "stockmax": stockmax,
                    "fecha_creacion": fecha_creacion,
                    "prioridadid": prioridadid,
                    "categoriaid": categoriaid,
                    "rutaImg": rutaImg
                };
                $.post(urlEditar + '/', producto, function (data) {
                    if (data > 0) {
                        sessionStorage.clear();
                        sessionStorage.codigo_barra = producto.codigo_barra;
                        sessionStorage.nombre = producto.nombre;
                        sessionStorage.mensaje = 'Producto modificado correctamente: ';
                        location = location.href;
                    } else {
                        alert("Error");
                    }
                });
            }
        } else {
            mensaje = "Error: Los campos estan vacios";
            showMenssage('error', mensaje, true);
        }
    }
}
function formsImage(urlFormsImagen, id) {
    $.get(urlFormsImagen + '/' + id, function (data) {
        abrirModal(data);
    });
}
function subirImagen(url) {
    var inputArchivoId = document.getElementById('idArchivo');
    var archivo = inputArchivoId.files[0];
    var dataForm = new FormData();

    dataForm.append('archivo', archivo);
    $.ajax({
        url: url,
        type: 'POST',
        data: dataForm,
        contentType: false,
        processData: false,
        success: function (data) {
            sessionStorage.clear();
            sessionStorage.mensaje = 'Imagen subida correctamente';
            location = location.href;
        },
        error: function (data) {
            mensaje = "Error: La imagen no se a cambiado o no se a subido al servidor";
            showMenssage('error', mensaje);
        },
    });
}
function formsEliminar(urlFormsEliminar, id) {
    $.get(urlFormsEliminar + '/' + id, function (data) {
        abrirModal(data);
    });
}
function eliminarProducto(urlEliminar, id) {
    $.post(urlEliminar + '/' + id, function (data) {
        if (data > 0 ) {
            sessionStorage.clear();
            sessionStorage.mensaje = 'Producto eliminado correctamente';
            location = location.href;
        } else {
            mensaje = "Error: El producto no se elimino, asegurese de que el producto no se ocupe en algun producto";
            showMenssage('error', mensaje, true);
        }
    },'json');
}

const abrirModal = (data) => {
    $('#coreModal').html(data);
    $('#coreModal').modal('show');
}

const validarCampos = (codigo_barra, nombre, prioridadid, categoriaid) => {
    let valid = true;
    let atr = '';
    if (codigo_barra.trim() == "") {
        atr = $('#text_codigo').attr('class');
        $('#text_codigo').addClass(atr +' text-danger')
        $('#codigo_barra').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_codigo').removeClass('text-danger');
        $('#codigo_barra').css('border-color', '');
    }
    if (nombre.trim() == "") {
        atr = $('#text_nombre').attr('class');
        $('#text_nombre').addClass(atr + ' text-danger')
        $('#nombre').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_nombre').removeClass('text-danger');
        $('#nombre').css('border-color', '');
    }
    if (prioridadid == "") {
        atr = $('#text_prioridad').attr('class');
        $('#text_prioridad').addClass(atr + ' text-danger')
        $('#prioridadId').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_prioridad').removeClass('text-danger');
        $('#prioridadId').css('border-color', '');
    }
    if (categoriaid == "") {
        atr = $('#text_categoria').attr('class');
        $('#text_categoria').addClass(atr + ' text-danger')
        $('#categoriaId').css('border-color', 'red');
        valid = false;
    } else {
        $('#text_categoria').removeClass('text-danger');
        $('#categoriaId').css('border-color', '');
    }
    return valid;
}

const esNatural = (number) => {
    if (!isNaN(number)) {
        if (parseInt(number) % 1 == 0) {
            return true;
        } else {
            return false;
        }
        return true;
    } else {
        return false;
    }
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

const quitarPuntoNumber = (number) => number.replace(',', '');

function borrarCodigo() { $('#codigo_barra').val(''); }

