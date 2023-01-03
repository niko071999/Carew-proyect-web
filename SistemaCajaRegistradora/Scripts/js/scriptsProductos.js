let mensaje = '';
let lista = [];
let min = 0;
let max = 0;

if (!!document.getElementById('stockProd')) {
    $(document).ready(function () {
        obtenerPrecios();
    });

    var storageRef;

    //listener modal
    let coreModal = document.getElementById('coreModal');
    coreModal.addEventListener('shown.bs.modal', function () {
        var codigo_barra = $('#codigo_barra').val();
        if (!!document.getElementById('codigo_barra')) {
            if (codigo_barra.trim() != '') {
                $("#barcode").show();
                JsBarcode("#barcode", codigo_barra.trim(), {
                    format: "EAN13",
                    lastChar: ">",
                    width: 3,
                });
                $("#texto_infocodigobarra").hide();
            }
            sessionStorage.codigobarrainit = codigo_barra.trim();
        }
        
    });

    let stockProd = document.getElementById('stockProd');
    let borrarCod = document.getElementById('btnBorrar');

    stockProd.addEventListener("change", verificarStock);
    borrarCod.addEventListener("click", borrarCodigo);
    $("#btn_reset").click(function () {
        let preciomin = parseInt($("#text_preciomin").val().replace(/[$.]/g, ''));
        let preciomax = parseInt($("#text_preciomax").val().replace(/[$.]/g, ''));
        if (preciomin != min || preciomax != max) {
            $("#text_preciomin").val(min);
            $("#text_preciomax").val(max);
            formatearIputs();
            resetTable1();
        }
    });

    $("#btnAplicar").attr('disabled', true);
    $("#text_preciomin").change(function () {
        let txt_min = parseInt($("#text_preciomin").val().replace(/[$.]/g, ''));
        if (txt_min < min || txt_min > max) {
            $("#text_preciomin").val(min);
            formatearIputs();
        } else {
            formatearIputs();
        }
    });
    $("#text_preciomax").change(function () {
        let txt_max = parseInt($("#text_preciomax").val().replace(/[$.]/g, ''));
        if (txt_max < min || txt_max > max) {
            $("#text_preciomax").val(max);
            formatearIputs();
        } else {
            formatearIputs();
        }
    });

    let collapseStock = document.getElementById('accordionPanelsStayOpenExample')
    collapseStock.addEventListener('shown.bs.collapse', function () {
        //Cargo por defecto el input de cantidad en 1
        $('#stockProd').val(1);
    })

    $('#codigoAdd').bind('keyup', function (e) {
        var key = e.keyCode || e.which;
        cargarExistencias(key);
    });
    $('#stockProd').bind('keyup', function (e) {
        var key = e.keyCode || e.which;
        cargarExistencias(key);
    });
}
function cargarExistencias(key) {
    var codigo = $('#codigoAdd').val();
    var newStock = $('#stockProd').val();
    var producto = {
        "id": 0,
        "codigo_barra": codigo,
        "stock": newStock,
    };
    if (key === 13 && codigo.trim() != '') {
        $.post('/Producto/addExistencias', producto, function (data) {
            console.log("1: ", lista);
            if (data.codigobarra != "") {
                let exist = false;
                $('#listProductos').empty();
                if (lista.length > 0) {
                    for (let i = 0; i < lista.length; i++) {
                        if (lista[i].codigobarra == data.codigobarra) {
                            lista[i].newstock = lista[i].newstock + data.newstock;
                            exist = true;
                        }
                    }
                    if (!exist) {
                        lista.push(data);
                    }
                } else {
                    lista.push(data);
                }

                $('#btnAplicar').attr('disabled', false);

                //Se carga la lista para ser visualizada
                drawList();

                mensaje = 'Codigo de barra ' + producto.codigo_barra + ' añadido';
                showMenssage('success', mensaje, true);
            } else {
                mensaje = "No existe ningun producto asociado a este codigo de barra";
                showMenssage('error', mensaje, true);
                $('#codigoAdd').val('');
            }
        }, 'json');
    };
}
function drawList() {
    for (let i = 0; i < lista.length; i++) {
        //diferencia = lista[i].newstock - lista[i].oldStock;
        $('#listProductos').append(
            `<li id="${lista[i].codigobarra}" class="list-group-item">${lista[i].codigobarra} - ${lista[i].nombre}
                    <span class="badge bg-primary rounded-pill">
                        ${lista[i].newstock} ingresadas
                    </span>
                    <button type="button" class="btn-close" aria-label="Close" onclick="quitarItem(${lista[i].codigobarra})"></button>
                </li>`
        );
        $('#codigoAdd').val('');
        $('#codigoAdd').focus();
        $('#stockProd').val(1);
    }
}
function aplicarExistencias() {
    let error = false;
    if (lista.length > 0) {
        for (let i = 0; i < lista.length; i++) {
            var producto = {
                "id": 0,
                "codigo_barra": lista[i].codigobarra,
                "stock": lista[i].newstock,
            };
            $.post('/Producto/aplicarExistencias', producto, function (data) {
                if (data > 0) {
                    error = false;
                } else {
                    error = true;
                }
            }, 'json');
        }
    } else {
        mensaje = "Lista vacia, no existe ningun cambio en las existencias.";
        showMenssage('error', mensaje, true);
    }
    if (error) {
        mensaje = "No se actualizo ningun producto";
        showMenssage('error', mensaje, true);
    } else {
        sessionStorage.clear();
        sessionStorage.mensaje = 'Se actualizo las existencia/s de los producto/s';
        location = location.href
    }
}
function cancelar() {
    lista = [];
    $('#codigoAdd').val('');
    $('#listProductos').empty();
    var myCollapse = document.getElementById('addExistenciasPanel')
    var bsCollapse = new bootstrap.Collapse(myCollapse, {
        hide: true,
    });
    $('#btnAplicar').attr('disabled', true);
    sessionStorage.clear();
}
function verificarStock() {
    let cantidadProd = stockProd.value;
    if (cantidadProd < 1) {
        stockProd.value = 1;
    }
}
function borrarCodigo() {
    $("#codigoAdd").val('');
}
function quitarItem(codigoItem) {
    lista = lista.filter((item) => item.codigobarra != codigoItem);
    $('#listProductos').empty();
    if (lista.length == 0) {
        $('#btnAplicar').attr('disabled', true);
    }
    drawList();
}

function AgregarForms(urlForms) {
    $.get(urlForms + '/', function (data) {
        abrirModal(data);
    });
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
    var imagenid = $('#imagenid').val();
    var fecha_creacion = "";

    precio = quitarPuntoNumber(precio);
    stock = quitarPuntoNumber(stock);
    stockmin = quitarPuntoNumber(stockmin);
    stockmax = quitarPuntoNumber(stockmax);

    //Validaciones campos vacios
    let isValidCampo = true;
    isValidCampo = validarCamposPro(codigo_barra, nombre, prioridadid, categoriaid);

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
                    "imagenid": imagenid
                };

                $.post(urlAgregar + '/', producto, function (data) {
                    if (data > 0) {
                        sessionStorage.clear();
                        sessionStorage.codigo_barra = producto.codigo_barra;
                        sessionStorage.nombre = producto.nombre;
                        sessionStorage.mensaje = 'Producto creado correctamente: ';
                        location = location.href;
                    } else {
                        mensaje = "Error: Hubo un problema al ingresar el productos, intentelo de nuevo o mas tarde";
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
                "imagenid": imagenid
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
    var imagenid = $('#imagenid').val();
    var fecha_creacion = $('#fecha_creacion').val();

    precio = quitarPuntoNumber(precio);
    stock = quitarPuntoNumber(stock);
    stockmin = quitarPuntoNumber(stockmin);
    stockmax = quitarPuntoNumber(stockmax);

    let validarCodigoExist = verificar(codigo_barra);

    if (!validarCodigoExist) {
        var isValidCampo = true;
        isValidCampo = validarCamposPro(codigo_barra, nombre, prioridadid, categoriaid);

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
                        "imagenid": imagenid
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
                    "imagenid": imagenid
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
function formsImagenP(urlFormsImagen, id) {
    $.get(urlFormsImagen + '/' + id, function (data) {
        abrirModal(data);
    });
}
function subirImagenP(url) {
    var inputArchivoId = document.getElementById('idArchivo');
    var archivo = inputArchivoId.files[0];
    let nameFile = Date.now().toString() + '_producto';

    // Create a root reference
    storageRef = firebase.storage().ref();

    // Create a reference
    var uploadTask = storageRef.child('productos')
        .child(nameFile)
        .put(archivo);

    uploadTask.on(firebase.storage.TaskEvent.STATE_CHANGED, // or 'state_changed'
        (snapshot) => {
            // Get task progress, including the number of bytes uploaded and the total number of bytes to be uploaded
            var progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
            console.log('Upload is ' + progress + '% done');
            console.log(snapshot.state);
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
                    alert('Se ha cancelado la subida de imagen');
                    console.error('La carga a sido cancelada');
                    break;
                case 'storage/unknown':
                    alert('Ha ocurrido un error desconocido');
                    console.error('A ocurrido un error desconocido');
                    break;
            }
        },
        () => {
            // Upload completed successfully, now we can get the download URL
            uploadTask.snapshot.ref.getDownloadURL().then((downloadURL) => {
                var dataForm = new FormData();
                dataForm.append('downloadURL', downloadURL);
                dataForm.append('nameFile', nameFile);
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: dataForm,
                    contentType: false,
                    processData: false,
                    success: function (data) {
                        console.log(data);
                        switch (data.status) {
                            case 'success':
                                if (data.idimg != 1) {
                                    eliminarImagenStorageP(data.nombreImg)
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
                                eliminarImagenStorageP(nameFile);
                            default:
                        }
                    },
                    error: function (data) {
                        data.mensaje = "Error: La imagen no se a cambiado o no se a subido al servidor";
                        showMenssage('error', mensaje, true);
                        eliminarImagenStorageP(nameFile);
                    },
                });
            });
        }
    );
}
function restablecerImagenP(url) {
    const swal = Swal.mixin({
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-danger'
        },
        buttonsStyling: false
    });
    swal.fire({
        title: 'Restablecer Imagen',
        text: '¿Seguro de restablecer la imagen a la predeterminada?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Si',
        cancelButtonText: 'No',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(url, function (data) {
                switch (data.status) {
                    case 'error':
                        showMenssage('error', data.mensaje, true);
                        break;
                    case 'success':
                        eliminarImagenStorageP(data.nameFileOld);
                        sessionStorage.clear();
                        sessionStorage.mensaje = data.mensaje;
                        location = location.href;
                        break;
                    default:
                        alert('ERROR INNESPERADO!');
                }
            });
        }
        return;
    });
}
function eliminarImagenStorageP(namefile) {
    if (storageRef == undefined) {
        storageRef = firebase.storage().ref();
    }
    var productosRef = storageRef.child('productos');
    var productoRef = productosRef.child(namefile);
    productoRef.delete().then(() => {
        //Success
    }).catch((error) => {
        console.error(error);
        let mensaje = 'Ocurrio un error al eliminar la imagen';
        showMenssage('error', mensaje, true);
    });
}
function eliminarDocumentoStorage(namefile) {
    if (storageRef == undefined) {
        storageRef = firebase.storage().ref();
    }
    var docsRef = storageRef.child('documento');
    var docRef = docsRef.child(namefile);
    docRef.delete().then(() => {
        console.log('documento eliminado');
    }).catch((error) => {
        console.error(error);
        let mensaje = 'Ocurrio un error al eliminar el documento';
        showMenssage('error', mensaje, true);
    });
}
function formsEliminar(urlFormsEliminar, id) {
    $.get(urlFormsEliminar + '/' + id, function (data) {
        abrirModal(data);
    });
}
function eliminarProducto(urlEliminar, id) {
    $.post(urlEliminar + '/' + id, function (data) {
        if (data.n > 0) {
            if (data.nameFile != '') {
                if (data.idimg != 1) {
                    eliminarImagenStorageP(data.nameFile);
                    sessionStorage.clear();
                    sessionStorage.mensaje = 'Producto eliminado correctamente';
                    location = location.href;
                }
                sessionStorage.clear();
                sessionStorage.mensaje = 'Producto eliminado correctamente';
                location = location.href;
            }
            sessionStorage.clear();
            sessionStorage.mensaje = 'Producto eliminado correctamente';
            location = location.href;
        } else {
            mensaje = "Error: El producto no se elimino, asegurese de que el producto no se ocupe en algun producto";
            showMenssage('error', mensaje, true);
        }
    }, 'json');
}
function cargarProdsForms(url) {
    $.get(url, function (data) {
        abrirModal(data);
    });
}
function cargarProdsFile(urlLoadProds) {
    let inputArchivoId = document.getElementById('idArchivo');
    let archivo = inputArchivoId.files[0];
    let nameFile = Date.now().toString() + '_docsvTEMP';

    // Create a root reference
    storageRef = firebase.storage().ref();

    // Create a reference
    var uploadTask = storageRef.child('documento')
        .child(nameFile)
        .put(archivo);

    uploadTask.on(firebase.storage.TaskEvent.STATE_CHANGED, // or 'state_changed'
        (snapshot) => {
            // Get task progress, including the number of bytes uploaded and the total number of bytes to be uploaded
            var progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
            console.log('Upload is ' + progress + '% done');
            console.log(snapshot.state);
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
                    alert('Se ha cancelado la subida del documento');
                    console.error('La carga a sido cancelada');
                    break;
                case 'storage/unknown':
                    alert('Ha ocurrido un error desconocido');
                    console.error('A ocurrido un error desconocido');
                    break;
            }
        },
        () => {
            // Upload completed successfully, now we can get the download URL
            uploadTask.snapshot.ref.getDownloadURL().then((downloadURL) => {
    var dataForm = new FormData();
                dataForm.append('downloadURL', downloadURL);
    $.ajax({
        url: urlLoadProds,
        type: 'POST',
        data: dataForm,
        contentType: false,
        processData: false,
        success: function (data) {
                        eliminarDocumentoStorage(nameFile);
            if (data.errorProd.length > 0 || data.prodRepetidos.length > 0) {
                for (var i = 0; i < data.errorProd.length; i++) {
                    msgErrorProd = msgErrorProd + data.errorProd[i] + '\n';
                }
                for (var i = 0; i < data.prodRepetidos.length; i++) {
                    msgProdRep = msgProdRep + data.prodRepetidos[i] + '\n';
                }
                Swal.fire({
                    title: 'Hubieron errores con los siguientes productos',
                    text: msgErrorProd+msgProdRep,
                    icon: 'info',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'Esta bien',
                    width: 900
                }).then(() => {
                    switch (data.status) {
                        case 'success':
                            sessionStorage.clear();
                            sessionStorage.mensaje = data.msg;
                            location = location.href;
                            break;
                        case 'error':
                            showMenssage('error', data.msg, true);
                        default:
                    }
                });
            } else {
                switch (data.status) {
                    case 'success':
                        sessionStorage.clear();
                        sessionStorage.mensaje = data.msg;
                        location = location.href;
                        break;
                    case 'error':
                        showMenssage('error', data.msg, true);
                    default:
                }
            }
        },
        error: function (data) {
            data.mensaje = "Error: La imagen no se a cambiado o no se a subido al servidor";
            showMenssage('error', mensaje, true);
        },
    });
            });
}
    );
}

const validarCamposPro = (codigo_barra, nombre, prioridadid, categoriaid) => {
    let valid = true;
    let atr = '';
    if (codigo_barra.trim() == "") {
        atr = $('#text_codigo').attr('class');
        $('#text_codigo').addClass(atr + ' text-danger');
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
    }
    return valid;
}

function obtenerPrecios() {
    let precios = [];
    let productos = [];
    const promesa = new Promise((resolver) => {
        $.get('/Producto/getProductos/', function (data) {
            productos = data;
            $.each(productos.data, function (index, item) {
                precios.push(parseInt(item.precio));
            });
            resolver(precios);
        });
    });
    promesa.then((data) => {
        min = Math.min.apply(null, data);
        max = Math.max.apply(null, data);

        $("#text_preciomin").val(min);
        $("#text_preciomax").val(max);

        formatearIputs();
    });
}
function accion(cod, id) {
    //SI COD == 1 : EDITAR - SI COD == 2 : BORRAR - SI COD == 3 : VISUALIZAR IMAGEN
    switch (cod) {
        case (1):
            formsEditar('/Producto/formsEditar/', id);
            break;
        case (2):
            formsEliminar('/Producto/formsEliminar/', id);
            break;
        case (3):
            formsImagenP('/Producto/formsImagen/', id)
            break;
        default:
            mensaje = 'Se ha producido un error inesperado, intentelo nuevamente o recargue la pagina';
            showMenssage('error', mensaje, true);
    }
}
function desabilitar() {
    var inputArchivoId = document.getElementById('idArchivo');
    var btn = document.getElementById('btnSubir');
    if (inputArchivoId.files[0] == undefined) {//No hay imagenes
        btn.disabled = true;
    } else {//Si hay imagenes
        btn.disabled = false;
    }
}
function formatearIputs() {
    $("#text_preciomin").val(parseFloat($("#text_preciomin").val().replace(/[$.]/g, ''))
        .toLocaleString('es-CL'));
    $("#text_preciomax").val(parseFloat($("#text_preciomax").val().replace(/[$.]/g, ''))
        .toLocaleString('es-CL'));
}

const quitarPuntoNumber = (number) => {
    return number.replace(',', '');
}


