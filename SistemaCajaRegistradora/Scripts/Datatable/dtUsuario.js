$(document).ready(function () {
    let iduser;
    let tiporol;
    let nameuser;
    $("#tableUsuario").DataTable({
        responsive: 'true',
        ajax: {
            'url': '/Usuario/getUsuarios',
            'type': 'GET',
            'datatype': 'json'
        },
        columns: [
            {
                data: 'id',
                'render': function (id) {
                    iduser = id;
                    return iduser;
                },
                visible: false,
                searchable: false
            },
            {
                data: 'rutaImg',
                'render': function (rutaimg) {
                    return `<a href="javascript:void(0);" onclick="accionU(1,${iduser})">
                                <img class="rounded mx-auto d-block" src="${rutaimg}" alt="Imagen del usuario"
                                style="width: 80px; min-width: 50px; min-height: 50px; height: 80px;" />
                            </a>`
                },
                searchable: false,
                orderable: false
            },
            { data: 'nombreCajero' },
            {
                data: 'nombreUsuario',
                'render': function (nuser) {
                    nameuser = nuser;
                    return nuser;
                }
            },
            {
                data: 'rol',
                'render': function (rol) {
                    tiporol = rol;
                    return rol;
                }
            },
            {
                data: 'stateConexion',
                'render': function (state) {
                    if (state) {
                        return 'Conectado'
                    } else {
                        return 'Desconectado'
                    }
                }
            },
            {
                data: 'solrespass',
                'render': function (solicitud) {
                    let plantilla = `<button class="btn btn-warning" type="button" onclick="accionU(2,${iduser})"
                                    data-bs-toggle="tooltip" title="Editar usuario">
                                 <i class="fas fa-pen float-left"></i>
                            </button>`;
                    if (solicitud) {
                        plantilla += `\n<button onclick="accionU(3,'${nameuser}')" class="btn btn-dark" type="button"
                                    data-bs-toggle="tooltip" title="Restablecer clave de acceso">
                                <i class="fas fa-key float-left"></i>
                            </button>`
                    }
                    if (tiporol != 'Administrador') {
                        plantilla += `\n<button class="btn btn-danger" type="button" onclick="accionU(4,${iduser})"
                                    data - bs - toggle="tooltip" title = "Eliminar usuario" >
                                <i class="fas fa-trash float-left"></i>
                            </button >`
                    }
                    return plantilla;
                },
                searchable: false,
                orderable: false
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.12.0/i18n/es-ES.json',
        }
    });
});


//@if (!u.nombreUsuario.Equals(ViewBag.nombreUser)) {
//    <button class="btn btn-danger" type="button" onclick="formsEliminar('@Url.Action(" formsEliminar", "Usuario")', @u.id)"
//    data - bs - toggle="tooltip" title = "Eliminar usuario" >
//        <i class="fas fa-trash float-left"></i>
//                               </button >
//                            }
//<button class="btn btn-secondary" type="button" onclick="accion(3,${iduser})"
//    data-bs-toggle="tooltip" title="Obtener codigo de barra de acceso">
//    <i class="fas fa-barcode float-left"></i>
//</button>
//@if (u.solrespass == true) {
//    <button onclick="restablecerClave('@u.nombreUsuario.Trim()')" class="btn btn-dark" type="button"
//        data-bs-toggle="tooltip" title="Restablecer clave de acceso">
//        <i class="fas fa-key float-left"></i>
//    </button>
//}