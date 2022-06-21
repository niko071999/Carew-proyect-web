<<<<<<< HEAD
﻿function moreDetail(id) {
    $.get('/Venta/getMoreDetail/' + id, function (data) {
        if (data != null) {
            abrirModal(data);
        }
        console.log(data);
    });
}
=======
﻿
>>>>>>> 963d1d0eb548451d125fa1f3118e08871080c9fd
