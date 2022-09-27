$("#inputname").change(generadorUserName);
$("#inputapellido").change(generadorUserName);
$("#inputname").keydown(function (event) {
    return /[a-z, ]/i.test(event.key);
}).keyup(function (event) {
    let nombre = $("#inputname").val();
    if (nombre != ' ') {
        let countEspacio = 0;
        countEspacio = cuantasVecesAparece(nombre, ' ');
        if (event.key == ' ') {
            if (countEspacio > 1) {
                let text = $("#inputname").val();
                text = text.slice(0, -1);
                $("#inputname").val(text);
            }
        }
    } else {
        $("#inputname").val('');
    }
});
$("#inputapellido").keydown(function (event) {
    return /[a-z, ]/i.test(event.key);
}).keyup(function (event) {
    let apellido = $("#inputapellido").val();
    if (apellido != ' ') {
        countEspacio = cuantasVecesAparece(apellido, ' ');
        if (event.key == ' ') {
            if (countEspacio > 1) {
                let text = $("#inputapellido").val();
                text = text.slice(0, -1);
                $("#inputapellido").val(text);
            }
        }
    } else {
        $("#inputapellido").val('');
    }
});
//Boton para ver la contraseña
let view = false;
$("#viewPass").click(function () {
    verPassword();
});
$("#viewPass2").click(function () {
    verPassword();
});

function verPassword() {
    let inputclave = document.getElementById("inputclave");
    let inputclave2 = document.getElementById("inputclave2");
    if (inputclave.type == "password") {
        inputclave.type = "text";
        inputclave2.type = "text";
        $("#icon").removeClass("fa-eye");
        $("#icon").addClass("fa-eye-slash");
        $("#icon2").removeClass("fa-eye");
        $("#icon2").addClass("fa-eye-slash");
    } else {
        inputclave.type = "password";
        inputclave2.type = "password";
        $("#icon").removeClass("fa-eye-slash");
        $("#icon").addClass("fa-eye");
        $("#icon2").removeClass("fa-eye-slash");
        $("#icon2").addClass("fa-eye");
    }
}
//Quita los espacios al input
$("#inputusername").keyup(function () {
    let text = $("#inputusername").val();
    text = text.replace(' ', '');
    $("#inputusername").val(text);
});

function generadorUserName() {
    let nombre = $("#inputname").val().trim().toLowerCase();
    let apellido = $("#inputapellido").val().trim().toLowerCase();
    let nombreusuario = $("#inputusername").val();
    if (nombre != '' && apellido != '') {
        if (apellido.indexOf(' ') == -1) {
            nombreusuario = nombre[0] + apellido;
        } else {
            nombreusuario = nombre[0] + apellido.slice(0, apellido.indexOf(" "));
        }
        $("#inputusername").val(nombreusuario);
    }
}

function cuantasVecesAparece(cadena, caracter) {
    var indices = [];
    for (var i = 0; i < cadena.length; i++) {
        if (cadena[i] === caracter) indices.push(i);
    }
    return indices.length;
}