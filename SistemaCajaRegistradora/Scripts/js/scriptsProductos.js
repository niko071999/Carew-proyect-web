$(document).ready(function () {
    obtenerPrecios();    
});

let mensaje = '';
let lista = [];
let min = 0;
let max = 0;

let stockProd = document.getElementById('stockProd');
let borrarCod = document.getElementById('btnBorrar');
stockProd.addEventListener("change", verificarStock);
borrarCod.addEventListener("click", borrarCodigo);

$("#btnAplicar").attr('disabled', true);
$("#text_preciomin").change(function () {
    let txt_min = parseInt($("#text_preciomin").val().replace(/[$.]/g, ''));
    if (txt_min < min || txt_min > max) {
        $("#text_preciomin").val(min);
        formatearIputs();
    }
});
$("#text_preciomax").change(function () {
    let txt_max = parseInt($("#text_preciomax").val().replace(/[$.]/g, ''));
    if (txt_max < min || txt_max > max) {
        $("#text_preciomax").val(max);
        formatearIputs();
    }
});

//listener modal
let coreModal = document.getElementById('coreModal');
coreModal.addEventListener('shown.bs.modal', function () {
    var codigo_barra = $('#codigo_barra').val();
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

function formatearIputs() {
    $("#text_preciomin").val(parseFloat($("#text_preciomin").val().replace(/[$.]/g, ''))
        .toLocaleString('es-CL'));
    $("#text_preciomax").val(parseFloat($("#text_preciomax").val().replace(/[$.]/g, ''))
        .toLocaleString('es-CL'));
}
function cargarExistencias(key) {
    var codigo = $('#codigoAdd').val();
    var newStock = $('#stockProd').val();
    var producto = {
        "id": 0,
        "codigo_barra": codigo,
        "nombre": 'test',
        "precio": 0,
        "stock": newStock,
        "stockmin": 0,
        "stockmax": 0,
        "fecha_creacion": null,
        "prioridadid": 0,
        "categoriaid": 0,
        "rutaImg": 'test'
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
                "nombre": 'test',
                "precio": 0,
                "stock": lista[i].newstock,
                "stockmin": 0,
                "stockmax": 0,
                "fecha_creacion": null,
                "prioridadid": 0,
                "categoriaid": 0,
                "rutaImg": 'test'
            };
            $.post('/Producto/aplicarExistencias', producto, function (data) {
                if (data > 0) {
                    error = false;
                } else {
                    error = true;
                }
            });
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
function formsImagenP(urlFormsImagen, id) {
    $.get(urlFormsImagen + '/' + id, function (data) {
        abrirModal(data);
    });
}
function subirImagenP(url) {
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
            formsImagenP('/Producto/formsImagen/',id)
            break;
        default:
            mensaje = 'Se ha producido un error inesperado, intentelo nuevamente o recargue la pagina';
            showMenssage('error', mensaje, true);
    }
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
const quitarPuntoNumber = (number) => {
    return number.replace(',', '');
}
function borrarCodigo() { $('#codigo_barra').val(''); }

